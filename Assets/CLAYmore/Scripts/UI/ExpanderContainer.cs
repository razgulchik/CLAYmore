using CLAYmore.ECS;
using TMPro;
using UnityEngine;

namespace CLAYmore
{
    public class ExpanderContainer : MonoBehaviour
    {
        public TextMeshProUGUI expandersText;

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<ExpanderBalanceChangedEvent>(OnExpanderBalanceChanged);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<ExpanderBalanceChangedEvent>(OnExpanderBalanceChanged);
        }

        private void OnExpanderBalanceChanged(ExpanderBalanceChangedEvent e) => expandersText.SetText(e.NewBalance.ToString());
    }
}
