using CLAYmore.ECS;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CLAYmore
{
    /// <summary>
    /// Detects touch swipes via the new Input System and publishes the same move events
    /// as PlayerInputHandler. One swipe = one move.
    /// Add to the player alongside PlayerInputHandler.
    /// </summary>
    public class SwipeInputHandler : MonoBehaviour
    {
        [Tooltip("Minimum swipe distance in pixels to register a move")]
        public float minSwipeDistance = 50f;

        private Vector2 _touchStart;
        private bool    _tracking;

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
                if (delta.magnitude < minSwipeDistance) return;

                Vector2Int dir = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                    ? new Vector2Int((int)Mathf.Sign(delta.x), 0)
                    : new Vector2Int(0, (int)Mathf.Sign(delta.y));

                World.Current?.Events.Publish(new PlayerMoveInputEvent { Direction = dir });
                World.Current?.Events.Publish(new PlayerMoveHeldEvent  { Direction = dir });
                World.Current?.Events.Publish(new PlayerMoveHeldEvent  { Direction = Vector2Int.zero });
            }
            else if (!touch.press.isPressed)
            {
                _tracking = false;
            }
        }
    }
}
