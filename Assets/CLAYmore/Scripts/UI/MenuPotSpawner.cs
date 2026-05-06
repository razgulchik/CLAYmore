using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Spawns decorative falling pots on the main menu background island.
    /// No ECS, no Economy, no player — purely visual.
    /// </summary>
    public class MenuPotSpawner : MonoBehaviour
    {
        [Header("References")]
        public IslandGenerator islandGenerator;
        public PrefabPool potPool;
        public PrefabPool shadowPool;
        public PotConfig[] potConfigs;

        [Header("Timing")]
        [Min(0.1f)] public float spawnIntervalMin = 1.5f;
        [Min(0.1f)] public float spawnIntervalMax = 3f;
        [Tooltip("Seconds a pot sits on the island before fading out")]
        [Min(0.1f)] public float sitDuration = 5f;
        [Min(0.05f)] public float vanishDuration = 0.5f;
        [Tooltip("Fall duration in seconds for all pots. Overrides the per-config value.")]
        [Min(0.1f)] public float fallDuration = 1.5f;

        private void Start()
        {
            StartCoroutine(SpawnLoop());
        }

        private IEnumerator SpawnLoop()
        {
            yield return null; // wait for IslandGenerator.Awake
            while (true)
            {
                TrySpawnPot();
                yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            }
        }

        private void TrySpawnPot()
        {
            if (potConfigs == null || potConfigs.Length == 0) return;

            if (!islandGenerator.TryGetRandomWalkableCellCenter(out Vector3 landPos)) return;
            if (!islandGenerator.TryReserveCell(landPos)) return;

            PotConfig config = PickConfig();
            if (config.isRock) return; // rocks fill island permanently — skip

            Vector3 spawnPos = landPos + Vector3.up * config.spawnHeight;

            GameObject potGO = potPool.Get(spawnPos);
            potGO.GetComponent<Pot>().enabled = false;

            var sr = potGO.GetComponent<SpriteRenderer>();
            sr.sprite            = config.sprite;
            sr.sortingLayerName  = "PotsFlight";
            var c = sr.color; c.a = 1f; sr.color = c;
            potGO.transform.localScale = Vector3.one;
            potGO.transform.DOKill();

            GameObject shadow = shadowPool != null
                ? shadowPool.Get(new Vector3(landPos.x, landPos.y, spawnPos.z))
                : null;

            potGO.transform.DOMove(landPos, fallDuration)
                .SetEase(config.fallEase)
                .OnComplete(() => OnLanded(potGO, shadow, landPos, sr));
        }

        private void OnLanded(GameObject potGO, GameObject shadow, Vector3 landPos, SpriteRenderer sr)
        {
            potGO.transform.position = landPos;
            sr.sortingLayerName      = "Pots";
            islandGenerator.MarkPotLanded(landPos);
            shadowPool?.Return(shadow);

            StartCoroutine(VanishAfterDelay(potGO, landPos, sr));
        }

        private IEnumerator VanishAfterDelay(GameObject potGO, Vector3 landPos, SpriteRenderer sr)
        {
            yield return new WaitForSeconds(sitDuration);

            yield return sr.DOFade(0f, vanishDuration).WaitForCompletion();

            var c = sr.color; c.a = 1f; sr.color = c;
            islandGenerator.ClearCell(landPos);
            potGO.GetComponent<Pot>().enabled = true;
            potPool.Return(potGO);
        }

        private PotConfig PickConfig()
        {
            float total = 0f;
            foreach (var cfg in potConfigs) total += cfg.spawnWeight;

            float roll = Random.Range(0f, total);
            float acc  = 0f;
            foreach (var cfg in potConfigs)
            {
                acc += cfg.spawnWeight;
                if (roll <= acc) return cfg;
            }
            return potConfigs[potConfigs.Length - 1];
        }
    }
}
