using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Lightning")]
    public class LightningModifier : ModifierConfig
    {
        [Min(1f)] public float baseInterval             = 8f;
        [Min(0f)] public float cooldownReductionPerLevel = 1f;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasLightning      = true;
            stats.LightningInterval = Mathf.Max(1f, baseInterval - (newLevel - 1) * cooldownReductionPerLevel);
            // Keep existing timer so there's no immediate reset on upgrade
            if (stats.LightningTimer <= 0f)
                stats.LightningTimer = stats.LightningInterval;
        }

        public override string GetDescription(int level)
        {
            float interval = Mathf.Max(1f, baseInterval - (level - 1) * cooldownReductionPerLevel);
            return $"Lightning strike every {interval:F1}s (-{cooldownReductionPerLevel:F1}s per level)";
        }
    }
}
