using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class PotComponent : IComponent
    {
        public PotConfig Config;
        public PotState  State;
        public Vector3Int LandCell;
        public Vector3    LandPos;
    }
}
