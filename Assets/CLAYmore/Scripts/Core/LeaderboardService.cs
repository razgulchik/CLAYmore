using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLAYmore.ECS;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Submits the player's session score to Unity Leaderboards on Game Over,
    /// then pre-fetches the top scores so LeaderboardUI can display them instantly.
    /// </summary>
    public class LeaderboardService : MonoBehaviour
    {
        [Tooltip("Must match the Leaderboard ID created in the Unity Dashboard")]
        public string leaderboardId = "best_score";
        public int    fetchLimit    = 10;

        /// <summary>Cached top scores after the last submit. Null until first fetch completes.</summary>
        public List<LeaderboardEntry> CachedScores { get; private set; }
        public bool IsFetching { get; private set; }
        public bool HasFetchCompleted { get; private set; }

        private StatsTracker _statsTracker;
        private bool         _isReady;

        public void Init(StatsTracker statsTracker) => _statsTracker = statsTracker;

        private async void Awake()
        {
            try
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                _isReady = true;
                Debug.Log($"[LeaderboardService] Signed in: {AuthenticationService.Instance.PlayerId}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LeaderboardService] Init failed: {e.Message}");
            }
        }

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<SessionScoredEvent>(OnSessionScored);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<SessionScoredEvent>(OnSessionScored);
        }

        private async void OnSessionScored(SessionScoredEvent e)
        {
            if (!_isReady) return;
            HasFetchCompleted = false;
            await SubmitAndFetchAsync(e.Score);
        }

        private async Task SubmitAndFetchAsync(int score)
        {
            // Submit first so our score is included in the fetch
            try
            {
                var entry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
                Debug.Log(
                    $"[LeaderboardService] Score {score} submitted.\n" +
                    $"  Rank:       #{entry.Rank + 1}\n" +
                    $"  Best score: {entry.Score}"
                );
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LeaderboardService] Submission failed: {e.Message}");
            }

            // Fetch top scores immediately after submit and cache them
            IsFetching = true;
            try
            {
                var result = await LeaderboardsService.Instance.GetScoresAsync(
                    leaderboardId, new GetScoresOptions { Limit = fetchLimit });
                CachedScores = result.Results;
                Debug.Log($"[LeaderboardService] Fetched {CachedScores.Count} scores.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LeaderboardService] Fetch failed: {e.Message}");
            }
            finally
            {
                IsFetching = false;
                HasFetchCompleted = true;
            }
        }
    }
}
