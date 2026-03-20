using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Damage Bonus")]
    public class DamageBonusModifier : ModifierConfig
    {
        [Min(1)] public int damagePerLevel = 1;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.DamageBonus = newLevel * damagePerLevel;
        }

        public override string GetDescription(int level)
            => $"+{level * damagePerLevel} урон за удар";
    }
}
