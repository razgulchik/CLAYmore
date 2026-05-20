using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Advances through waves sequentially. Each wave runs for waveDuration seconds
    /// (0 = no limit). When all waves are exhausted, publishes FinalCleanupEvent.
    /// </summary>
    public class SessionTimerSystem : ISystem
    {
        private readonly WaveConfig[] _waves;

        private World _world;
        private float _elapsed;
        private float _waveElapsed;
        private int   _currentWaveIndex = -1;
        private bool  _active = true;
        private bool  _started;

        public float TimeElapsed    => _elapsed;
        public int   CurrentWaveIndex => _currentWaveIndex;

        public SessionTimerSystem(WaveConfig[] waves)
        {
            _waves = (waves != null && waves.Length > 0)
                ? (WaveConfig[])waves.Clone()
                : System.Array.Empty<WaveConfig>();
        }

        public void Initialize(World world)
        {
            _world = world;
            world.Events.Subscribe<GameOverEvent>(OnGameOver);
            world.Events.Subscribe<WaveClearedEvent>(OnWaveCleared);
        }

        public void Tick(float deltaTime)
        {
            if (!_active) return;

            _elapsed     += deltaTime;
            _waveElapsed += deltaTime;

            if (!_started)
            {
                _started = true;
                AdvanceWave();
                return;
            }

            if (_currentWaveIndex < 0 || _currentWaveIndex >= _waves.Length) return;

            var wave = _waves[_currentWaveIndex];
            if (wave.waveDuration > 0f && _waveElapsed >= wave.waveDuration)
                AdvanceWave();
        }

        private void OnWaveCleared(WaveClearedEvent _)
        {
            if (!_active) return;
            AdvanceWave();
        }

        private void AdvanceWave()
        {
            _currentWaveIndex++;
            _waveElapsed = 0f;

            if (_currentWaveIndex >= _waves.Length)
            {
                _active = false;
                _world.Events.Publish(new FinalCleanupEvent());
                Debug.Log($"[SessionTimer] All waves done — t={_elapsed:F0}s — FinalCleanup");
                return;
            }

            var wave = _waves[_currentWaveIndex];
            _world.Events.Publish(new WaveChangedEvent
            {
                WaveIndex  = _currentWaveIndex,
                Config     = wave,
                IsLastWave = _currentWaveIndex == _waves.Length - 1
            });
            Debug.Log($"[SessionTimer] Wave {_currentWaveIndex} — t={_elapsed:F0}s, duration={wave.waveDuration:F0}s");
        }

        private void OnGameOver(GameOverEvent _) => _active = false;
    }
}
