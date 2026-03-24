using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Per-direction appearance settings for one edge indicator line.
    /// Attach this component to each of the four edge child GameObjects
    /// (Up / Down / Left / Right).
    /// </summary>
    public class EdgeLineConfig : MonoBehaviour
    {
        [Header("Appearance")]
        [Tooltip("Толщина полосы в юнитах")]
        public float lineThickness = 0.25f;

        [Tooltip("Смещение наружу от края острова")]
        public float edgeOffset = 0.5f;

        [Tooltip("Сколько отрезать с каждой стороны линии (уменьшает длину с обоих концов)")]
        public float lengthMargin = 0f;

        [Tooltip("Смещение вдоль края: для Up/Down — влево/вправо, для Left/Right — вверх/вниз")]
        public float centerOffset = 0f;
    }
}
