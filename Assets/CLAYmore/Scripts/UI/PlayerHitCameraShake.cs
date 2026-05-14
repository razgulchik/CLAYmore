using CLAYmore.ECS;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Triggers a Cinemachine impulse shake + brief hitstop whenever the player takes damage.
    /// Attach to the CinemachineCamera (also needs CinemachineImpulseListener).
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class PlayerHitCameraShake : MonoBehaviour
    {
        [SerializeField] private float _hitstopDuration = 0.05f;

        private CinemachineImpulseSource _impulse;

        private void Awake()
        {
            _impulse = GetComponent<CinemachineImpulseSource>();
        }

        private void OnEnable()
            => World.Current?.Events.Subscribe<EntityDamagedEvent>(OnEntityDamaged);

        private void OnDisable()
            => World.Current?.Events.Unsubscribe<EntityDamagedEvent>(OnEntityDamaged);

        private void OnEntityDamaged(EntityDamagedEvent evt)
        {
            if (!evt.Entity.Has<PlayerStatsComponent>()) return;
            _impulse.GenerateImpulse();
            StartCoroutine(Hitstop());
        }

        private IEnumerator Hitstop()
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(_hitstopDuration);
            Time.timeScale = 1f;
        }
    }
}
