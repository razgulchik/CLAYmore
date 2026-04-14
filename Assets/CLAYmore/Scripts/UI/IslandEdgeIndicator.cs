using CLAYmore.ECS;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CLAYmore
{
    /// <summary>
    /// Shows a ghost preview of the island tiles that would appear when the player
    /// stands adjacent to a boundary and the island expands in that direction.
    ///
    /// Scene setup:
    ///   - Create a second Tilemap (e.g. "GhostTilemap") as a child of the same Grid.
    ///     Set its Sorting Layer / Order so it renders above water but below gameplay objects.
    ///   - Assign that Tilemap to the ghostTilemap slot below.
    ///   - The four EdgeLine child GameObjects (Up / Down / Left / Right) only need
    ///     a TextMeshPro component for the price label — the SpriteRenderer is no longer used.
    /// </summary>
    public class IslandEdgeIndicator : MonoBehaviour
    {
        [Header("References")]
        public IslandGenerator islandGenerator;

        [Header("Edge Lines (one per direction)")]
        public EdgeLine up;
        public EdgeLine down;
        public EdgeLine left;
        public EdgeLine right;

        [Header("Ghost Preview")]
        public Tilemap ghostTilemap;
        public Color ghostCanAffordColor    = new(1f,   1f,   1f,   0.45f);
        public Color ghostCannotAffordColor = new(0.4f, 0.4f, 0.4f, 0.3f);

        [Header("Price Label Colors")]
        public Color canAffordColor    = new Color(1f,   0.85f, 0f,   0.7f); // жёлтый
        public Color cannotAffordColor = new Color(0.4f, 0.4f,  0.4f, 0.5f); // серый

        // ── Internal state ────────────────────────────────────────────────────

        private int _coinBalance;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<PlayerTileChangedEvent>(OnPlayerTileChanged);
            World.Current?.Events.Subscribe<CoinBalanceChangedEvent>(OnCoinBalanceChanged);
            HideAll();
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<PlayerTileChangedEvent>(OnPlayerTileChanged);
            World.Current?.Events.Unsubscribe<CoinBalanceChangedEvent>(OnCoinBalanceChanged);
            HideAll();
        }

        private void OnPlayerTileChanged(PlayerTileChangedEvent _)       => Refresh();
        private void OnCoinBalanceChanged(CoinBalanceChangedEvent e) { _coinBalance = e.NewBalance; Refresh(); }

        // ── Private ───────────────────────────────────────────────────────────

        private void Refresh()
        {
            if (islandGenerator == null) return;

            if (ghostTilemap != null) ghostTilemap.ClearAllTiles();

            Vector3 playerWorld = islandGenerator.GetCellCenter(islandGenerator.GetPlayerCell());
            int     cost        = islandGenerator.ExpansionCost;
            bool    canAfford   = _coinBalance >= cost;
            Color   labelColor  = canAfford ? canAffordColor : cannotAffordColor;

            bool anyActive = false;
            anyActive |= RefreshLine(up,    playerWorld, Vector2Int.up,    cost, labelColor);
            anyActive |= RefreshLine(down,  playerWorld, Vector2Int.down,  cost, labelColor);
            anyActive |= RefreshLine(left,  playerWorld, Vector2Int.left,  cost, labelColor);
            anyActive |= RefreshLine(right, playerWorld, Vector2Int.right, cost, labelColor);

            if (anyActive && ghostTilemap != null)
                ghostTilemap.color = canAfford ? ghostCanAffordColor : ghostCannotAffordColor;
        }

        /// <returns>True if the line is active (player is at this edge).</returns>
        private bool RefreshLine(EdgeLine line, Vector3 playerWorld,
                                 Vector2Int dir, int cost, Color labelColor)
        {
            if (line == null || line.root == null) return false;

            bool atEdge = islandGenerator.IsBlockedByEdge(playerWorld, dir);
            line.root.SetActive(atEdge);
            if (!atEdge) return false;

            // Populate ghost tiles for this direction
            if (ghostTilemap != null)
            {
                var ghostCells = islandGenerator.GetExpansionGhostCells(dir);
                foreach (var (cell, type, isLight) in ghostCells)
                {
                    var tile = islandGenerator.tileSet.GetTile(type, isLight);
                    if (tile != null)
                        ghostTilemap.SetTile(cell, tile);
                }
            }

            // Keep the price label centred on the edge
            if (line.priceLabel != null)
            {
                var (center, _) = islandGenerator.GetEdgeLineWorld(dir);

                EdgeLineConfig cfg = line.root.GetComponent<EdgeLineConfig>();
                float edgeOffset  = cfg != null ? cfg.edgeOffset  : 0.5f;
                float centerOffset = cfg != null ? cfg.centerOffset : 0f;
                var   perp        = new Vector2(-dir.y, dir.x);

                float z = line.root.transform.position.z;
                line.root.transform.position = new Vector3(
                    center.x + dir.x * edgeOffset + perp.x * centerOffset,
                    center.y + dir.y * edgeOffset + perp.y * centerOffset,
                    z);

                line.priceLabel.text  = cost.ToString();
                line.priceLabel.color = labelColor;
            }

            return true;
        }

        private void HideAll()
        {
            up?.root?.SetActive(false);
            down?.root?.SetActive(false);
            left?.root?.SetActive(false);
            right?.root?.SetActive(false);
            if (ghostTilemap != null) ghostTilemap.ClearAllTiles();
        }
    }

    [System.Serializable]
    public class EdgeLine
    {
        public GameObject     root;            // включается/выключается целиком
        public SpriteRenderer spriteRenderer;  // больше не используется, можно убрать из сцены
        public TextMeshPro    priceLabel;       // текст с ценой в центре линии
    }
}
