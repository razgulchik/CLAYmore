using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CLAYmore
{
    /// <summary>
    /// One row in the leaderboard panel.
    /// Assign rankLabel, playerLabel, scoreLabel in the Inspector (on BestScore Row prefab).
    /// </summary>
    public class ScoreRowUI : MonoBehaviour
    {
        public TextMeshProUGUI rankLabel;
        public TextMeshProUGUI playerLabel;
        public TextMeshProUGUI scoreLabel;
        public Button          renameButton;

        public void Setup(int rank, string playerName, int score)
        {
            if (rankLabel   != null) rankLabel.text   = $"#{rank}";
            if (playerLabel != null) playerLabel.text = playerName;
            if (scoreLabel  != null) scoreLabel.text  = score.ToString();
        }

        public void Highlight(Color color)
        {
            if (rankLabel   != null) rankLabel.color   = color;
            if (playerLabel != null) playerLabel.color = color;
            if (scoreLabel  != null) scoreLabel.color  = color;
        }

        public void ShowRenameButton(UnityAction callback)
        {
            if (renameButton == null) return;
            renameButton.gameObject.SetActive(true);
            renameButton.onClick.AddListener(callback);
        }
    }
}
