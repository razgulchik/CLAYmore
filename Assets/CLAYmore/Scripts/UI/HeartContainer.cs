using UnityEngine;

namespace CLAYmore
{
    public class HeartContainer : MonoBehaviour
    {
        public PlayerHealth playerHealth;
        public Heart heartPrefab;

        private Heart[] _hearts;

        private void Start()
        {
            _hearts = new Heart[playerHealth.maxHp];
            for (int i = 0; i < _hearts.Length; i++)
                _hearts[i] = Instantiate(heartPrefab, transform);

            playerHealth.OnDamaged += OnDamaged;
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.OnDamaged -= OnDamaged;
        }

        private void OnDamaged(int hp)
        {
            for (int i = 0; i < _hearts.Length; i++)
                _hearts[i].SetAlive(i < hp);
        }
    }
}
