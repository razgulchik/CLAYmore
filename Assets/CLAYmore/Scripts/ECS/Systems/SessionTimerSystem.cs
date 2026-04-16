using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Counts down the session, fires WaveChangedEvent at each wave boundary,
    /// and publishes SessionTimeUpEvent when time runs out.
    /// </summary>
    public class SessionTimerSystem : ISystem
    {
        private readonly float        _sessionDuration;
        private readonly WaveConfig[] _waves; // sorted by startTime

        private World _world;
        private float _elapsed;
        private int   _nextWaveIndex;
        private bool  _active = true;

        public float TimeRemaining => Mathf.Max(0f, _sessionDuration - _elapsed);

        public SessionTimerSystem(float sessionDuration, WaveConfig[] waves)
        {
            _sessionDuration = sessionDuration;

            if (waves != null && waves.Length > 0)
            {
                _waves = (WaveConfig[])waves.Clone();
                System.Array.Sort(_waves, (a, b) => a.startTime.CompareTo(b.startTime));
            }
            else
            {
                _waves = System.Array.Empty<WaveConfig>();
            }
        }

        public void Initialize(World world)
        {
            _world = world;
            world.Events.Subscribe<GameOverEvent>(OnGameOver);
        }

        public void Tick(float deltaTime)
        {
            if (!_active) return;

            _elapsed += deltaTime;

            // Trigger all waves whose startTime has been passed this frame
            while (_nextWaveIndex < _waves.Length &&
                   _elapsed >= _waves[_nextWaveIndex].startTime)
            {
                var wave = _waves[_nextWaveIndex];
                _world.Events.Publish(new WaveChangedEvent
                {
                    WaveIndex = _nextWaveIndex,
                    Config    = wave
                });
                Debug.Log($"[SessionTimer] Wave {_nextWaveIndex} — t={_elapsed:F0}s");
                _nextWaveIndex++;
            }

            if (_elapsed >= _sessionDuration)
            {
                _active = false;
                _world.Events.Publish(new SessionTimeUpEvent());
                Debug.Log("[SessionTimer] Session complete.");
            }
        }

        private void OnGameOver(GameOverEvent _) => _active = false;
    }
}
