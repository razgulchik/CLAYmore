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
        public Button skipButton;
        public TextMeshProUGUI skipCoinsLabel;

        [Header("Arrow Indicators")]
        public Image arrowLeft;
        public Image arrowUp;
        public Image arrowRight;
        public Image arrowDown;

        [Header("Hold Fill")]
        public Image holdFillImage;

        private const float HoldDelay  = 0.5f;
        private const float PulseMax   = 1.15f;
        private const float PulseSpeed = 15f;

        private Coroutine _holdCoroutine;
        private bool      _isOpen;

        private Vector2 _arrowLeftOrigin;
        private Vector2 _arrowUpOrigin;
        private Vector2 _arrowRightOrigin;
        private Vector2 _arrowDownOrigin;

        private ModifierConfig[]      _modifierPool;
        private int                   _coinsOnSkip;
        private List<ModifierConfig>  _offered = new();
        private PlayerModifiersComponent _modifiers;

        public void Init(ModifierConfig[] modifierPool, int coinsOnSkip)
        {
            _modifierPool = modifierPool;
            _coinsOnSkip  = coinsOnSkip;
        }

        private void Awake()
        {
            panel.SetActive(false);
            if (arrowLeft  != null) _arrowLeftOrigin  = arrowLeft.rectTransform.anchoredPosition;
            if (arrowUp    != null) _arrowUpOrigin    = arrowUp.rectTransform.anchoredPosition;
            if (arrowRight != null) _arrowRightOrigin = arrowRight.rectTransform.anchoredPosition;
            if (arrowDown  != null) _arrowDownOrigin  = arrowDown.rectTransform.anchoredPosition;
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
            System.Action action    = null;
            Image         arrow     = null;
            bool          canAfford = true;

            if (dir == new Vector2Int(-1, 0) && cards.Length > 0 && cards[0].gameObject.activeSelf)
            {
                action = () => cards[0].button.onClick.Invoke();
                arrow  = arrowLeft;
                canAfford = cards[0].button.interactable;
            }
            else if (dir == new Vector2Int(0, 1) && cards.Length > 1 && cards[1].gameObject.activeSelf)
            {
                action = () => cards[1].button.onClick.Invoke();
                arrow  = arrowUp;
                canAfford = cards[1].button.interactable;
            }
            else if (dir == new Vector2Int(1, 0) && cards.Length > 2 && cards[2].gameObject.activeSelf)
            {
                action = () => cards[2].button.onClick.Invoke();
                arrow  = arrowRight;
                canAfford = cards[2].button.interactable;
            }
            else if (dir == new Vector2Int(0, -1))
            {
                action = () => skipButton.onClick.Invoke();
                arrow  = arrowDown;
            }

            if (action == null) return;

            if (!canAfford)
            {
                if (arrow != null)
                {
                    var origin = arrow == arrowLeft  ? _arrowLeftOrigin  :
                                 arrow == arrowUp    ? _arrowUpOrigin    :
                                 arrow == arrowRight ? _arrowRightOrigin :
                                                       _arrowDownOrigin;
                    arrow.rectTransform.DOShakeAnchorPos(0.5f, strength: 10f, vibrato: 20, randomness: 90f)
                        .SetUpdate(true)
                        .OnComplete(() => arrow.rectTransform.anchoredPosition = origin);
                }
                return;
            }

            _holdCoroutine = StartCoroutine(HoldAndSelect(action));
            if (arrow != null)
                arrow.transform.DOScale(PulseMax, Mathf.PI / PulseSpeed)
                    .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
        }

        private void OnMoveHeld(PlayerMoveHeldEvent evt)
        {
            if (!_isOpen) return;
            if (evt.Direction == Vector2Int.zero)
                StopPending();
        }

        private IEnumerator HoldAndSelect(System.Action action)
        {
            float elapsed = 0f;
            while (elapsed < HoldDelay)
            {
                elapsed += Time.unscaledDeltaTime;
                SetMaskFill(elapsed / HoldDelay);
                yield return null;
            }
            _holdCoroutine = null;
            SetMaskFill(0f);
            StopPulse();
            action();
        }

        private void SetMaskFill(float t)
        {
            if (holdFillImage == null) return;
            holdFillImage.fillAmount = Mathf.Clamp01(t);
        }

        private void StopPulse()
        {
            Image[] arrows = { arrowLeft, arrowUp, arrowRight, arrowDown };
            foreach (var a in arrows)
            {
                if (a == null) continue;
                DOTween.Kill(a.transform);
                a.transform.localScale = Vector3.one;
            }
        }

        private void StopPending()
        {
            if (_holdCoroutine != null) { StopCoroutine(_holdCoroutine); _holdCoroutine = null; }
            SetMaskFill(0f);
            StopPulse();
            if (arrowLeft  != null) { DOTween.Kill(arrowLeft.rectTransform);  arrowLeft.rectTransform.anchoredPosition  = _arrowLeftOrigin; }
            if (arrowUp    != null) { DOTween.Kill(arrowUp.rectTransform);    arrowUp.rectTransform.anchoredPosition    = _arrowUpOrigin; }
            if (arrowRight != null) { DOTween.Kill(arrowRight.rectTransform); arrowRight.rectTransform.anchoredPosition = _arrowRightOrigin; }
            if (arrowDown  != null) { DOTween.Kill(arrowDown.rectTransform);  arrowDown.rectTransform.anchoredPosition  = _arrowDownOrigin; }
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnChestActivated(ChestActivatedEvent evt)
        {
            _modifiers = GetPlayerModifiers();

            List<ModifierConfig> pool = BuildAvailablePool();
            _offered = PickRandom(pool, 3);

            if (_offered.Count == 0)
            {
                // Nothing to offer — give coins and skip automatically
                World.Current?.Events.Publish(new ModifierSkippedEvent
                {
                    CoinsGiven = _coinsOnSkip,
                });
                return;
            }

            // Pause game
            PauseManager.Instance.Push();

            // Populate cards
            int currentCoins = World.Current?.GetSystem<EconomySystem>()?.GetCoinCount() ?? 0;
            float discount = GetPlayerPriceDiscount();
            for (int i = 0; i < cards.Length; i++)
            {
                if (i < _offered.Count)
                {
                    var mod = _offered[i];
                    _modifiers.Levels.TryGetValue(mod.name, out int currentLevel);
                    cards[i].gameObject.SetActive(true);
                    int nextPrice = Mathf.FloorToInt(mod.GetPrice(currentLevel + 1) * (1f - discount));
                    cards[i].Setup(mod, currentLevel + 1, nextPrice, currentCoins >= nextPrice, OnCardChosen);
                }
                else
                {
                    cards[i].gameObject.SetActive(false);
                }
            }

            // Skip button
            if (skipCoinsLabel != null)
                skipCoinsLabel.text = _coinsOnSkip > 0 ? $"+{_coinsOnSkip}" : "Skip";
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() => OnSkip(_coinsOnSkip));

            panel.SetActive(true);
            _isOpen = true;
        }

        private void OnCardChosen(ModifierConfig modifier)
        {
            var economy = World.Current?.GetSystem<EconomySystem>();
            _modifiers.Levels.TryGetValue(modifier.name, out int currentLevel);
            int price = Mathf.FloorToInt(modifier.GetPrice(currentLevel + 1) * (1f - GetPlayerPriceDiscount()));
            if (price > 0 && (economy == null || !economy.TrySpend(price)))
                return;
            World.Current?.Events.Publish(new ModifierChosenEvent
            {
                Modifier = modifier,
                NewLevel = currentLevel + 1,
            });
            Close();
        }

        private void OnSkip(int coins)
        {
            World.Current?.Events.Publish(new ModifierSkippedEvent { CoinsGiven = coins });
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

        private float GetPlayerPriceDiscount()
        {
            if (World.Current == null) return 0f;
            foreach (var e in World.Current.Query<CLAYmore.ECS.PlayerStatsComponent>())
                return e.Get<CLAYmore.ECS.PlayerStatsComponent>().PriceDiscount;
            return 0f;
        }

        private List<ModifierConfig> PickRandom(List<ModifierConfig> pool, int count)
        {
            var result  = new List<ModifierConfig>();
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
    }
}
