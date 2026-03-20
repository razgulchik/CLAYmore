using UnityEngine;

namespace CLAYmore
{
    public class Heart : MonoBehaviour
    {
        public void SetAlive(bool alive) => gameObject.SetActive(alive);
    }
}
