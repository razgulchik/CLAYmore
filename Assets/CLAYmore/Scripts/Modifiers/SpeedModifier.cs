using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Speed")]
    public class SpeedModifier : ModifierConfig
    {
        [Min(0.001f)] public float reductionPerLevel = 0.02f;
        [Min(0.03f)]  public float minMoveTime       = 0.05f;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            var move  = playerEntity.Get<MovementComponent>();
            move.MoveTime = Mathf.Max(minMoveTime, stats.BaseMoveTime - newLevel * reductionPerLevel);
        }

        public override string GetDescription(int level)
        {
            float reduction = reductionPerLevel * level * 1000f;
            float min       = minMoveTime * 1000f;
            return $"Move time -{reduction:F0}ms (min {min:F0}ms)";
        }
    }
}
