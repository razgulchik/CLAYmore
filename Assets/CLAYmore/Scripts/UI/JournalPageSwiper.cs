using UnityEngine;
using UnityEngine.InputSystem;

namespace CLAYmore
{
    /// <summary>
    /// Swipe-based page navigation for the mobile Journal panel.
    /// Attach to the journal panel or any of its children.
    /// Assign the three section roots in order: CharacterInfo, ModifierInfo, GameInfo.
    /// </summary>
    public class JournalPageSwiper : MonoBehaviour
    {
        [Tooltip("Section roots in display order: CharacterInfo, ModifierInfo, GameInfo")]
        public GameObject[] pages;

        [Tooltip("Minimum horizontal swipe distance in pixels to flip a page")]
        public float minSwipeDistance = 60f;

        private static int _currentPage = 0;

        private Vector2 _touchStart;
        private bool    _tracking;

        private void OnEnable()
        {
            ShowPage(_currentPage);
        }

        private void Update()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null) return;

            var touch = touchscreen.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                _touchStart = touch.position.ReadValue();
                _tracking   = true;
            }
            else if (touch.press.wasReleasedThisFrame && _tracking)
            {
                _tracking = false;
                Vector2 delta = touch.position.ReadValue() - _touchStart;

                float absX = Mathf.Abs(delta.x);
                float absY = Mathf.Abs(delta.y);

                // Only react to predominantly horizontal swipes above threshold.
                if (absX < minSwipeDistance || absY > absX) return;

                if (delta.x < 0)
                    Navigate(+1);  // swipe left → next page
                else
                    Navigate(-1);  // swipe right → previous page
            }
            else if (!touch.press.isPressed)
            {
                _tracking = false;
            }

#if UNITY_EDITOR
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.rightArrowKey.wasPressedThisFrame) Navigate(+1);
                if (kb.leftArrowKey.wasPressedThisFrame)  Navigate(-1);
            }
#endif
        }

        private void Navigate(int direction)
        {
            if (pages == null || pages.Length == 0) return;
            _currentPage = (_currentPage + direction + pages.Length) % pages.Length;
            ShowPage(_currentPage);
        }

        private void ShowPage(int index)
        {
            if (pages == null) return;
            for (int i = 0; i < pages.Length; i++)
                if (pages[i] != null)
                    pages[i].SetActive(i == index);
        }
    }
}
