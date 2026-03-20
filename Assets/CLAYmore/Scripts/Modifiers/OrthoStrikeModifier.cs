using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Ortho Strike")]
    public class OrthoStrikeModifier : ModifierConfig
    {
        public override void Apply(Entity playerEntity, int newLevel)
        {
            playerEntity.Get<PlayerStatsComponent>().HasOrthoStrike = true;
        }
    }
}
