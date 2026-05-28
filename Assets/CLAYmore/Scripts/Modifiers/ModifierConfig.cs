using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public enum ModifierCategory { Attack, Heal, Money, Speed }

    [System.Serializable]
    public struct ModifierCategoryBackground
    {
        public ModifierCategory category;
        public Sprite           sprite;
    }

    public abstract class ModifierConfig : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public ModifierCategory category;
        [Min(1)] public int   maxLevel    = 1;
        [Min(0)] public float spawnWeight = 1f;

        public abstract void Apply(Entity playerEntity, int newLevel);

        public virtual string GetDescription(int level) => description;

        public virtual bool IsAvailable(Entity playerEntity) => true;
    }
}
