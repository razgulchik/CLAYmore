using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CLAYmore
{
    public class ScoreRowUI : MonoBehaviour
    {
        public TextMeshProUGUI rankLabel;
        public TextMeshProUGUI playerLabel;
        public TextMeshProUGUI scoreLabel;
        public Button          renameButton;

        [Header("Backgrounds")]
        public Image defaultBackground;
        public Image playerBackground;

        public void Setup(int rank, string playerName, int score)
        {
            if (rankLabel   != null) rankLabel.text   = $"{rank}.";
            if (playerLabel != null) playerLabel.text = playerName;
            if (scoreLabel  != null) scoreLabel.text  = score.ToString();

            SetPlayerRow(false);
        }

        public void SetPlayerRow(bool isPlayer)
        {
            if (defaultBackground != null) defaultBackground.gameObject.SetActive(!isPlayer);
            if (playerBackground  != null) playerBackground.gameObject.SetActive(isPlayer);
        }

        public void ShowRenameButton(UnityAction callback)
        {
            if (renameButton == null) return;
            renameButton.gameObject.SetActive(true);
            renameButton.onClick.AddListener(callback);
        }
    }
}
