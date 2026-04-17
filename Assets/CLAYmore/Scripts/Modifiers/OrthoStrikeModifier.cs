using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Ortho Strike")]
    public class OrthoStrikeModifier : ModifierConfig
    {
        [Min(1)] public int damage = 1;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasOrthoStrike    = true;
            stats.OrthoStrikeDamage = damage;
        }
    }
}
