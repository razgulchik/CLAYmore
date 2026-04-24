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
        private Tween      _shakeTween;
        private Vector3    _shakeOrigin;

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
            SetFill(_heldDirection, Mathf.Clamp01(_holdTimer / holdDuration));

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
            SetFill(dir, 0f);
            StartShake(dir);
        }

        private void CancelHold()
        {
            if (!_holdActive) return;
            _holdActive = false;
            _holdTimer  = 0f;
            StopShake(_heldDirection);
            SetFill(_heldDirection, -1f);
        }

        private void CompleteHold()
        {
            _holdActive = false;
            _holdTimer  = 0f;
            StopShake(_heldDirection);
            islandGenerator.TryExpand(_heldDirection);
            SetFill(_heldDirection, -1f);
        }

        private void StartShake(Vector2Int dir)
        {
            var t = GetLine(dir)?.holdBar?.transform;
            if (t == null) return;
            _shakeTween?.Kill();
            _shakeOrigin = t.localPosition;
            _shakeTween = DOTween.Sequence()
                .Append(t.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, fadeOut: true))
                .SetLoops(-1);
        }

        private void StopShake(Vector2Int dir)
        {
            _shakeTween?.Kill();
            _shakeTween = null;
            var t = GetLine(dir)?.holdBar?.transform;
            if (t != null) t.localPosition = _shakeOrigin;
        }

        private void SetFill(Vector2Int dir, float progress)
        {
            var line = GetLine(dir);
            if (line?.holdBar == null) return;

            if (progress < 0f)
            {
                line.holdBar.SetActive(false);
            }
            else
            {
                line.holdBar.SetActive(true);
                if (line.holdFill != null)
                {
                    var s = line.holdFill.localScale;
                    s.x = progress;
                    s.y = progress;
                    line.holdFill.localScale = s;
                }
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

                EdgeLineConfig cfg    = line.root.GetComponent<EdgeLineConfig>();
                float edgeOffset      = cfg != null ? cfg.edgeOffset   : 0.5f;
                float centerOffset    = cfg != null ? cfg.centerOffset  : 0f;
                var   perp            = new Vector2(-dir.y, dir.x);

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
        public GameObject     holdBar;   // включается при удержании
        public Transform      holdFill;  // Fill круг — scale (0,0,1) → (1,1,1)
    }
}
