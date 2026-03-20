using UnityEngine;

namespace CLAYmore
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem sparks;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (sparks == null)
                sparks = GetComponentInChildren<ParticleSystem>();
        }

        private void Start()
        {
            Hide();
        }

        public void Show()
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
            sparks?.Play();
        }

        public void Hide()
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
        }
    }
}
