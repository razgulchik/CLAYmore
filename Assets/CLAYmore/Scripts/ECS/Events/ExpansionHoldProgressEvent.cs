using UnityEngine;

namespace CLAYmore.ECS
{
    public struct ExpansionHoldProgressEvent
    {
        public float Progress; // 0..1 активно, -1 завершено/отменено
    }
}
