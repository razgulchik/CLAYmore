using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class MovementComponent : IComponent
    {
        public bool IsMoving;
        public Vector2Int FacingDirection = Vector2Int.down;
    }
}
