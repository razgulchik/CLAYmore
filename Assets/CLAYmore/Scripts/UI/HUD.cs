using UnityEngine;

namespace CLAYmore
{
    public class HUD : MonoBehaviour
    {
        [Header("Containers")]
        public HeartContainer      heartContainer;
        public CoinContainer       coinContainer;
        public ModifierChoiceUI    modifierChoiceUI;
        public IslandEdgeIndicator islandEdgeIndicator;
        public JournalUI           journalUI;
        public LeaderboardUI       leaderboardUI;

        private void Awake()
        {
            if (heartContainer      != null) heartContainer.gameObject.SetActive(true);
            if (coinContainer       != null) coinContainer.gameObject.SetActive(true);
            if (modifierChoiceUI    != null) modifierChoiceUI.gameObject.SetActive(true);
            if (islandEdgeIndicator != null) islandEdgeIndicator.gameObject.SetActive(true);
            if (journalUI           != null) journalUI.gameObject.SetActive(true);
            if (leaderboardUI       != null) leaderboardUI.gameObject.SetActive(true);
        }
    }
}
