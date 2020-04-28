using UnityEngine;

namespace DeBox.ObjectPool
{
	/// <summary>
	/// A game object pool that can dynamically instantiate Unity game objects from a given prefab
	/// 
	/// This class describes a Pool of a single GameObject prefab type 
	/// </summary>
	public class DynamicGameObjectPool : DynamicPool<GameObject>
	{
		private readonly GameObject _prefab;
        private readonly Transform _parent = null;
        
        /// <summary>
        /// Create a new DynamicGameObjectPool
        /// </summary>
        /// <param name="prefab">The source prefab</param>
        /// <param name="maxSize">Maximum pool size</param>
        /// <param name="parent">Paren transform to hold the pooled instances</param>
        public DynamicGameObjectPool(GameObject prefab, int maxSize, Transform parent)
			: base(maxSize)
		{
			_prefab = prefab;
            _parent = parent;
		}
        
        /// <summary>
        /// Borrow an instance from the pool and get its component of type T
        /// If the object does not have a component of type T - then null will be returned and no object will be
        /// borrowed
        /// </summary>
        /// <param name="parent">The target parent to place the instance under when borrowed</param>
        /// <param name="worldPositionStays">Acts like worldPositionStays of Transform.SetParent</param>
        /// <typeparam name="T">Component type of the pooled object</typeparam>
        /// <returns>The component of the borrowed object</returns>
        public T Borrow<T>(Transform parent = null, bool worldPositionStays = true) where T : MonoBehaviour
        {
            var obj = Borrow();
            T instanceComponent = obj.GetComponent<T>();
            if (instanceComponent == null)
            {
                base.Revert(obj);
                return null;
            }
            obj.transform.SetParent(parent, worldPositionStays);
            return instanceComponent;
        }

        /// <summary>
        /// Borrow an instance from the pool, places it under a parent transform and get its GameObject
        /// </summary>
        /// <param name="parent">Target parent to place the instance under after borrowing, can be null</param>
        /// <returns>GameObject of the borrowed object</returns>
        public GameObject Borrow(Transform parent)
        {
            var obj = Borrow();
            obj.transform.SetParent(parent, true);
            return obj;
        }
        
        /// <summary>
        /// Override for the OnBorrow of the base class
        /// </summary>
        /// <param name="gameObject">Borrowed GameObject</param>
        protected override void OnBorrow(GameObject gameObject)
        {
            var pooledComponents = gameObject.GetComponents<IPooledComponent>();
            foreach (var pooledComponent in pooledComponents)
            {
                pooledComponent.OnBorrow();
            }
        }
        
        /// <summary>
        /// Override for the CreateNew of the base class. Creates a new GameObject instance from the prefab of the pool
        /// </summary>
        /// <returns>Created instance GameObject</returns>
        protected override GameObject CreateNew()
		{
			GameObject obj = Object.Instantiate(_prefab, _parent, false);
			var poolPrefab = GetOrAddPoolPrefabComponent(obj);
			poolPrefab.SourcePrefab = _prefab;
			return obj;
		}


		/// <summary>
		/// Override for the OnRevert of the base class. Resets the parent of the returned instance to the
		/// pool container transform
		/// </summary>
		/// <param name="obj">Returned instance GameObject</param>
        protected override void OnRevert(GameObject obj)
        {
            base.OnRevert(obj);
            obj.transform.SetParent(_parent, true);
        }

		/// <summary>
		/// Override for the DestroyExisting of the base class. Performs Object.Destroy on the GameObject
		/// </summary>
		/// <param name="instance"></param>
        protected override void DestroyExisting(GameObject instance)
        {
            Object.Destroy(instance);
        }
		
        
		private PoolPrefab GetOrAddPoolPrefabComponent(GameObject obj)
		{
			PoolPrefab o = obj.GetComponent<PoolPrefab>();
			if (o == null)
			{
				o = obj.AddComponent<PoolPrefab>();
			}
			return o;
		}
    }
}