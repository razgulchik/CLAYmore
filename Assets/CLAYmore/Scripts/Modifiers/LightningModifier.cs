using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Lightning")]
    public class LightningModifier : ModifierConfig
    {
        [Min(1f)] public float baseInterval             = 8f;
        [Min(0f)] public float cooldownReductionPerLevel = 1f;
        [Min(1)]  public int   damage                   = 1;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasLightning      = true;
            stats.LightningDamage   = damage;
            stats.LightningInterval = Mathf.Max(1f, baseInterval - (newLevel - 1) * cooldownReductionPerLevel);
            // Keep existing timer so there's no immediate reset on upgrade
            if (stats.LightningTimer <= 0f)
                stats.LightningTimer = stats.LightningInterval;
        }

        public override string GetDescription(int level)
        {
            float interval = Mathf.Max(1f, baseInterval - (level - 1) * cooldownReductionPerLevel);
            return $"Every {interval:F0}s strikes a random pot for {damage} damage (cd -{cooldownReductionPerLevel:F0}s per level)";
        }
    }
}
