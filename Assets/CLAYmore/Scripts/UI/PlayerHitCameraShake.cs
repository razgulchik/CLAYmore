using CLAYmore.ECS;
using Unity.Cinemachine;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Triggers a Cinemachine impulse shake whenever the player takes damage.
    /// Attach directly to the CinemachineCamera. Also add CinemachineImpulseListener to the same camera.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class PlayerHitCameraShake : MonoBehaviour
    {
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
        }
    }
}
