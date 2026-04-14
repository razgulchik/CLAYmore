using CLAYmore.ECS;
using TMPro;
using UnityEngine;

namespace CLAYmore
{
    public class CoinContainer : MonoBehaviour
    {
        public TextMeshProUGUI coinsText;

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<CoinBalanceChangedEvent>(OnCoinBalanceChanged);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<CoinBalanceChangedEvent>(OnCoinBalanceChanged);
        }

        private void OnCoinBalanceChanged(CoinBalanceChangedEvent e) => coinsText.SetText(e.NewBalance.ToString());
    }
}
