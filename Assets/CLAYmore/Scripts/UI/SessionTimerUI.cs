using CLAYmore.ECS;
using TMPro;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Displays the remaining session time as MM:SS.
    /// Assign timerLabel in the Inspector.
    /// </summary>
    public class SessionTimerUI : MonoBehaviour
    {
        public TextMeshProUGUI timerLabel;

        private SessionTimerSystem _timerSystem;

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<GameOverEvent>(OnGameOver);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<GameOverEvent>(OnGameOver);
        }

        private void Update()
        {
            if (_timerSystem == null)
                _timerSystem = World.Current?.GetSystem<SessionTimerSystem>();

            if (_timerSystem == null || timerLabel == null) return;

            float elapsed = _timerSystem.TimeElapsed;
            int minutes = (int)(elapsed / 60f);
            int seconds = (int)(elapsed % 60f);
            timerLabel.text = $"{minutes:D2}:{seconds:D2}";
        }

        private void OnGameOver(GameOverEvent _)
        {
            enabled = false;
        }
    }
}
