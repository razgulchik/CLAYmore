using System.Collections;
using CLAYmore.ECS;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Leaderboard panel. Uses scores pre-fetched by LeaderboardService after submit.
    /// Falls back to polling if fetch is still in progress.
    /// Assign in Inspector: panel, rowPrefab, listRoot, leaderboardService, loadingLabel.
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("References")]
        public GameObject      panel;
        public ScoreRowUI      rowPrefab;
        public Transform       listRoot;
        public LeaderboardService leaderboardService;
        public TextMeshProUGUI loadingLabel;

        private void Awake()
        {
            panel.SetActive(false);
        }

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<GameOverEvent>(OnGameOver);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<GameOverEvent>(OnGameOver);
        }

        private void OnGameOver(GameOverEvent _) => Show();

        // ── Public ────────────────────────────────────────────────────────────

        public void Show()
        {
            panel.SetActive(true);
            ClearRows();
            StartCoroutine(WaitAndDisplay());
        }

        public void Hide()
        {
            StopAllCoroutines();
            panel.SetActive(false);
            ClearRows();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private IEnumerator WaitAndDisplay()
        {
            if (loadingLabel != null)
            {
                loadingLabel.gameObject.SetActive(true);
                loadingLabel.text = "Server Request...";
            }

            // Wait until LeaderboardService finishes submit + fetch
            while (leaderboardService != null && !leaderboardService.HasFetchCompleted)
                yield return null;

            if (loadingLabel != null)
                loadingLabel.gameObject.SetActive(false);

            if (leaderboardService == null || leaderboardService.CachedScores == null)
            {
                Debug.LogWarning("[LeaderboardUI] No cached scores available.");
                yield break;
            }

            string currentPlayerId = AuthenticationService.Instance.IsSignedIn
                ? AuthenticationService.Instance.PlayerId
                : null;

            foreach (var entry in leaderboardService.CachedScores)
            {
                var row = Instantiate(rowPrefab, listRoot);
                row.Setup(entry.Rank + 1, entry.PlayerName, (int)entry.Score);

                if (currentPlayerId != null && entry.PlayerId == currentPlayerId)
                    row.Highlight(Color.yellow);
            }
        }

        private void ClearRows()
        {
            foreach (Transform child in listRoot)
                Destroy(child.gameObject);
        }
    }
}
