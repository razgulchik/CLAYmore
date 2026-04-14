using CLAYmore.ECS;

namespace CLAYmore
{
    public static class PlayerStatsPublisher
    {
        public static void Publish(World world)
        {
            var player = world.QueryFirst<PlayerStatsComponent>();
            if (player == null) return;

            var stats    = player.Get<PlayerStatsComponent>();
            var movement = player.Has<MovementComponent>() ? player.Get<MovementComponent>() : null;
            var health   = player.Has<HealthComponent>()   ? player.Get<HealthComponent>()   : null;

            world.Events.Publish(new PlayerStatsChangedEvent
            {
                MaxHp    = health?.MaxHp ?? 0,
                Damage   = 1 + stats.DamageBonus,
                MoveTime = movement?.MoveTime ?? 0f,
            });
        }
    }
}
