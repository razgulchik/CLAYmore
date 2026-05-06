using UnityEngine;

namespace CLAYmore
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer longSwordRenderer;
        [SerializeField] private Animator slashAnimator;
        [SerializeField] private float _longSwordUnitWidth = 0f;

        private bool _isLong;
        private float _longSwordBaseWidth;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (slashAnimator == null)
                slashAnimator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (longSwordRenderer != null)
                _longSwordBaseWidth = longSwordRenderer.size.x;
            Hide();
            if (longSwordRenderer != null)
                longSwordRenderer.enabled = false;
        }

        public SpriteRenderer WeaponRenderer => _isLong ? longSwordRenderer : spriteRenderer;

        public void SetSortingOrder(int order)
        {
            if (spriteRenderer    != null) spriteRenderer.sortingOrder    = order;
            if (longSwordRenderer != null) longSwordRenderer.sortingOrder = order;
        }

        public void Show()
        {
            var active = _isLong ? longSwordRenderer : spriteRenderer;
            if (active != null) active.enabled = true;
            if (slashAnimator != null) slashAnimator.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (spriteRenderer     != null) spriteRenderer.enabled     = false;
            if (longSwordRenderer  != null) longSwordRenderer.enabled  = false;
            if (slashAnimator      != null) slashAnimator.gameObject.SetActive(false);
        }

        public void SetReach(int reach)
        {
            _isLong = reach > 0;

            if (!_isLong || longSwordRenderer == null) return;

            float unitW = _longSwordUnitWidth > 0f ? _longSwordUnitWidth
                : spriteRenderer != null ? spriteRenderer.sprite.bounds.size.x
                : 1f;
            longSwordRenderer.drawMode = SpriteDrawMode.Sliced;
            longSwordRenderer.size     = new Vector2(_longSwordBaseWidth + unitW * (reach - 1), longSwordRenderer.size.y);
        }
    }
}
