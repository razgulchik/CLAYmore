using CLAYmore.ECS;
using TMPro;
using UnityEngine;

namespace CLAYmore
{
    public class WaveCountdownUI : MonoBehaviour
    {
        public TextMeshProUGUI timerLabel;

        private float _timeRemaining;

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<WaveChangedEvent>(OnWaveChanged);
            World.Current?.Events.Subscribe<WaveClearedEvent>(OnWaveCleared);
            World.Current?.Events.Subscribe<GameOverEvent>(OnGameOver);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<WaveChangedEvent>(OnWaveChanged);
            World.Current?.Events.Unsubscribe<WaveClearedEvent>(OnWaveCleared);
            World.Current?.Events.Unsubscribe<GameOverEvent>(OnGameOver);
        }

        private void Update()
        {
            if (timerLabel == null) return;

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining < 0f) _timeRemaining = 0f;

            int total   = Mathf.CeilToInt(_timeRemaining);
            int minutes = total / 60;
            int seconds = total % 60;
            timerLabel.text = $"{minutes:D2}:{seconds:D2}";
        }

        private void OnWaveChanged(WaveChangedEvent evt)
        {
            if (evt.Config.waveDuration <= 0f)
            {
                enabled = false;
                return;
            }

            _timeRemaining = evt.Config.waveDuration;
            enabled = true;
        }

        private void OnWaveCleared(WaveClearedEvent _) => enabled = false;
        private void OnGameOver(GameOverEvent _)       => enabled = false;
    }
}
