using System.Collections;
using System.Collections.Generic;
using CLAYmore.ECS;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

namespace CLAYmore
{
    /// <summary>
    /// Leaderboard panel. Uses scores pre-fetched by LeaderboardService after submit.
    /// Falls back to polling if fetch is still in progress.
    /// Assign in Inspector: panel, rowPrefab, listRoot, leaderboardService, loadingLabel,
    /// prevButton, nextButton, pageLabel.
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("References")]
        public GameObject         panel;
        public ScoreRowUI         rowPrefab;
        public Transform          listRoot;
        public LeaderboardService leaderboardService;
        public TextMeshProUGUI    loadingLabel;

        [Header("Pagination")]
        public Button          prevButton;
        public Button          nextButton;
        public TextMeshProUGUI pageLabel;
        [SerializeField] private int pageSize = 10;

        [Header("Rename Panel")]
        [SerializeField] private bool debugAllowRename;
        public GameObject      renamePanel;
        public TMP_InputField  nameInput;
        public Button          confirmNameButton;
        public TextMeshProUGUI nameStatusLabel;

        private List<LeaderboardEntry> _entries;
        private string                 _currentPlayerId;
        private int                    _currentPage;

        private void Awake()
        {
            panel.SetActive(false);
            if (renamePanel != null) renamePanel.SetActive(false);
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
            if (renamePanel != null) renamePanel.SetActive(false);
            ClearRows();
            _entries = null;
        }

        public void OnRenameClicked()
        {
            if (renamePanel == null) return;
            if (nameInput != null)
                nameInput.text = PlayerPrefs.GetString("player_name", "");
            if (nameStatusLabel != null) nameStatusLabel.text = "";
            renamePanel.SetActive(true);
        }

        public void OnCancelRenameClicked()
        {
            if (renamePanel != null) renamePanel.SetActive(false);
        }

        public async void OnConfirmNameClicked()
        {
            if (nameInput == null || leaderboardService == null) return;
            string name = nameInput.text.Trim();
            if (!IsValidName(name))
            {
                if (nameStatusLabel != null) nameStatusLabel.text = "Letters, digits, _ only (max 12)";
                return;
            }

            if (confirmNameButton != null) confirmNameButton.interactable = false;
            if (nameStatusLabel != null) nameStatusLabel.text = "Updating...";

            await leaderboardService.UpdateNameAndRefreshAsync(name);

            _entries         = leaderboardService.CachedScores;
            _currentPlayerId = AuthenticationService.Instance.PlayerId;
            ShowPage(_currentPage);

            PlayerPrefs.SetInt("name_changed", 1);
            if (confirmNameButton != null) confirmNameButton.interactable = true;
            if (renamePanel != null) renamePanel.SetActive(false);
        }

        private bool CanRename() => debugAllowRename || PlayerPrefs.GetInt("name_changed", 0) == 0;

        private static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 12) return false;
            foreach (char c in name)
                if (!char.IsLetterOrDigit(c) && c != '_') return false;
            string lower = name.ToLowerInvariant();
            foreach (var w in new[] {
                "fuck", "f4ck", "fvck", "phuck",
                "shit", "sh1t", "5hit",
                "bitch", "b1tch",
                "dick", "d1ck",
                "cunt", "cvnt",
                "cock", "c0ck",
                "pussy", "pu55y",
                "nigger", "n1gger", "nigga", "n1gga",
                "faggot", "f4ggot", "fag",
                "retard", "r3tard",
                "whore", "wh0re",
                "slut", "bastard", "piss", "crap",
                "asshole",
                "хуй", "х0й", "хуйло",
                "пизда", "п1зда",
                "блядь", "бл4дь",
                "сука", "еблан",
                "ебать", "еб4ть",
                "мудак", "мудило",
                "пидор", "п1дор", "пидрила",
                "залупа", "ублюдок",
                "шлюха", "дрочить"
            })
                if (lower.Contains(w)) return false;
            return true;
        }

        public void NextPage() => ShowPage(_currentPage + 1);
        public void PrevPage() => ShowPage(_currentPage - 1);

        // ── Private ───────────────────────────────────────────────────────────

        private IEnumerator WaitAndDisplay()
        {
            if (loadingLabel != null)
            {
                loadingLabel.gameObject.SetActive(true);
                loadingLabel.text = "Server Request...";
            }

            while (leaderboardService != null && !leaderboardService.HasFetchCompleted)
                yield return null;

            if (loadingLabel != null)
                loadingLabel.gameObject.SetActive(false);

            if (leaderboardService == null || leaderboardService.CachedScores == null)
            {
                Debug.LogWarning("[LeaderboardUI] No cached scores available.");
                yield break;
            }

            _entries = leaderboardService.CachedScores;
            _currentPlayerId = AuthenticationService.Instance.IsSignedIn
                ? AuthenticationService.Instance.PlayerId
                : null;

            ShowPage(0);
        }

        private void ShowPage(int page)
        {
            _currentPage = page;
            ClearRows();

            int total = _entries?.Count ?? 0;
            int pages = Mathf.CeilToInt((float)total / pageSize);
            int from  = page * pageSize;
            int to    = Mathf.Min(from + pageSize, total);

            for (int i = from; i < to; i++)
            {
                var entry = _entries[i];
                var row   = Instantiate(rowPrefab, listRoot);
                row.Setup(entry.Rank + 1, entry.PlayerName, (int)entry.Score);
                if (_currentPlayerId != null && entry.PlayerId == _currentPlayerId)
                {
                    row.Highlight(Color.yellow);
                    if (CanRename())
                        row.ShowRenameButton(OnRenameClicked);
                }
            }

            if (pageLabel != null)
                pageLabel.text = pages > 0 ? $"{page + 1} / {pages}" : "";

            if (prevButton != null) prevButton.interactable = page > 0;
            if (nextButton != null) nextButton.interactable = page < pages - 1;
        }

        private void ClearRows()
        {
            foreach (Transform child in listRoot)
                Destroy(child.gameObject);
        }
    }
}
