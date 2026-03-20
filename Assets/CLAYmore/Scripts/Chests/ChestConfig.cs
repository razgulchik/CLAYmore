using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(fileName = "ChestConfig", menuName = "CLAYmore/Chest Config")]
    public class ChestConfig : ScriptableObject
    {
        public Sprite sprite;
        [Min(1)] public int   choiceCount  = 3;
        [Min(0)] public int   coinsOnSkip  = 5;
    }
}
