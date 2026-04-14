using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/Heal")]
    public class HealModifier : ModifierConfig
    {
        public override void Apply(Entity playerEntity, int newLevel)
        {
            var health = playerEntity.Get<HealthComponent>();
            health.Hp = Mathf.Min(health.Hp + 1, health.MaxHp);
        }

        public override bool IsAvailable(Entity playerEntity)
        {
            if (playerEntity == null || !playerEntity.Has<HealthComponent>()) return true;
            var health = playerEntity.Get<HealthComponent>();
            return health.Hp < health.MaxHp;
        }

        public override string GetDescription(int level) => "+1 HP";
    }
}
