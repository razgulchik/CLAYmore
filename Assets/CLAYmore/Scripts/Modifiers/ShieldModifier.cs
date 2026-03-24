using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Shield")]
    public class ShieldModifier : ModifierConfig
    {
        [Min(5f)] public float cooldownSeconds = 10f;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.ShieldMax         = newLevel;
            stats.ShieldCurrent     = newLevel;
            stats.ShieldCooldownMax = cooldownSeconds;
        }

        public override string GetDescription(int level)
            => $"Absorb {level} damage, then cooldown {cooldownSeconds}s";
    }
}
