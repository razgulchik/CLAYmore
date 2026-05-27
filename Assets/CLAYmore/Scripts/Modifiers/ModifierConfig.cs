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
        [Min(1)] public int   maxLevel        = 1;
        [Min(0)] public float spawnWeight    = 1f;
        [Min(0)] public int   price          = 0;
        [Min(1)] public float priceMultiplier = 1.2f;

        public int GetPrice(int level)
            => Mathf.RoundToInt(price * Mathf.Pow(priceMultiplier, level - 1));

        /// <summary>
        /// Apply this modifier to the player. Called by ModifierSystem.
        /// newLevel is the level AFTER this pick (1-based).
        /// </summary>
        public abstract void Apply(Entity playerEntity, int newLevel);

        /// <summary>
        /// Optional: override to return a level-specific description.
        /// </summary>
        public virtual string GetDescription(int level) => description;

        /// <summary>
        /// Optional: override to conditionally exclude this modifier from the pool.
        /// Return false to hide the modifier (e.g. when player is at full HP).
        /// </summary>
        public virtual bool IsAvailable(Entity playerEntity) => true;
    }
}
