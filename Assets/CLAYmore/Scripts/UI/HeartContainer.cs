using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class HeartContainer : MonoBehaviour
    {
        public Heart[] hearts;

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<PlayerHpChangedEvent>(OnHpChanged);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<PlayerHpChangedEvent>(OnHpChanged);
        }

        private void OnHpChanged(PlayerHpChangedEvent e)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null) continue;
                bool inRange = i < e.MaxHp;
                hearts[i].gameObject.SetActive(inRange);
                if (inRange) hearts[i].SetAlive(i < e.Hp);
            }
        }
    }
}
