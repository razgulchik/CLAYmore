using DG.Tweening;
using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// View: animates the player based on PlayerMoveResultEvent published by MovementSystem.
    /// No movement logic lives here.
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        public IslandGenerator islandGenerator;
        [SerializeField] private Transform spriteTransform;

        [Header("Movement Timing")]
        public float moveTime         = 0.15f;
        public float bounceTime       = 0.1f;
        public float bounceReturnTime = 0.05f;

        [Header("Hit Shake")]
        public float shakeStrength  = 0.08f;
        public float shakeDuration  = 0.2f;
        public int   shakeVibrato   = 10;

        public Vector2Int FacingDirection => _movement.FacingDirection;

        private MovementComponent _movement;
        private Weapon _weapon;
        private Transform _weaponTransform;
        private Vector3 _weaponDefaultLocalPos;

        private void Awake()
        {
            var entity = gameObject.GetComponent<Entity>() ?? gameObject.AddComponent<Entity>();
            _movement = entity.Add(new MovementComponent
            {
                FacingDirection = Vector2Int.down
            });

            _weapon = GetComponentInChildren<Weapon>();
            if (_weapon != null)
                _weaponTransform = _weapon.transform;


        }

        private void Start()
        {
            if (_weaponTransform != null)
                _weaponDefaultLocalPos = _weaponTransform.localPosition;
        }

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<PlayerMoveResultEvent>(OnMoveResult);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<PlayerMoveResultEvent>(OnMoveResult);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnMoveResult(PlayerMoveResultEvent evt)
        {
            UpdateWeaponOrientation(evt.Direction);

            // Preserve the player's z (rendering layer)
            Vector3 target = evt.Target;
            target.z = transform.position.z;

            switch (evt.MoveType)
            {
                case MoveType.Walk:
                    ShowWeapon();
                    transform.DOMove(target, moveTime)
                        .OnComplete(() =>
                        {
                            if (islandGenerator != null)
                                islandGenerator.SetPlayerTileFromWorldPos(transform.position);
                            HideWeapon();
                            _movement.IsMoving = false;
                        });
                    break;

                case MoveType.Bounce:
                    ShowWeapon();
                    Vector3 bounceReturn = evt.SlideTarget != Vector3.zero ? evt.SlideTarget : transform.position;
                    bounceReturn.z = transform.position.z;
                    transform.DOMove(target, bounceTime)
                        .OnComplete(() =>
                        {
                            HideWeapon();
                            if (spriteTransform != null)
                                spriteTransform.DOPunchPosition(Vector3.one * shakeStrength, shakeDuration, shakeVibrato, elasticity: 0f);
                            transform.DOMove(bounceReturn, bounceReturnTime)
                                .OnComplete(() =>
                                {
                                    if (islandGenerator != null)
                                        islandGenerator.SetPlayerTileFromWorldPos(transform.position);
                                    _movement.IsMoving = false;
                                });
                        });
                    break;

                case MoveType.Blocked:
                    HideWeapon();
                    break;
            }
        }

        private void ShowWeapon() => _weapon?.Show();
        private void HideWeapon() => _weapon?.Hide();

        private void UpdateWeaponOrientation(Vector2Int direction)
        {
            if (_weaponTransform == null) return;

            if (direction.x != 0)
            {
                float scaleX = direction.x < 0 ? -1f : 1f;
                transform.localScale = new Vector3(scaleX, 1f, 1f);
                _weaponTransform.SetLocalPositionAndRotation(_weaponDefaultLocalPos, Quaternion.identity);
            }
            else if (direction.y != 0)
            {
                float offset = _weaponDefaultLocalPos.x;
                Vector3 pos = new(0f, direction.y > 0 ? offset : -offset, _weaponDefaultLocalPos.z);
                Quaternion rot = Quaternion.Euler(0f, 0f, direction.y > 0 ? 90f : -90f);
                _weaponTransform.SetLocalPositionAndRotation(pos, rot);
            }
        }
    }
}
