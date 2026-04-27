using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Hard Bargain")]
    public class HardBargainModifier : ModifierConfig
    {
        [Range(0f, 1f)] public float discountPerLevel = 0.05f;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            playerEntity.Get<PlayerStatsComponent>().PriceDiscount = newLevel * discountPerLevel;
        }

        public override string GetDescription(int level)
            => $"-{Mathf.RoundToInt(level * discountPerLevel * 100)}% upgrade prices";
    }
}
