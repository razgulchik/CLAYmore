using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Shield")]
    public class ShieldModifier : ModifierConfig
    {
        [Min(1f)] public float baseCooldown              = 10f;
        [Min(0f)] public float cooldownReductionPerLevel = 1f;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.ShieldMax         = 1;
            stats.ShieldCurrent     = 1;
            stats.ShieldCooldownMax = Mathf.Max(1f, baseCooldown - (newLevel - 1) * cooldownReductionPerLevel);
        }

        public override string GetDescription(int level)
        {
            float cd = Mathf.Max(1f, baseCooldown - (level - 1) * cooldownReductionPerLevel);
            return $"Blocks 1 hit, then {cd:F0}s cooldown (cd -{cooldownReductionPerLevel:F0}s per level)";
        }
    }
}
