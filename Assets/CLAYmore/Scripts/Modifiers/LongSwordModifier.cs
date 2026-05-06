using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Long Sword")]
    public class LongSwordModifier : ModifierConfig
    {
        public override void Apply(Entity playerEntity, int newLevel)
        {
            playerEntity.Get<PlayerStatsComponent>().LongSwordReach = newLevel;
        }

        public override string GetDescription(int level)
            => $"Sword reaches {level + 1} cells forward";
    }
}
