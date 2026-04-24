using CLAYmore.ECS;

namespace CLAYmore
{
    [UnityEngine.CreateAssetMenu(menuName = "CLAYmore/Modifiers/Fire Balls")]
    public class FireBallsModifier : ModifierConfig
    {
        [UnityEngine.Min(1)] public int damagePerLevel = 1;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasFireBalls    = true;
            stats.FireBallsDamage = damagePerLevel * newLevel;
        }

        public override string GetDescription(int level)
        {
            return $"On move: shoot fire balls sideways for {damagePerLevel * level} damage (+{damagePerLevel} per level)";
        }
    }
}
