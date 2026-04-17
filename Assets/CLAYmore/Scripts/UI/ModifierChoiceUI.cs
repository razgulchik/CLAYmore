using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLAYmore.ECS;

namespace CLAYmore
{
    /// <summary>
    /// Roguelike modifier choice screen.
    /// Activated by ChestActivatedEvent. Shows N modifier cards + a skip option.
    /// Publishes ModifierChosenEvent or ModifierSkippedEvent, then unpauses the game.
    /// </summary>
    public class ModifierChoiceUI : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panel;
        public ModifierCardUI[] cards;     // pre-built card slots (assign in inspector)
        public Button skipButton;
        public TextMeshProUGUI skipCoinsLabel;

        [HideInInspector]
        public ModifierConfig[] modifierPool;  // set by Bootstrap
        [HideInInspector]
        public int coinsOnSkip;                // set by Bootstrap

        private ChestConfig _chestConfig;
        private List<ModifierConfig> _offered = new();
        private PlayerModifiersComponent _modifiers;

        private void Awake()
        {
            panel.SetActive(false);
        }

        private bool _isOpen;

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<ChestActivatedEvent>(OnChestActivated);
            World.Current?.Events.Subscribe<PlayerMoveInputEvent>(OnMoveInput);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<ChestActivatedEvent>(OnChestActivated);
            World.Current?.Events.Unsubscribe<PlayerMoveInputEvent>(OnMoveInput);
        }

        private void OnMoveInput(PlayerMoveInputEvent evt)
        {
            if (!_isOpen) return;

            var dir = evt.Direction;

            if (dir == new Vector2Int(-1, 0) && cards.Length > 0 && cards[0].gameObject.activeSelf)
                cards[0].button.onClick.Invoke();
            else if (dir == new Vector2Int(0, 1) && cards.Length > 1 && cards[1].gameObject.activeSelf)
                cards[1].button.onClick.Invoke();
            else if (dir == new Vector2Int(1, 0) && cards.Length > 2 && cards[2].gameObject.activeSelf)
                cards[2].button.onClick.Invoke();
            else if (dir == new Vector2Int(0, -1))
                skipButton.onClick.Invoke();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnChestActivated(ChestActivatedEvent evt)
        {
            _chestConfig = evt.ChestEntity.Get<ChestComponent>().Config;
            _modifiers   = GetPlayerModifiers();

            List<ModifierConfig> pool = BuildAvailablePool();
            _offered = PickRandom(pool, _chestConfig != null ? _chestConfig.choiceCount : 3);

            if (_offered.Count == 0)
            {
                // Nothing to offer — give coins and skip automatically
                World.Current?.Events.Publish(new ModifierSkippedEvent
                {
                    CoinsGiven = coinsOnSkip,
                });
                return;
            }

            // Pause game
            PauseManager.Instance.Push();

            // Populate cards
            int currentCoins = World.Current?.GetSystem<EconomySystem>()?.GetCoinCount() ?? 0;
            for (int i = 0; i < cards.Length; i++)
            {
                if (i < _offered.Count)
                {
                    var mod = _offered[i];
                    _modifiers.Levels.TryGetValue(mod.name, out int currentLevel);
                    cards[i].gameObject.SetActive(true);
                    cards[i].Setup(mod, currentLevel + 1, mod.price, currentCoins >= mod.price, OnCardChosen);
                }
                else
                {
                    cards[i].gameObject.SetActive(false);
                }
            }

            // Skip button
            if (skipCoinsLabel != null)
                skipCoinsLabel.text = coinsOnSkip > 0 ? $"+{coinsOnSkip}" : "Skip";
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() => OnSkip(coinsOnSkip));

            panel.SetActive(true);
            StartCoroutine(EnableInputAfterDelay(0.25f));
        }

        private IEnumerator EnableInputAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            _isOpen = true;
        }

        private void OnCardChosen(ModifierConfig modifier)
        {
            var economy = World.Current?.GetSystem<EconomySystem>();
            if (modifier.price > 0 && (economy == null || !economy.TrySpend(modifier.price)))
                return;

            _modifiers.Levels.TryGetValue(modifier.name, out int currentLevel);
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
            panel.SetActive(false);
            StartCoroutine(UnpauseNextFrame());
        }

        private IEnumerator UnpauseNextFrame()
        {
            yield return null;
            PauseManager.Instance.Pop();
        }

        /// <summary>
        /// Filters out modifiers that have reached maxLevel or whose IsAvailable() returns false.
        /// </summary>
        private List<ModifierConfig> BuildAvailablePool()
        {
            var result = new List<ModifierConfig>();
            if (modifierPool == null) return result;

            Entity player = GetPlayerEntity();

            foreach (var mod in modifierPool)
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

        /// <summary>
        /// Weighted random selection of up to 'count' distinct modifiers.
        /// </summary>
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
