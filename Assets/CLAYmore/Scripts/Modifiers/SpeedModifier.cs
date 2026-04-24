using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Speed")]
    public class SpeedModifier : ModifierConfig
    {
        [Range(0.01f, 0.5f)] public float speedBonusPerLevel = 0.10f;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            var move  = playerEntity.Get<MovementComponent>();

            stats.SpeedMultiplier = 1f + newLevel * speedBonusPerLevel;
            move.MoveTime         = stats.BaseMoveTime / stats.SpeedMultiplier;
        }

        public override string GetDescription(int level)
        {
            float totalBonus = speedBonusPerLevel * level * 100f;
            float reduction  = (1f - 1f / (1f + speedBonusPerLevel * level)) * 100f;
            return $"+{totalBonus:F0}% speed ({reduction:F0}% faster)";
        }
    }
}
