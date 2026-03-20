using System.Collections.Generic;

namespace CLAYmore.ECS
{
    public class PlayerModifiersComponent : IComponent
    {
        /// <summary>
        /// ModifierConfig.name (asset name) → current level (1-based).
        /// </summary>
        public Dictionary<string, int> Levels = new();
    }
}
