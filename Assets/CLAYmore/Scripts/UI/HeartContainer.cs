using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class HeartContainer : MonoBehaviour
    {
        public Heart heartPrefab;

        private Heart[] _hearts;

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
            // Инициализируем сердечки при первом событии или если MaxHp изменился
            if (_hearts == null || _hearts.Length != e.MaxHp)
            {
                foreach (Transform child in transform)
                    Destroy(child.gameObject);

                _hearts = new Heart[e.MaxHp];
                for (int i = 0; i < _hearts.Length; i++)
                    _hearts[i] = Instantiate(heartPrefab, transform);
            }

            for (int i = 0; i < _hearts.Length; i++)
                _hearts[i].SetAlive(i < e.Hp);
        }
    }
}
