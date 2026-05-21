using UnityEngine;

namespace CLAYmore
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "CLAYmore/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Player — Health")]
        [Min(1)] public int playerMaxHp = 3;

        [Header("Player — Movement")]
        public float moveTime         = 0.4f;
        public float bounceReturnTime = 0.4f;
        [Tooltip("How long (s) an input stays buffered while mid-move. 0 = no buffer.")]
        [Min(0)] public float inputBufferWindow = 0.15f;

        [Header("Session")]
        [Tooltip("Difficulty waves in order (sequential). Leave empty to skip wave progression.")]
        public WaveConfig[] waves;
        [Tooltip("Time (seconds) from first to last pot during a simultaneous-spawn sweep")]
        [Min(0.1f)] public float burstSweepDuration = 1.5f;

        [Header("Chests")]
        [Tooltip("Coins needed to spawn the first chest")]
        public int chestFirstThreshold = 15;
        [Tooltip("Multiplier applied to the threshold after each chest spawn")]
        public float chestThresholdMultiplier = 1f;
        [Tooltip("Flat coins added to the threshold after each chest spawn")]
        public int chestThresholdAdditive = 5;
        [Tooltip("Expanders awarded to the player when skipping a modifier choice")]
        public int expandersOnSkip = 1;

        [Header("Island Expansion")]
        [Tooltip("Multiplier applied to expansion cost after each expansion")]
        public float expansionCostMultiplier = 1f;
        [Tooltip("Flat expanders added to expansion cost after each expansion")]
        public int expansionCostAdditive = 1;
        public ModifierConfig[] modifierPool;

        [Header("Starting State")]
        public int startingCoins = 0;
        public ModifierConfig[] startingModifiers;

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
