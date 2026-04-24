using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Whirlwind")]
    public class WhirlwindModifier : ModifierConfig
    {
        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasWhirlwind    = true;
            stats.WhirlwindDamage = newLevel;
        }

        public override string GetDescription(int level)
        {
            return $"On landing: hit radius 1 for {level} damage";
        }
    }
}
