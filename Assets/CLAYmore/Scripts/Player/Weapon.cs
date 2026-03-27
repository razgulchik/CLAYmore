using UnityEngine;

namespace CLAYmore
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator slashAnimator;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (slashAnimator == null)
                slashAnimator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            Hide();
        }

        public void Show()
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
            if (slashAnimator != null)
                slashAnimator.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
            if (slashAnimator != null)
                slashAnimator.gameObject.SetActive(false);
        }
    }
}
