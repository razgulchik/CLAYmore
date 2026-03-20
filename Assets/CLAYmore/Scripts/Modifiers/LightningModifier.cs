using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Lightning")]
    public class LightningModifier : ModifierConfig
    {
        [Min(1f)] public float baseInterval = 8f;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasLightning      = true;
            stats.LightningInterval = baseInterval / newLevel;
            // Keep existing timer so there's no immediate reset on upgrade
            if (stats.LightningTimer <= 0f)
                stats.LightningTimer = stats.LightningInterval;
        }

        public override string GetDescription(int level)
            => $"Молния бьёт каждые {baseInterval / level:F1}с по случайному горшку";
    }
}
