using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Fire Trail")]
    public class FireTrailModifier : ModifierConfig
    {
        [Min(1)]    public int   damage          = 1;
        [Min(1f)]   public float baseLifetime    = 10f;
        [Min(0.1f)] public float lifetimePerLevel = 5f;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasFireTrail      = true;
            stats.FireTrailDamage   = damage;
            stats.FireTrailLifetime = baseLifetime + lifetimePerLevel * (newLevel - 1);
        }

        public override string GetDescription(int level)
        {
            float lifetime = baseLifetime + lifetimePerLevel * (level - 1);
            return $"Leaving a cell sets it on fire for {lifetime:F0}s, dealing {damage} damage/s (+{lifetimePerLevel:F0}s per level)";
        }
    }
}
