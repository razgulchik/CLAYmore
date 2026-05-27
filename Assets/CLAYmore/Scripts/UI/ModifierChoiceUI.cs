using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using CLAYmore.ECS;

namespace CLAYmore
{
    public class ModifierChoiceUI : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panel;
        public ModifierCardUI[] cards;
        public GameObject skipButton;
        public TextMeshProUGUI skipCoinsLabel;

        [Header("Arrow Indicators")]
        public Image arrowDown;

        [Header("Category Backgrounds")]
        public ModifierCategoryBackground[] categoryBackgrounds;

        [Header("Hold Fill")]
        public Image holdFillImage;

        private const float HoldDelay  = 0.5f;
        private const float PulseMax   = 1.15f;
        private const float PulseSpeed = 15f;

        private Coroutine _holdCoroutine;
        private bool      _isOpen;

        private Vector2 _arrowDownOrigin;

        private ModifierConfig[]         _modifierPool;
        private int                      _expandersOnSkip;
        private List<ModifierConfig>     _offered = new();
        private PlayerModifiersComponent _modifiers;

        public void Init(ModifierConfig[] modifierPool, int expandersOnSkip)
        {
            _modifierPool    = modifierPool;
            _expandersOnSkip = expandersOnSkip;
        }

        private void Awake()
        {
            panel.SetActive(false);
            if (arrowDown != null) _arrowDownOrigin = arrowDown.rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<ChestActivatedEvent>(OnChestActivated);
            World.Current?.Events.Subscribe<PlayerMoveInputEvent>(OnMoveInput);
            World.Current?.Events.Subscribe<PlayerMoveHeldEvent>(OnMoveHeld);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<ChestActivatedEvent>(OnChestActivated);
            World.Current?.Events.Unsubscribe<PlayerMoveInputEvent>(OnMoveInput);
            World.Current?.Events.Unsubscribe<PlayerMoveHeldEvent>(OnMoveHeld);
        }

        private void OnMoveInput(PlayerMoveInputEvent evt)
        {
            if (!_isOpen) return;

            StopPending();

            var dir = evt.Direction;

            if (dir == new Vector2Int(-1, 0) && cards.Length > 0 && cards[0].gameObject.activeSelf)
            {
                cards[0].StartHold(() => cards[0].Select());
            }
            else if (dir == new Vector2Int(0, 1) && cards.Length > 1 && cards[1].gameObject.activeSelf)
            {
                cards[1].StartHold(() => cards[1].Select());
            }
            else if (dir == new Vector2Int(1, 0) && cards.Length > 2 && cards[2].gameObject.activeSelf)
            {
                cards[2].StartHold(() => cards[2].Select());
            }
            else if (dir == new Vector2Int(0, -1))
            {
                _holdCoroutine = StartCoroutine(HoldAndSkip());
                if (arrowDown != null)
                    arrowDown.transform.DOScale(PulseMax, Mathf.PI / PulseSpeed)
                        .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
            }
        }

        private void OnMoveHeld(PlayerMoveHeldEvent evt)
        {
            if (!_isOpen) return;
            if (evt.Direction == Vector2Int.zero)
                StopPending();
        }

        private IEnumerator HoldAndSkip()
        {
            float elapsed = 0f;
            while (elapsed < HoldDelay)
            {
                elapsed += Time.unscaledDeltaTime;
                if (holdFillImage != null) holdFillImage.fillAmount = Mathf.Clamp01(elapsed / HoldDelay);
                yield return null;
            }
            _holdCoroutine = null;
            if (holdFillImage != null) holdFillImage.fillAmount = 0f;
            StopSkipPulse();
            OnSkip(_expandersOnSkip);
        }

        private void StopSkipPulse()
        {
            if (arrowDown == null) return;
            DOTween.Kill(arrowDown.transform);
            arrowDown.transform.localScale = Vector3.one;
        }

        private void StopPending()
        {
            if (_holdCoroutine != null) { StopCoroutine(_holdCoroutine); _holdCoroutine = null; }
            if (holdFillImage != null) holdFillImage.fillAmount = 0f;
            StopSkipPulse();
            if (arrowDown != null) { DOTween.Kill(arrowDown.rectTransform); arrowDown.rectTransform.anchoredPosition = _arrowDownOrigin; }
            foreach (var card in cards)
                if (card != null) card.StopHold();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnChestActivated(ChestActivatedEvent evt)
        {
            _modifiers = GetPlayerModifiers();

            List<ModifierConfig> pool = BuildAvailablePool();
            _offered = PickRandom(pool, 3);

            if (_offered.Count == 0)
            {
                World.Current?.Events.Publish(new ModifierSkippedEvent
                {
                    ExpandersGiven = _expandersOnSkip,
                });
                return;
            }

            PauseManager.Instance.Push();

            for (int i = 0; i < cards.Length; i++)
            {
                if (i < _offered.Count)
                {
                    var mod = _offered[i];
                    _modifiers.Levels.TryGetValue(mod.name, out int currentLevel);
                    cards[i].gameObject.SetActive(true);
                    cards[i].Setup(mod, currentLevel + 1, OnCardChosen, GetBackground(mod.category));
                }
                else
                {
                    cards[i].gameObject.SetActive(false);
                }
            }

            if (skipCoinsLabel != null)
                skipCoinsLabel.text = _expandersOnSkip > 0 ? $"+{_expandersOnSkip}" : "Skip";

            panel.SetActive(true);
            _isOpen = true;
        }

        private void OnCardChosen(ModifierConfig modifier)
        {
            _modifiers.Levels.TryGetValue(modifier.name, out int currentLevel);
            World.Current?.Events.Publish(new ModifierChosenEvent
            {
                Modifier = modifier,
                NewLevel = currentLevel + 1,
            });
            Close();
        }

        public void OnSkipClicked() => OnSkip(_expandersOnSkip);

        private void OnSkip(int expanders)
        {
            World.Current?.Events.Publish(new ModifierSkippedEvent { ExpandersGiven = expanders });
            Close();
        }

        private void Close()
        {
            _isOpen = false;
            StopPending();
            panel.SetActive(false);
            StartCoroutine(UnpauseNextFrame());
        }

        private IEnumerator UnpauseNextFrame()
        {
            yield return null;
            PauseManager.Instance.Pop();
        }

        private List<ModifierConfig> BuildAvailablePool()
        {
            var result = new List<ModifierConfig>();
            if (_modifierPool == null) return result;

            Entity player = GetPlayerEntity();

            foreach (var mod in _modifierPool)
            {
                if (mod == null) continue;
                _modifiers.Levels.TryGetValue(mod.name, out int currentLevel);
                if (currentLevel < mod.maxLevel && mod.IsAvailable(player))
                    result.Add(mod);
            }
            return result;
        }

        private Entity GetPlayerEntity()
        {
            if (World.Current == null) return null;
            foreach (var e in World.Current.Query<CLAYmore.ECS.PlayerStatsComponent>())
                return e;
            return null;
        }

        private List<ModifierConfig> PickRandom(List<ModifierConfig> pool, int count)
        {
            var result    = new List<ModifierConfig>();
            var remaining = new List<ModifierConfig>(pool);

            for (int i = 0; i < count && remaining.Count > 0; i++)
            {
                float total = 0f;
                foreach (var m in remaining) total += m.spawnWeight;

                float roll = Random.Range(0f, total);
                float acc  = 0f;
                ModifierConfig picked = remaining[remaining.Count - 1];
                foreach (var m in remaining)
                {
                    acc += m.spawnWeight;
                    if (roll < acc) { picked = m; break; }
                }

                result.Add(picked);
                remaining.Remove(picked);
            }
            return result;
        }

        private PlayerModifiersComponent GetPlayerModifiers()
        {
            if (World.Current == null) return new PlayerModifiersComponent();
            foreach (var e in World.Current.Query<CLAYmore.ECS.PlayerModifiersComponent>())
                return e.Get<CLAYmore.ECS.PlayerModifiersComponent>();
            return new PlayerModifiersComponent();
        }

        private Sprite GetBackground(ModifierCategory category)
        {
            if (categoryBackgrounds == null) return null;
            foreach (var b in categoryBackgrounds)
                if (b.category == category) return b.sprite;
            return null;
        }
    }
}
