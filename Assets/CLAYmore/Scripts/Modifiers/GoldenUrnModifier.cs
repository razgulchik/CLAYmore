using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Golden Urn")]
    public class GoldenUrnModifier : ModifierConfig
    {
        [Min(0.01f)] public float chance        = 0.1f;
        [Min(1)]     public int   goldPerLevel = 10;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            playerEntity.Get<PlayerStatsComponent>().GoldenUrnChance     = chance;
            playerEntity.Get<PlayerStatsComponent>().GoldenUrnGoldReward = newLevel * goldPerLevel;
        }

        public override string GetDescription(int level)
            => $"{chance * 100:F0}% spawn chance of Golden Urn (+{level * goldPerLevel} gold)";
    }
}
