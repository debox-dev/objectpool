using UnityEngine;

namespace DeBox.ObjectPool
{
    /// <summary>
    /// An inspector-space object pool you can place in the scene
    /// </summary>
    public class GameObjectPool : MonoBehaviour, IPool<GameObject>
    {
        [SerializeField] private GameObject prefab = null;
        [SerializeField] private int maxSize = 200;
        [SerializeField] private int preCache = 50;
        [SerializeField] private bool dontDestroyOnLoad = false;
        [SerializeField] private bool hideInInspectorOnRun = true;

        private DynamicGameObjectPool _internalPool = null;
        private Transform _poolContainer;

        private void Start()
        {
            _poolContainer = new GameObject("Instances").transform;
            _poolContainer.SetParent(transform);
            _poolContainer.gameObject.SetActive(false);
            _internalPool = GameObjectPoolManager.CreatePool(prefab, maxSize, preCache, false, _poolContainer);
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (hideInInspectorOnRun)
            {
                gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }
        }

        private void OnDestroy()
        {
            _internalPool.DepopulateAll();
        }

        /// <summary>
        /// Borrow an instance
        /// </summary>
        /// <returns>The borrowed instance</returns>
        public GameObject Borrow()
        {
            return _internalPool.Borrow();
        }

        /// <summary>
        /// Returns a borrowed instance to the pool
        /// </summary>
        /// <param name="obj">The returned instance</param>
        public void Revert(GameObject obj)
        {
            _internalPool.Revert(obj);
        }
    }
}
