using CLAYmore.ECS;
using System.Text;
using TMPro;
using UnityEngine;

namespace CLAYmore
{
    public class JournalUI : MonoBehaviour
    {
        [Header("References")]
        public GameObject   panel;
        public StatsTracker statsTracker;

        [Header("Session Labels")]
        public TextMeshProUGUI timeLabel;
        public TextMeshProUGUI potsLabel;
        public TextMeshProUGUI coinsLabel;
        public TextMeshProUGUI modifiersLabel;
        public TextMeshProUGUI scoreLabel;

        [Header("Character Labels")]
        public TextMeshProUGUI maxHpLabel;
        public TextMeshProUGUI damageLabel;
        public TextMeshProUGUI speedLabel;

        [Header("Modifiers List")]
        public TextMeshProUGUI modifiersListLabel;

        private bool  _isOpen;
        private int   _maxHp;
        private int   _damage;
        private float _moveTime;

        private void Awake()
        {
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<JournalToggleEvent>(OnJournalToggle);
            World.Current?.Events.Subscribe<PlayerStatsChangedEvent>(OnStatsChanged);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<JournalToggleEvent>(OnJournalToggle);
            World.Current?.Events.Unsubscribe<PlayerStatsChangedEvent>(OnStatsChanged);
        }

        private void OnStatsChanged(PlayerStatsChangedEvent e)
        {
            _maxHp    = e.MaxHp;
            _damage   = e.Damage;
            _moveTime = e.MoveTime;
        }

        private void OnJournalToggle(JournalToggleEvent _)
        {
            if (_isOpen) Close();
            else         Open();
        }

        private void Open()
        {
            _isOpen = true;
            Refresh();
            panel.SetActive(true);
            PauseManager.Instance.Push();
        }

        private void Close()
        {
            _isOpen = false;
            panel.SetActive(false);
            PauseManager.Instance.Pop();
        }

        private void Refresh()
        {
            RefreshSession();
            RefreshCharacter();
            RefreshModifiersList();
        }

        private void RefreshSession()
        {
            if (statsTracker == null) return;

            int totalSec = (int)statsTracker.SessionTimePlayed;
            if (timeLabel      != null) timeLabel.text      = $"{totalSec / 60:00}:{totalSec % 60:00}";
            if (potsLabel      != null) potsLabel.text      = statsTracker.SessionPots.ToString();
            if (modifiersLabel != null) modifiersLabel.text = statsTracker.SessionModifiers.ToString();
            if (coinsLabel     != null) coinsLabel.text     = statsTracker.SessionCoins.ToString();
            if (scoreLabel     != null) scoreLabel.text     = statsTracker.CalculateScore().ToString();
        }

        private void RefreshCharacter()
        {
            if (maxHpLabel  != null) maxHpLabel.text  = _maxHp.ToString();
            if (damageLabel != null) damageLabel.text  = _damage.ToString();
            if (speedLabel  != null) speedLabel.text   = _moveTime.ToString("F2");
        }

        private void RefreshModifiersList()
        {
            if (modifiersListLabel == null || World.Current == null) return;

            foreach (var e in World.Current.Query<PlayerModifiersComponent>())
            {
                var levels = e.Get<PlayerModifiersComponent>().Levels;
                if (levels.Count == 0)
                {
                    modifiersListLabel.text = "—";
                    return;
                }

                var sb = new StringBuilder();
                foreach (var kv in levels)
                    sb.AppendLine($"{kv.Key}  Lv.{kv.Value}");
                modifiersListLabel.text = sb.ToString();
                return;
            }

            modifiersListLabel.text = "—";
        }
    }
}
