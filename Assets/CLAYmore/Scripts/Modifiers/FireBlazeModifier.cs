using CLAYmore.ECS;

namespace CLAYmore
{
    [UnityEngine.CreateAssetMenu(menuName = "CLAYmore/Modifiers/Fire Blaze")]
    public class FireBlazeModifier : ModifierConfig
    {
        [UnityEngine.Min(1)] public int damagePerLevel = 1;

        public override void Apply(Entity playerEntity, int newLevel)
        {
            var stats = playerEntity.Get<PlayerStatsComponent>();
            stats.HasFireBlaze    = true;
            stats.FireBlazeDamage = damagePerLevel * newLevel;
        }

        public override string GetDescription(int level)
            => $"On landing: shoot fire diagonally forward for {damagePerLevel * level} damage (+{damagePerLevel} per level)";
    }
}
