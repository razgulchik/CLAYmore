using CLAYmore.ECS;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CLAYmore
{
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
        public Color canAffordColor    = new Color(1f,   0.85f, 0f,   0.7f);
        public Color cannotAffordColor = new Color(0.4f, 0.4f,  0.4f, 0.5f);

        [Header("Expansion Hold")]
        [Min(0.1f)] public float holdDuration  = 3f;
        public float shakeStrength = 0.04f;
        public float shakeDuration = 0.3f;
        public int   shakeVibrato  = 20;

        // ── Internal state ────────────────────────────────────────────────────

        private int        _coinBalance;
        private Vector2Int _heldDirection;
        private float      _holdTimer;
        private bool       _holdActive;
        private Tween      _ghostShakeTween;
        private Vector3    _ghostShakeOrigin;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<PlayerTileChangedEvent>(OnPlayerTileChanged);
            World.Current?.Events.Subscribe<CoinBalanceChangedEvent>(OnCoinBalanceChanged);
            World.Current?.Events.Subscribe<PlayerMoveHeldEvent>(OnMoveHeld);
            HideAll();
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<PlayerTileChangedEvent>(OnPlayerTileChanged);
            World.Current?.Events.Unsubscribe<CoinBalanceChangedEvent>(OnCoinBalanceChanged);
            World.Current?.Events.Unsubscribe<PlayerMoveHeldEvent>(OnMoveHeld);
            CancelHold();
            HideAll();
        }

        private void Update()
        {
            if (!_holdActive) return;

            _holdTimer += Time.deltaTime;
            PublishProgress(Mathf.Clamp01(_holdTimer / holdDuration));

            if (_holdTimer >= holdDuration)
                CompleteHold();
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void OnPlayerTileChanged(PlayerTileChangedEvent _)    => Refresh();
        private void OnCoinBalanceChanged(CoinBalanceChangedEvent e) { _coinBalance = e.NewBalance; Refresh(); }

        private void OnMoveHeld(PlayerMoveHeldEvent evt)
        {
            if (evt.Direction == Vector2Int.zero) { CancelHold(); return; }

            Vector3 playerWorld = islandGenerator.GetCellCenter(islandGenerator.GetPlayerCell());
            if (islandGenerator.CanExpand(playerWorld, evt.Direction))
                StartHold(evt.Direction);
        }

        // ── Hold logic ────────────────────────────────────────────────────────

        private void StartHold(Vector2Int dir)
        {
            _heldDirection = dir;
            _holdTimer     = 0f;
            _holdActive    = true;
            PublishProgress(0f);
            StartGhostShake();
            RedrawGhostForDirection(dir);
            HideInactiveEdges(dir);
        }

        private void CancelHold()
        {
            if (!_holdActive) return;
            _holdActive = false;
            _holdTimer  = 0f;
            StopGhostShake();
            PublishProgress(-1f);
            Refresh();
        }

        private void CompleteHold()
        {
            _holdActive = false;
            _holdTimer  = 0f;
            StopGhostShake();
            PublishProgress(-1f);
            islandGenerator.TryExpand(_heldDirection);
        }

        private void PublishProgress(float progress)
        {
            World.Current?.Events.Publish(new ExpansionHoldProgressEvent { Progress = progress });
        }

        private void StartGhostShake()
        {
            if (ghostTilemap == null) return;
            _ghostShakeTween?.Kill();
            _ghostShakeOrigin = ghostTilemap.transform.localPosition;
            _ghostShakeTween = DOTween.Sequence()
                .Append(ghostTilemap.transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, fadeOut: true))
                .SetLoops(-1);
        }

        private void StopGhostShake()
        {
            _ghostShakeTween?.Kill();
            _ghostShakeTween = null;
            if (ghostTilemap != null) ghostTilemap.transform.localPosition = _ghostShakeOrigin;
        }

        private void HideInactiveEdges(Vector2Int activeDir)
        {
            var active = GetLine(activeDir);
            foreach (var line in new[] { up, down, left, right })
                if (line != active && line != null && line.root != null)
                    line.root.SetActive(false);
        }

        private void RedrawGhostForDirection(Vector2Int dir)
        {
            if (ghostTilemap == null) return;
            ghostTilemap.ClearAllTiles();
            var ghostCells = islandGenerator.GetExpansionGhostCells(dir);
            foreach (var (cell, type, isLight) in ghostCells)
            {
                var tile = islandGenerator.tileSet.GetTile(type, isLight);
                if (tile != null) ghostTilemap.SetTile(cell, tile);
            }
        }

        // ── Ghost / label refresh ─────────────────────────────────────────────

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

        private bool RefreshLine(EdgeLine line, Vector3 playerWorld,
                                 Vector2Int dir, int cost, Color labelColor)
        {
            if (line == null || line.root == null) return false;

            bool atEdge = islandGenerator.IsBlockedByEdge(playerWorld, dir);
            line.root.SetActive(atEdge);
            if (!atEdge) return false;

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

            if (line.priceLabel != null)
            {
                var (center, _) = islandGenerator.GetEdgeLineWorld(dir);

                EdgeLineConfig cfg = line.root.GetComponent<EdgeLineConfig>();
                float edgeOffset   = cfg != null ? cfg.edgeOffset  : 0.5f;
                float centerOffset = cfg != null ? cfg.centerOffset : 0f;
                var   perp         = new Vector2(-dir.y, dir.x);

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

        private EdgeLine GetLine(Vector2Int dir)
        {
            if (dir == Vector2Int.up)    return up;
            if (dir == Vector2Int.down)  return down;
            if (dir == Vector2Int.left)  return left;
            if (dir == Vector2Int.right) return right;
            return null;
        }
    }

    [System.Serializable]
    public class EdgeLine
    {
        public GameObject     root;
        public SpriteRenderer spriteRenderer;
        public TextMeshPro    priceLabel;
    }
}
