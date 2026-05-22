using UnityEngine;

namespace CLAYmore
{
    public class HUD : MonoBehaviour
    {
        [Header("Containers")]
        public HeartContainer      heartContainer;
        public CoinContainer       coinContainer;
        public ExpanderContainer   expanderContainer;
        public LevelUpContainer    levelUpContainer;
        public ModifierChoiceUI    modifierChoiceUI;
        public IslandEdgeIndicator islandEdgeIndicator;
        public JournalUI           journalUI;
        public LeaderboardUI       leaderboardUI;
        public SessionTimerUI        sessionTimerUI;
        public WaveAnnouncementUI    waveAnnouncementUI;
        public WaveCountdownUI       waveCountdownUI;
        public PauseButtonContainer pauseButtonContainer;

        private void Awake()
        {
            if (pauseButtonContainer != null) pauseButtonContainer.gameObject.SetActive(true);
            if (heartContainer      != null) heartContainer.gameObject.SetActive(true);
            if (coinContainer       != null) coinContainer.gameObject.SetActive(true);
            if (expanderContainer   != null) expanderContainer.gameObject.SetActive(true);
            if (levelUpContainer    != null) levelUpContainer.gameObject.SetActive(true);
            if (modifierChoiceUI    != null) modifierChoiceUI.gameObject.SetActive(true);
            if (islandEdgeIndicator != null) islandEdgeIndicator.gameObject.SetActive(true);
            if (journalUI           != null) journalUI.gameObject.SetActive(true);
            if (leaderboardUI       != null) leaderboardUI.gameObject.SetActive(true);
            if (sessionTimerUI       != null) sessionTimerUI.gameObject.SetActive(true);
            if (waveAnnouncementUI   != null) waveAnnouncementUI.gameObject.SetActive(true);
            if (waveCountdownUI      != null) waveCountdownUI.gameObject.SetActive(true);
        }
    }
}
