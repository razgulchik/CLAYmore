using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Base ScriptableObject for all player modifiers.
    /// Each concrete modifier subclass implements Apply() with its specific logic.
    /// </summary>
    public abstract class ModifierConfig : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        [Min(1)] public int   maxLevel    = 1;
        [Min(0)] public float spawnWeight = 1f;
        [Min(0)] public int   price       = 0;

        /// <summary>
        /// Apply this modifier to the player. Called by ModifierSystem.
        /// newLevel is the level AFTER this pick (1-based).
        /// </summary>
        public abstract void Apply(Entity playerEntity, int newLevel);

        /// <summary>
        /// Optional: override to return a level-specific description.
        /// </summary>
        public virtual string GetDescription(int level) => description;
    }
}
