using UnityEngine;

namespace CLAYmore
{
    public class HUD : MonoBehaviour
    {
        public HeartContainer heartContainer;
        public CoinContainer coinContainer;

        public void Setup(PlayerHealth playerHealth)
        {
            if (heartContainer != null)
                heartContainer.playerHealth = playerHealth;
        }
    }
}
