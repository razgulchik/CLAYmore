using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Shockwave")]
    public class ShockwaveModifier : ModifierConfig
    {
        [Min(1)] public int damagePerLevel = 1;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasShockwave    = true;
            stats.ShockwaveDamage = damagePerLevel * newLevel;
        }

        public override string GetDescription(int level)
        {
            int dmg = damagePerLevel * level;
            return $"Every 3 steps fire a shockwave forward for {dmg} damage (+{damagePerLevel} per level)";
        }
    }
}
