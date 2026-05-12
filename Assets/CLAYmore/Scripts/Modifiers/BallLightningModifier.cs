using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Ball Lightning")]
    public class BallLightningModifier : ModifierConfig
    {
        [Min(1)] public int damagePerLevel   = 5;
        [Min(1)] public int explosionRadius  = 1;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasBallLightning    = true;
            stats.BallLightningDamage = damagePerLevel * newLevel;
            stats.BallLightningRadius = explosionRadius;
        }

        public override bool IsAvailable(Entity playerEntity)
        {
            if (playerEntity == null || !playerEntity.Has<PlayerModifiersComponent>()) return false;
            return playerEntity.Get<PlayerModifiersComponent>().Levels.ContainsKey("Lightning Mod");
        }

        public override string GetDescription(int level)
        {
            int dmg = damagePerLevel * level;
            return $"Lightning leave a ball lightning. Trigger it for {dmg} damage in radius {explosionRadius}"; // (+{damagePerLevel} per level)
        }
    }
}
