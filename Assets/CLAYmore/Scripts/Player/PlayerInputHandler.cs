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
        private bool _inUIMode;
        private Vector2Int _lastMoveDirection;

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
            _inputActions.Player.Move.started   += HandleMoveStarted;
            _inputActions.Player.Move.performed += HandleMovePerformed;
            _inputActions.Player.Move.canceled  += HandleMoveReleased;
            _inputActions.UI.Enable();
            _inputActions.UI.Restart.performed  += HandleRestart;
            _inputActions.UI.Journal.performed  += HandleJournal;
            _inputActions.UI.Quit.performed     += HandleQuit;

            World.Current?.Events.Subscribe<ChestActivatedEvent>(OnChestActivated);
            World.Current?.Events.Subscribe<ModifierChosenEvent>(OnModifierChosen);
            World.Current?.Events.Subscribe<ModifierSkippedEvent>(OnModifierSkipped);
        }

        private void OnDisable()
        {
            _inputActions.Player.Move.started   -= HandleMoveStarted;
            _inputActions.Player.Move.performed -= HandleMovePerformed;
            _inputActions.Player.Move.canceled  -= HandleMoveReleased;
            _inputActions.Player.Disable();
            _inputActions.UI.Restart.performed  -= HandleRestart;
            _inputActions.UI.Journal.performed  -= HandleJournal;
            _inputActions.UI.Quit.performed     -= HandleQuit;
            _inputActions.UI.Disable();

            World.Current?.Events.Unsubscribe<ChestActivatedEvent>(OnChestActivated);
            World.Current?.Events.Unsubscribe<ModifierChosenEvent>(OnModifierChosen);
            World.Current?.Events.Unsubscribe<ModifierSkippedEvent>(OnModifierSkipped);

            if (_inUIMode)
            {
                _inputActions.UI.Navigate.started -= HandleMoveStarted;
                _inUIMode = false;
            }
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        // ── Action Map switching ──────────────────────────────────────────────

        private void OnChestActivated(ChestActivatedEvent _)
        {
            _inputActions.Player.Disable();
            _inputActions.UI.Navigate.started += HandleMoveStarted;
            _inUIMode = true;
        }

        private void OnModifierChosen(ModifierChosenEvent _) => ExitUIMode();
        private void OnModifierSkipped(ModifierSkippedEvent _) => ExitUIMode();

        private void ExitUIMode()
        {
            if (!_inUIMode) return;
            _inputActions.UI.Navigate.started -= HandleMoveStarted;
            _inputActions.Player.Enable();
            _lastMoveDirection = Vector2Int.zero;
            _inUIMode = false;
        }

        // ── Input handlers ────────────────────────────────────────────────────

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

        private void HandleMoveStarted(InputAction.CallbackContext context)
        {
            Vector2Int direction = ToGridDirection(context.ReadValue<Vector2>());
            _lastMoveDirection = direction;
            PublishMove(direction);
        }

        private void HandleMovePerformed(InputAction.CallbackContext context)
        {
            Vector2Int direction = ToGridDirection(context.ReadValue<Vector2>());
            if (direction == Vector2Int.zero || direction == _lastMoveDirection) return;
            _lastMoveDirection = direction;
            PublishMove(direction);
        }

        private void HandleMoveReleased(InputAction.CallbackContext context)
        {
            _lastMoveDirection = Vector2Int.zero;
            World.Current?.Events.Publish(new PlayerMoveHeldEvent { Direction = Vector2Int.zero });
        }

        private static Vector2Int ToGridDirection(Vector2 raw)
        {
            return Mathf.Abs(raw.x) >= Mathf.Abs(raw.y)
                ? new Vector2Int((int)Mathf.Sign(raw.x), 0)
                : new Vector2Int(0, (int)Mathf.Sign(raw.y));
        }

        private static void PublishMove(Vector2Int direction)
        {
            World.Current?.Events.Publish(new PlayerMoveInputEvent { Direction = direction });
            World.Current?.Events.Publish(new PlayerMoveHeldEvent  { Direction = direction });
        }
    }
}
