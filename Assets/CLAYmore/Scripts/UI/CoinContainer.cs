using TMPro;
using UnityEngine;

namespace CLAYmore
{
    public class CoinContainer : MonoBehaviour
    {
        public Economy economy;
        public TextMeshProUGUI coinsText;

        private void Start()
        {
            economy.OnChanged += OnCoinsChanged;
            OnCoinsChanged(economy.Coins);
        }

        private void OnDestroy()
        {
            if (economy != null)
                economy.OnChanged -= OnCoinsChanged;
        }

        private void OnCoinsChanged(int coins) => coinsText.SetText(coins.ToString());
    }
}
