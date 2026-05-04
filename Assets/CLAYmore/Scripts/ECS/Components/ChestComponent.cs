using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public enum ChestState { Active, Opened }

    public class ChestComponent : IComponent
    {
        public ChestState  State;
        public Vector3Int  LandCell;
        public Vector3     LandPos;
    }
}
