using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Counts up from zero, fires WaveChangedEvent at each wave boundary.
    /// The last wave stays active until the player dies — no session time limit.
    /// </summary>
    public class SessionTimerSystem : ISystem
    {
        private readonly WaveConfig[] _waves; // sorted by startTime

        private World _world;
        private float _elapsed;
        private int   _nextWaveIndex;
        private bool  _active = true;

        public float TimeElapsed => _elapsed;

        public SessionTimerSystem(WaveConfig[] waves)
        {
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
        }

        private void OnGameOver(GameOverEvent _) => _active = false;
    }
}
