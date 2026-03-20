using DG.Tweening;
using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(menuName = "CLAYmore/Pot Config", fileName = "PotConfig")]
    public class PotConfig : ScriptableObject
    {
        [Min(1)] public int maxHp = 1;
        [Min(0)] public int coinDropMin = 1;
        [Min(0)] public int coinDropMax = 3;
        public float fallDuration = 0.8f;
        public Ease fallEase = Ease.InQuad;
        public float spawnHeight = 8f;
        public Sprite sprite;
        [Min(0f)] public float spawnWeight = 1f;
    }
}
