using System;
using UnityEngine;

namespace CLAYmore
{
    [Serializable]
    public struct WavePotWeight
    {
        public PotConfig config;
        [Min(0f)] public float weight;
    }

    /// <summary>
    /// Parameters for one difficulty wave.
    /// Assign in GameConfig.waves[] sorted by startTime.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "CLAYmore/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        [Tooltip("Session time in seconds when this wave activates")]
        public float startTime = 0f;

        [Header("Spawn Timing")]
        [Tooltip("Spawn interval at the start of this wave")]
        public float spawnInterval = 3f;
        [Tooltip("Hard minimum — interval cannot go below this value")]
        public float minSpawnInterval = 0.5f;
        [Tooltip("How fast spawn interval keeps decreasing within this wave (per second)")]
        public float spawnDecreasePerSecond = 0.02f;

        [Header("Fall Speed")]
        [Tooltip("Multiplier applied to fallDuration of all pots. < 1 = faster fall.")]
        [Range(0.1f, 2f)] public float fallDurationMultiplier = 1f;

        [Header("Targeting")]
        [Tooltip("Every N spawns, one targets the player (min)")]
        [Min(1)] public int targetedSpawnEveryMin = 4;
        [Tooltip("Every N spawns, one targets the player (max)")]
        [Min(1)] public int targetedSpawnEveryMax = 7;

        [Header("Pot Weights")]
        [Tooltip("Per-pot-type weight overrides for this wave. Leave empty to use PotConfig.spawnWeight.")]
        public WavePotWeight[] potWeights;

        [Header("Rocks")]
        [Tooltip("Probability (0-1) that a spawn event produces a rock instead of a pot")]
        [Range(0f, 1f)] public float rockSpawnChance = 0f;
    }
}
