using UnityEngine;

namespace CLAYmore
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerHealth))]
    public class Player : MonoBehaviour
    {
        [HideInInspector] public PlayerMovement movement;
        [HideInInspector] public PlayerHealth   health;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            health   = GetComponent<PlayerHealth>();
        }
    }
}
