using CLAYmore.ECS;

namespace CLAYmore
{
    /// <summary>
    /// Applies chosen modifiers to the player entity and tracks their levels.
    /// Also handles the skip-for-coins action.
    /// </summary>
    public class ModifierSystem : ISystem
    {
        private World _world;
        private EconomySystem _economy;

        public void Initialize(World world)
        {
            _world   = world;
            _economy = world.GetSystem<EconomySystem>();
            world.Events.Subscribe<ModifierChosenEvent>(OnModifierChosen);
            world.Events.Subscribe<ModifierSkippedEvent>(OnModifierSkipped);
        }

        public void Tick(float deltaTime) { }

        private void OnModifierChosen(ModifierChosenEvent evt)
        {
            Entity playerEntity = GetPlayerEntity();
            if (playerEntity == null) return;

            var modifiers = playerEntity.Get<PlayerModifiersComponent>();
            modifiers.Levels.TryGetValue(evt.Modifier.name, out int currentLevel);
            int newLevel = currentLevel + 1;
            modifiers.Levels[evt.Modifier.name] = newLevel;

            evt.Modifier.Apply(playerEntity, newLevel);
            PlayerStatsPublisher.Publish(_world);

            var health = playerEntity.Has<HealthComponent>() ? playerEntity.Get<HealthComponent>() : null;
            if (health != null)
                _world.Events.Publish(new PlayerHpChangedEvent { Hp = health.Hp, MaxHp = health.MaxHp });
        }

        private void OnModifierSkipped(ModifierSkippedEvent evt)
        {
            _economy.Add(evt.CoinsGiven);
        }

        private Entity GetPlayerEntity() => _world.QueryFirst<PlayerStatsComponent>();
    }
}
