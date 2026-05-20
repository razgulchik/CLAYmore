using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Two responsibilities:
    ///   1. Burst waves — tracks non-rock pot count, starts clearance pause, then publishes WaveClearedEvent.
    ///   2. Final cleanup — triggered by FinalCleanupEvent (all waves done). Counts remaining live
    ///      non-rock pots; when zero, publishes GameWonEvent.
    /// </summary>
    public class WaveClearanceSystem : ISystem
    {
        private World _world;

        private bool  _inBurstMode;
        private bool  _inFinalCleanup;
        private int   _remainingNonRockPots;
        private float _pauseTimer = -1f;
        private float _clearancePauseDuration;

        public void Initialize(World world)
        {
            _world = world;

            world.Events.Subscribe<WaveChangedEvent>(OnWaveChanged);
            world.Events.Subscribe<BurstPotSpawnedEvent>(OnBurstPotSpawned);
            world.Events.Subscribe<EntityDiedEvent>(OnEntityDied);
            world.Events.Subscribe<FinalCleanupEvent>(OnFinalCleanup);
            world.Events.Subscribe<GameOverEvent>(OnGameOver);
        }

        public void Tick(float dt)
        {
            if (_pauseTimer < 0f) return;

            _pauseTimer -= dt;
            if (_pauseTimer > 0f) return;

            _pauseTimer  = -1f;
            _inBurstMode = false;
            _world.Events.Publish(new WaveClearedEvent());
        }

        // ── Burst wave ────────────────────────────────────────────────────────

        private void OnWaveChanged(WaveChangedEvent e)
        {
            _pauseTimer = -1f;

            if (!e.Config.useSimultaneousSpawn)
            {
                _inBurstMode = false;
                return;
            }

            _inBurstMode            = true;
            _clearancePauseDuration = e.Config.clearancePauseDuration;
            _remainingNonRockPots   = 0;

            foreach (var entity in _world.Query<PotComponent>())
            {
                var pot = entity.Get<PotComponent>();
                if (pot.Config != null && !pot.Config.isRock)
                    _remainingNonRockPots++;
            }

            Debug.Log($"[WaveClearance] Wave {e.WaveIndex} — existing non-rock pots: {_remainingNonRockPots}");
        }

        private void OnBurstPotSpawned(BurstPotSpawnedEvent e)
        {
            if (!_inBurstMode) return;
            if (!e.IsRock)
                _remainingNonRockPots++;
        }

        // ── Final cleanup ─────────────────────────────────────────────────────

        private void OnFinalCleanup(FinalCleanupEvent _)
        {
            _inBurstMode      = false;
            _inFinalCleanup   = true;
            _pauseTimer       = -1f;
            _remainingNonRockPots = 0;

            foreach (var entity in _world.Query<PotComponent>())
            {
                var pot = entity.Get<PotComponent>();
                if (pot.Config == null || pot.Config.isRock) continue;
                if (entity.Has<HealthComponent>() && entity.Get<HealthComponent>().Hp > 0)
                    _remainingNonRockPots++;
            }

            Debug.Log($"[WaveClearance] FinalCleanup — live non-rock pots: {_remainingNonRockPots}");

            if (_remainingNonRockPots == 0)
            {
                _inFinalCleanup = false;
                _world.Events.Publish(new GameWonEvent());
            }
        }

        // ── Shared ────────────────────────────────────────────────────────────

        private void OnEntityDied(EntityDiedEvent e)
        {
            if (!_inBurstMode && !_inFinalCleanup) return;
            if (!e.Entity.Has<PotComponent>()) return;

            var pot = e.Entity.Get<PotComponent>();
            if (pot.Config == null || pot.Config.isRock) return;

            _remainingNonRockPots = Mathf.Max(0, _remainingNonRockPots - 1);

            if (_remainingNonRockPots > 0) return;

            if (_inFinalCleanup)
            {
                _inFinalCleanup = false;
                _world.Events.Publish(new GameWonEvent());
            }
            else if (_inBurstMode && _pauseTimer < 0f)
            {
                _pauseTimer = _clearancePauseDuration;
                Debug.Log($"[WaveClearance] All burst pots cleared — pause {_clearancePauseDuration:F1}s");
            }
        }

        private void OnGameOver(GameOverEvent _)
        {
            _inBurstMode    = false;
            _inFinalCleanup = false;
            _pauseTimer     = -1f;
        }
    }
}
