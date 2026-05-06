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
        [Header("Movement Timing")]
        [SerializeField] private float _moveTime         = 0.15f;
        [SerializeField] private float _bounceReturnTime = 0.1f;

        [Header("Hit Shake")]
        [SerializeField] private float _shakeStrength = 0.08f;
        [SerializeField] private float _shakeDuration  = 0.2f;
        [SerializeField] private int   _shakeVibrato   = 10;

        [Header("Visual Root")]
        [SerializeField] private Transform      _visualRoot;
        [SerializeField] private SpriteRenderer _playerSprite;
        [SerializeField] private Sprite         _frontSprite;
        [SerializeField] private Sprite         _backSprite;

        [Header("Whirlwind VFX")]
        [SerializeField] private WhirlVFXController _whirlVFX;


        public Vector2Int FacingDirection => _movement.FacingDirection;

        private IslandGenerator   _islandGenerator;
        private Entity            _entity;
        private MovementComponent _movement;
        private Weapon            _weapon;
        private Transform         _weaponTransform;
        private SpriteRenderer    _weaponSprite;
        private Vector3           _weaponDefaultLocalPos;

        public void Init(IslandGenerator islandGenerator, float moveTime, float bounceReturnTime)
        {
            _islandGenerator  = islandGenerator;
            _moveTime         = moveTime;
            _bounceReturnTime = bounceReturnTime;
            _movement.MoveTime = moveTime;
        }

        private void Awake()
        {
            _entity = gameObject.GetComponent<Entity>();
            if (_entity == null) _entity = gameObject.AddComponent<Entity>();
            _movement = _entity.Add(new MovementComponent
            {
                FacingDirection = Vector2Int.down,
                MoveTime        = _moveTime,
            });

            _weapon = GetComponentInChildren<Weapon>();
            if (_weapon != null)
            {
                _weaponTransform = _weapon.transform;
                _weaponSprite    = _weapon.WeaponRenderer;
            }
        }

        private void Start()
        {
            if (_weaponTransform != null)
                _weaponDefaultLocalPos = _weaponTransform.localPosition;
        }

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<PlayerMoveResultEvent>(OnMoveResult);
            World.Current?.Events.Subscribe<PlayerStatsChangedEvent>(OnStatsChanged);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<PlayerMoveResultEvent>(OnMoveResult);
            World.Current?.Events.Unsubscribe<PlayerStatsChangedEvent>(OnStatsChanged);
        }

        private void OnStatsChanged(PlayerStatsChangedEvent evt)
        {
            if (evt.MoveTime > 0f)
            {
                _moveTime          = evt.MoveTime;
                _movement.MoveTime = evt.MoveTime;
            }

            if (evt.BounceReturnTime > 0f)
                _bounceReturnTime = evt.BounceReturnTime;

            _weapon?.SetReach(evt.LongSwordReach);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnMoveResult(PlayerMoveResultEvent evt)
        {
            UpdateWeaponOrientation(evt.Direction);
            UpdateSpriteForDirection(evt.Direction);

            Vector3 target = evt.Target;
            target.z = transform.position.z;

            switch (evt.MoveType)
            {
                case MoveType.Walk:
                    ShowWeapon();
                    transform.DOMove(target, _moveTime)
                        .OnComplete(() =>
                        {
                            if (_islandGenerator != null)
                                _islandGenerator.SetPlayerTileFromWorldPos(transform.position);
                            if (_entity.Has<PlayerStatsComponent>() && _entity.Get<PlayerStatsComponent>().HasWhirlwind
                                && _whirlVFX != null)
                                _whirlVFX.Play();
                            HideWeapon();
                            _movement.IsMoving = false;
                        });
                    break;

                case MoveType.Bounce:
                    ShowWeapon();
                    Vector3 bounceReturn = evt.SlideTarget != Vector3.zero ? evt.SlideTarget : transform.position;
                    bounceReturn.z = transform.position.z;
                    transform.DOMove(target, _moveTime)
                        .OnComplete(() =>
                        {
                            HideWeapon();
                            if (_playerSprite != null)
                                _playerSprite.transform.DOPunchPosition(Vector3.one * _shakeStrength, _shakeDuration, _shakeVibrato, elasticity: 0f);
                            transform.DOMove(bounceReturn, _bounceReturnTime)
                                .OnComplete(() =>
                                {
                                    if (_islandGenerator != null)
                                        _islandGenerator.SetPlayerTileFromWorldPos(transform.position);
                                    if (_entity.Has<PlayerStatsComponent>() && _entity.Get<PlayerStatsComponent>().HasWhirlwind
                                        && _whirlVFX != null)
                                        _whirlVFX.Play();
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

        private void UpdateSpriteForDirection(Vector2Int direction)
        {
            bool facingUp = direction.y > 0;
            if (_playerSprite != null)
                _playerSprite.sprite = facingUp ? _backSprite : _frontSprite;
            if (_weapon != null && _playerSprite != null)
                _weapon.SetSortingOrder(_playerSprite.sortingOrder + (facingUp ? -1 : 1));
        }

        private void UpdateWeaponOrientation(Vector2Int direction)
        {
            if (_weaponTransform == null) return;

            if (direction.x != 0)
            {
                float scaleX = direction.x < 0 ? -1f : 1f;
                var flipTarget = _visualRoot != null ? _visualRoot : transform;
                flipTarget.localScale = new Vector3(scaleX, 1f, 1f);
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
