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

        [Header("Session")]
        [Tooltip("Total session duration in seconds (e.g. 600 = 10 min)")]
        public float sessionDuration = 600f;
        [Tooltip("Difficulty waves sorted by startTime. Leave empty to skip wave progression.")]
        public WaveConfig[] waves;

        [Header("Chest Spawner")]
        public float chestSpawnInitialInterval = 30f;
        public float chestSpawnMinInterval     = 20f;

        [Header("Scoring")]
        [Tooltip("Points awarded per second of play time")]
        public int pointsPerSecond   = 5;
        [Tooltip("Points awarded per pot destroyed")]
        public int pointsPerPot      = 10;
        [Tooltip("Points awarded per modifier chosen")]
        public int pointsPerModifier = 20;
        [Tooltip("Points awarded per coin earned")]
        public int pointsPerCoin     = 5;
    }
}
