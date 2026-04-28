using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Modifiers/MaxHp")]
    public class MaxHpModifier : ModifierConfig
    {
        public override void Apply(Entity playerEntity, int newLevel)
        {
            var health = playerEntity.Get<HealthComponent>();
            health.MaxHp += 1;
            health.Hp = Mathf.Min(health.Hp + 1, health.MaxHp);
        }

        public override string GetDescription(int level) => "+1 max HP";
    }
}
