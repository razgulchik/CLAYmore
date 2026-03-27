using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "CLAYmore/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Player — Health")]
        [Min(1)] public int playerMaxHp = 3;

        [Header("Player — Movement")]
        public float moveTime         = 0.15f;
        public float bounceTime       = 0.1f;
        public float bounceReturnTime = 0.05f;

        [Header("Pot Spawner — Timing")]
        public float spawnInitialInterval = 3f;
        public float spawnMinInterval = 0.5f;
        [Tooltip("Seconds subtracted from spawn interval per second of play time")]
        public float spawnIntervalDecreasePerSecond = 0.02f;

        [Header("Pot Spawner — Targeted")]
        [Tooltip("N is chosen randomly in [min, max] each time; the next targeted spawn falls on the player's tile")]
        [Min(1)] public int targetedSpawnEveryMin = 4;
        [Min(1)] public int targetedSpawnEveryMax = 7;

        [Header("Chest Spawner")]
        public float chestSpawnInitialInterval = 30f;
        public float chestSpawnMinInterval     = 20f;
    }
}
