using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Fire Trail")]
    public class FireTrailModifier : ModifierConfig
    {
        [Min(1)] public int damagePerLevel = 1;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasFireTrail    = true;
            stats.FireTrailDamage = damagePerLevel * newLevel;
        }

        public override string GetDescription(int level)
        {
            int dmg = damagePerLevel * level;
            return $"Leaving a cell sets it on fire for 10s, dealing {dmg} damage/s (+{damagePerLevel} per level)";
        }
    }
}
