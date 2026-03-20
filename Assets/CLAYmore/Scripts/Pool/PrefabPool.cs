using UnityEngine;
using UnityEngine.Pool;

namespace CLAYmore
{
    /// <summary>
    /// Generic self-expanding prefab pool backed by Unity's built-in ObjectPool.
    /// Attach to a scene GameObject and assign the prefab in the Inspector.
    /// </summary>
    public class PrefabPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxSize = 50;

        private ObjectPool<GameObject> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<GameObject>(
                createFunc:       CreateInstance,
                actionOnGet:      null,                        // activated manually after position is set
                actionOnRelease:  obj => obj.SetActive(false),
                actionOnDestroy:  obj => Destroy(obj),
                collectionCheck:  false,
                defaultCapacity:  defaultCapacity,
                maxSize:          maxSize
            );
        }

        /// <summary>Retrieve an instance from the pool, placed at the given world position.</summary>
        public GameObject Get(Vector3 position)
        {
            GameObject obj = _pool.Get();       // still inactive (actionOnGet = null)
            obj.transform.position = position;  // set position before any rendering
            obj.SetActive(true);                // activate only after correct position
            return obj;
        }

        /// <summary>Return an instance to the pool.</summary>
        public void Return(GameObject obj)
        {
            if (obj == null) return;
            _pool.Release(obj);
        }

        private GameObject CreateInstance()
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            return obj;
        }
    }
}
