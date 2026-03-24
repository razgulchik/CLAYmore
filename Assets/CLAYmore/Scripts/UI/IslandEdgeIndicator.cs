using CLAYmore.ECS;
using TMPro;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Highlights the island edge and shows the expansion cost when the player
    /// stands adjacent to a boundary.
    ///
    /// Scene setup — create 4 child GameObjects (Up / Down / Left / Right), each with:
    ///   - SpriteRenderer  : a simple white 1x1 sprite (will be scaled to fit the edge)
    ///   - TextMeshPro     : price label, anchored at the center of the edge
    /// Assign them to the four slots below.
    /// </summary>
    public class IslandEdgeIndicator : MonoBehaviour
    {
        [Header("References")]
        public IslandGenerator islandGenerator;
        public Economy         economy;

        [Header("Edge Lines (one per direction)")]
        public EdgeLine up;
        public EdgeLine down;
        public EdgeLine left;
        public EdgeLine right;

        [Header("Appearance")]
        public Color canAffordColor    = new Color(1f,   0.85f, 0f,   0.7f); // жёлтый
        public Color cannotAffordColor = new Color(0.4f, 0.4f,  0.4f, 0.5f); // серый

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<PlayerTileChangedEvent>(OnPlayerTileChanged);
            if (economy != null) economy.OnChanged += OnEconomyChanged;
            HideAll();
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<PlayerTileChangedEvent>(OnPlayerTileChanged);
            if (economy != null) economy.OnChanged -= OnEconomyChanged;
            HideAll();
        }

        private void OnPlayerTileChanged(PlayerTileChangedEvent _) => Refresh();
        private void OnEconomyChanged(int _)                       => Refresh();

        // ── Private ───────────────────────────────────────────────────────────

        private void Refresh()
        {
            if (islandGenerator == null) return;

            Vector3 playerWorld = islandGenerator.GetCellCenter(islandGenerator.GetPlayerCell());
            int     cost        = islandGenerator.ExpansionCost;
            bool    canAfford   = economy != null && economy.Coins >= cost;
            Color   color       = canAfford ? canAffordColor : cannotAffordColor;

            RefreshLine(up,    playerWorld, Vector2Int.up,    cost, color);
            RefreshLine(down,  playerWorld, Vector2Int.down,  cost, color);
            RefreshLine(left,  playerWorld, Vector2Int.left,  cost, color);
            RefreshLine(right, playerWorld, Vector2Int.right, cost, color);
        }

        private void RefreshLine(EdgeLine line, Vector3 playerWorld,
                                  Vector2Int dir, int cost, Color color)
        {
            if (line == null || line.root == null) return;

            bool atEdge = islandGenerator.IsBlockedByEdge(playerWorld, dir);
            line.root.SetActive(atEdge);
            if (!atEdge) return;

            var (center, length) = islandGenerator.GetEdgeLineWorld(dir);

            EdgeLineConfig cfg = line.root.GetComponent<EdgeLineConfig>();
            float edgeOffset    = cfg != null ? cfg.edgeOffset    : 0.5f;
            float lineThickness = cfg != null ? cfg.lineThickness : 0.25f;
            float lengthMargin  = cfg != null ? cfg.lengthMargin  : 0f;
            float centerOffset  = cfg != null ? cfg.centerOffset  : 0f;

            // Perpendicular to dir: for Up/Down → X axis, for Left/Right → Y axis
            var perp = new Vector2(-dir.y, dir.x);

            // Position at the edge center + offset outward + center offset along edge
            float z = line.root.transform.position.z;
            line.root.transform.position = new Vector3(
                center.x + dir.x * edgeOffset + perp.x * centerOffset,
                center.y + dir.y * edgeOffset + perp.y * centerOffset,
                z);

            // Scale the sprite to match the edge length and desired thickness
            if (line.spriteRenderer != null)
            {
                line.spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                bool  horizontal   = dir.y != 0;
                float trimmedLength = Mathf.Max(0f, length - 2f * lengthMargin);
                line.spriteRenderer.size = horizontal
                    ? new Vector2(trimmedLength, lineThickness)
                    : new Vector2(lineThickness, trimmedLength);
                line.spriteRenderer.transform.localScale = Vector3.one;
                line.spriteRenderer.color = color;
            }

            // Price label centered on the line
            if (line.priceLabel != null)
            {
                line.priceLabel.text  = cost.ToString();
                line.priceLabel.color = color;
            }
        }

        private void HideAll()
        {
            up?.root?.SetActive(false);
            down?.root?.SetActive(false);
            left?.root?.SetActive(false);
            right?.root?.SetActive(false);
        }
    }

    [System.Serializable]
    public class EdgeLine
    {
        public GameObject     root;            // включается/выключается целиком
        public SpriteRenderer spriteRenderer;  // масштабируется по длине края
        public TextMeshPro    priceLabel;       // текст с ценой в центре линии
    }
}
