using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Golden Urn")]
    public class GoldenUrnModifier : ModifierConfig
    {
        [Min(0.01f)] public float chancePerLevel = 0.1f;
        [Min(1)]     public int   goldPerLevel   = 10;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            playerEntity.Get<PlayerStatsComponent>().GoldenUrnChance    = newLevel * chancePerLevel;
            playerEntity.Get<PlayerStatsComponent>().GoldenUrnGoldReward = newLevel * goldPerLevel;
        }

        public override string GetDescription(int level)
            => $"{level * chancePerLevel * 100:F0}% spawn chance of Golden Urn (+{level * goldPerLevel} gold)";
    }
}
