using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/AoE Strike")]
    public class AoeStrikeModifier : ModifierConfig
    {
        public override void Apply(Entity playerEntity, int newLevel)
        {
            playerEntity.Get<PlayerStatsComponent>().HasAoeStrike = true;
        }
    }
}
