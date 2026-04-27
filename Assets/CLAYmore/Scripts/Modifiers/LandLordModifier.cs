using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Land Lord")]
    public class LandLordModifier : ModifierConfig
    {
        [Range(0f, 1f)] public float discountPerLevel = 0.05f;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            playerEntity.Get<PlayerStatsComponent>().LandDiscount = newLevel * discountPerLevel;
        }

        public override string GetDescription(int level)
            => $"-{Mathf.RoundToInt(level * discountPerLevel * 100)}% land prices";
    }
}
