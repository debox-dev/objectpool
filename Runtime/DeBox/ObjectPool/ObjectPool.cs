using UnityEngine;
using System;
using System.Collections.Generic;




namespace DeBox.ObjectPool
{
    /// <summary>
    /// Thrown when trying to create two pools for the same prefab
    /// </summary>
    public class PoolAlreadyExistsForPrefabException : Exception {}
    
    /// <summary>
    ///A singleton instance of this will be auto-created in the static constructor
    ///
    ///Immediate Usage:
    ///     
    ///
    ///GameObject newInstance = GameObjectPoolManager.Borrow(GameObjectPrefab);
    ///MyScript newInstance2 = GameObjectPoolManager.Borrow<MyScript>(myScriptPrefab, newParent: Transform);
    ///
    ///
    /// When we're done, we give the objects back to the pool. The pool manager will auto-deactivate the gameObject of each.
    ///GameObjectPoolManager.Revert(newInstance);
    ///GameObjectPoolManager.Revert(newInstance2);
    /// </summary>
    public class GameObjectPoolManager : MonoBehaviour
    {
        private Transform _transform;

        private static GameObjectPoolManager _singletonInstance = null;

        private static GameObjectPoolManager Main { 
            get { 
                if (_singletonInstance == null)
                {
                    var instance = new GameObject("ObjectPoolManager");
                    DontDestroyOnLoad(instance);
                    instance.hideFlags = HideFlags.HideAndDontSave;
                    instance.SetActive(false);
                    _singletonInstance = instance.AddComponent<GameObjectPoolManager>();
                }
                return _singletonInstance; 
            }
            set {
                if (_singletonInstance != null && value != null)
                {
                    throw new Exception("Instance already exists.");
                }
                _singletonInstance = value; 
            }
        }

        void Awake()
        {
            _transform = transform;
            _transform.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;	
            Main = this;
        }

        private void OnDestroy()
        {
            if (Main == this)
            {
                Main = null;
            }
        }

        /// <summary>
        /// Borrow a GameObject of the given prefab
        ///
        /// If no pool exists for this prefab, a new pool will be immediately created, but not pre-cached
        /// </summary>
        /// <param name="prefab">Prefab of the borrowed instance</param>
        /// <returns>Borrowed instance</returns>
        public static GameObject Borrow(GameObject prefab)
        {
            return Main._borrow(prefab, null);
        }

        /// <summary>
        /// Borrows an instance of the give prefab and 
        ///
        /// If there is no such component on the instance, null will be returned and no instance will be borrowed
        /// </summary>
        /// <param name="prefab">Prefab of the borrowed instance</param>
        /// <param name="newParent">Target parent for the borrowed instance transform</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Borrowed instance component of type T or null if no such component</returns>
        public static T Borrow<T>(T prefab, Transform newParent) where T : MonoBehaviour
        {
            var instance = Main._borrow(prefab.gameObject, newParent);
            var instanceComponent = instance.GetComponent<T>();
            if (instanceComponent == null)
            {
                Revert(instance);
            }
            return instanceComponent;
        }

        /// <summary>
        /// Create a pool for a prefab. This is useful if you know how you want to cache your pools before you borrow
        /// any prefabs (and is very recommended)
        /// </summary>
        /// <param name="prefab">The prefab to create a pool for</param>
        /// <param name="size">Maximum size of the pool</param>
        /// <param name="precacheSize">PreCached amount of instances to create immediately</param>
        /// <param name="failIfExists">If true, will throw an exception if called twice for the same prefab</param>
        /// <param name="pooledParent">
        /// The parent transform to use for cached objects. If null then then a global, hidden transform is used.
        /// </param>
        /// <typeparam name="T">Any component type</typeparam>
        /// <returns>The created or existing pool for the given prefab</returns>
        /// <exception cref="Exception">
        /// Thrown if failIfExists is true and the given prefab already has a pool
        /// </exception>
        public static DynamicGameObjectPool CreatePool<T>(T prefab, int size, int precacheSize = -1, bool failIfExists = true, Transform pooledParent = null) where T : UnityEngine.Component
        {
            return CreatePool(prefab.gameObject, size, precacheSize, failIfExists, pooledParent);
        }
        
        /// <summary>
        /// Create a pool for a prefab. This is useful if you know how you want to cache your pools before you borrow
        /// any prefabs (and is very recommended)
        /// </summary>
        /// <param name="prefab">The prefab to create a pool for</param>
        /// <param name="size">Maximum size of the pool</param>
        /// <param name="precacheSize">PreCached amount of instances to create immediately</param>
        /// <param name="failIfExists">If true, will throw an exception if called twice for the same prefab</param>
        /// <param name="pooledParent">
        /// The parent transform to use for cached objects. If null then then a global, hidden transform is used.
        /// </param>
        /// <typeparam name="T">Any component type</typeparam>
        /// <returns>The created or existing pool for the given prefab</returns>
        /// <exception cref="Exception">
        /// Thrown if failIfExists is true and the given prefab already has a pool
        /// </exception>
        public static DynamicGameObjectPool CreatePool(GameObject prefab, int size, int precacheSize = -1, bool failIfExists = true, Transform pooledParent = null)
        {
            var gameObject = prefab;
            if (Main._pools.ContainsKey(prefab.gameObject))
            {
                if (!failIfExists)
                {
                    return GetPool(gameObject);
                }
                throw new PoolAlreadyExistsForPrefabException();
            }
            if (pooledParent == null)
            {
                pooledParent = Main.transform;
            }
            Main._pools[gameObject] = new DynamicGameObjectPool(gameObject, size, pooledParent);
            if (precacheSize < 0)
            {
                precacheSize = size;
            }
            Main._pools[gameObject].PreCache(precacheSize);
            return GetPool(gameObject);
        }



        public static DynamicGameObjectPool GetPool(GameObject prefab)
        {
            return Main.GetPoolFor(prefab);
        }

        public static T Borrow<T>(T prefab) where T : MonoBehaviour
        {
            return Borrow<T>(prefab, null);
        }

        public static void Revert(GameObject prefab, GameObject instance)
        {
            Main._revert(prefab, instance);
        }

        public static void Revert(MonoBehaviour instance)
        {
            Revert(instance.gameObject);
        }

        public static void DestroyPool(GameObject prefab)
        {
            Main._destoryPool(prefab);
        }

        private void _destoryPool(GameObject prefab)
        {
            var p = GetPoolFor(prefab);
            _pools.Remove(prefab);
            p.DepopulateAll();
        }

        public static void Revert(GameObject instance)
        {
            PoolPrefab poolPrefab = instance.GetComponent<PoolPrefab>();
            Revert(poolPrefab.SourcePrefab, instance);
        }

        public static void SetMaxPoolSize(GameObject prefab, int maxSize)
        {
            Main._setMaxPoolSize(prefab, maxSize);
        }

        public int defaultMaxPoolSize = 1000;
        private Dictionary<UnityEngine.Object, DynamicGameObjectPool> _pools = new Dictionary<UnityEngine.Object, DynamicGameObjectPool>();



		private GameObject _borrow(GameObject prefab, Transform newParent)
        {
            var pool = GetPoolFor(prefab);
            GameObject item = pool.Borrow(newParent);
            //PoolPrefab poolPrefab = DynamicGameObjectPool.GetOrAddPoolPrefabComponent(item);
            item.transform.SetParent(newParent, false);
            return item;
        }

        private void _revert(GameObject prefab, GameObject instance)
        {            
            GetPoolFor(prefab).Revert(instance);
        }

        private void _setMaxPoolSize(GameObject prefab, int maxSize)
        {
            GetPoolFor(prefab).MaxSize = maxSize;
        }

        private DynamicGameObjectPool GetPoolFor(GameObject prefab)
        {
            if (!_pools.ContainsKey(prefab))
            {
                _pools.Add(prefab, new DynamicGameObjectPool(prefab, defaultMaxPoolSize, this.transform));
            }
            return _pools[prefab];
        }
    }
}