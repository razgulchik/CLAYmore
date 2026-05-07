using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Lucky Day")]
    public class LuckyDayModifier : ModifierConfig
    {
        [Min(0.001f)] public float baseChance      = 0.05f;
        [Min(0.001f)] public float chancePerLevel  = 0.01f;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            playerEntity.Get<PlayerStatsComponent>().HearthChance =
                baseChance + (newLevel - 1) * chancePerLevel;
        }

        public override string GetDescription(int level)
        {
            float chance = baseChance + Mathf.Max(0, level - 1) * chancePerLevel;
            return $"{chance * 100:F0}% chance a heart falls instead of a pot. Heals 1 HP.";
        }
    }
}
