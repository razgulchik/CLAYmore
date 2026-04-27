using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Golden Boy")]
    public class GoldenBoyModifier : ModifierConfig
    {
        [Min(1)] public int goldPerLevel = 1;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            playerEntity.Get<PlayerStatsComponent>().GoldBonusPerPot = newLevel * goldPerLevel;
        }

        public override string GetDescription(int level)
            => $"+{level * goldPerLevel} gold per pot";
    }
}
