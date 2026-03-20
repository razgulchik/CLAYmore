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

        [Header("Modifier Pool")]
        public ModifierConfig[] modifierPool;  // all available modifiers assigned in inspector

        private ChestConfig _chestConfig;
        private List<ModifierConfig> _offered = new();
        private PlayerModifiersComponent _modifiers;

        private void Awake()
        {
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<ChestActivatedEvent>(OnChestActivated);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<ChestActivatedEvent>(OnChestActivated);
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
                    CoinsGiven = _chestConfig != null ? _chestConfig.coinsOnSkip : 0,
                });
                return;
            }

            // Pause game
            Time.timeScale = 0f;
            World.Current?.Events.Publish(new GamePausedEvent { IsPaused = true });

            // Populate cards
            for (int i = 0; i < cards.Length; i++)
            {
                if (i < _offered.Count)
                {
                    var mod = _offered[i];
                    _modifiers.Levels.TryGetValue(mod.name, out int currentLevel);
                    cards[i].gameObject.SetActive(true);
                    cards[i].Setup(mod, currentLevel + 1, OnCardChosen);
                }
                else
                {
                    cards[i].gameObject.SetActive(false);
                }
            }

            // Skip button
            int coinsOnSkip = _chestConfig != null ? _chestConfig.coinsOnSkip : 0;
            if (skipCoinsLabel != null)
                skipCoinsLabel.text = coinsOnSkip > 0 ? $"+{coinsOnSkip}" : "Пропустить";
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() => OnSkip(coinsOnSkip));

            panel.SetActive(true);
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

        private void OnSkip(int coins)
        {
            World.Current?.Events.Publish(new ModifierSkippedEvent { CoinsGiven = coins });
            Close();
        }

        private void Close()
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
            World.Current?.Events.Publish(new GamePausedEvent { IsPaused = false });
        }

        /// <summary>
        /// Filters out modifiers that have already reached maxLevel.
        /// </summary>
        private List<ModifierConfig> BuildAvailablePool()
        {
            var result = new List<ModifierConfig>();
            if (modifierPool == null) return result;

            foreach (var mod in modifierPool)
            {
                if (mod == null) continue;
                _modifiers.Levels.TryGetValue(mod.name, out int currentLevel);
                if (currentLevel < mod.maxLevel)
                    result.Add(mod);
            }
            return result;
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
