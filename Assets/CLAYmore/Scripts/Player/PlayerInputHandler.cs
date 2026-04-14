using Claymore;
using CLAYmore.ECS;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CLAYmore
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private InputSystem_Actions _inputActions;
        private bool _restartPending;

        private void Update()
        {
            if (_restartPending)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
            _inputActions.Player.Move.started  += HandleMove;
            _inputActions.Player.Move.canceled += HandleMoveReleased;
            _inputActions.UI.Enable();
            _inputActions.UI.Restart.performed  += HandleRestart;
            _inputActions.UI.Journal.performed  += HandleJournal;
            _inputActions.UI.Quit.performed     += HandleQuit;
        }

        private void OnDisable()
        {
            _inputActions.Player.Move.started   -= HandleMove;
            _inputActions.Player.Move.canceled  -= HandleMoveReleased;
            _inputActions.Player.Disable();
            _inputActions.UI.Restart.performed  -= HandleRestart;
            _inputActions.UI.Journal.performed  -= HandleJournal;
            _inputActions.UI.Quit.performed     -= HandleQuit;
            _inputActions.UI.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        private void HandleRestart(InputAction.CallbackContext context)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleJournal(InputAction.CallbackContext context)
        {
            World.Current?.Events.Publish(new JournalToggleEvent());
        }

        private void HandleQuit(InputAction.CallbackContext context)
        {
            Application.Quit();
        }

        private void HandleMove(InputAction.CallbackContext context)
        {
            Vector2 raw = context.ReadValue<Vector2>();

            Vector2Int direction = Mathf.Abs(raw.x) >= Mathf.Abs(raw.y)
                ? new Vector2Int((int)Mathf.Sign(raw.x), 0)
                : new Vector2Int(0, (int)Mathf.Sign(raw.y));

            World.Current?.Events.Publish(new PlayerMoveInputEvent { Direction = direction });
            World.Current?.Events.Publish(new PlayerMoveHeldEvent  { Direction = direction });
        }

        private void HandleMoveReleased(InputAction.CallbackContext context)
        {
            World.Current?.Events.Publish(new PlayerMoveHeldEvent { Direction = Vector2Int.zero });
        }
    }
}
