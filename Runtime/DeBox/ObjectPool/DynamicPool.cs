using System;
using System.Collections.Generic;
using System.Linq;

namespace DeBox.ObjectPool
{
    /// <summary>
    /// Thrown when a pool is at its maximum instance-count limit
    /// </summary>
    public class MaxPoolSizeException : Exception {}
    
    
    /// <summary>
    /// An object pool that can dynamically instantiate new members in the pool in case the pool runs out of instances
    /// This is an abstract class that helps implement such dynamic pools for type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DynamicPool<T> : IPool<T>
    {
        private readonly List<T> _pool = new List<T>();
        private readonly HashSet<T> _borrowed = new HashSet<T>();

        /// <summary>
        /// Amount of instances currently borrowed from the pool
        /// </summary>
        public int BorrowedCount => _borrowed.Count;

        /// <summary>
        /// Returns the total count of instances managed by this pool, borrowed and cached
        /// </summary>
        public int Size => _pool.Count + _borrowed.Count;

        /// <summary>
        /// Maximum size of the pool, can also be set at runtime
        ///
        /// If you set a lower count than what is currently borrowed, objects will not be returned and nothing will
        /// be harmed, but new objects could not be borrowed
        /// </summary>
        public int MaxSize { get; set; }
        
        /// <summary>
        /// Initialize the instance of the pool with the maximum size
        /// </summary>
        /// <param name="maxSize">Maximum pool size</param>
        public DynamicPool(int maxSize)
        {
            MaxSize = maxSize;
        }
        
        /// <summary>
        /// Override this to create a new instance of type T in-case the pool needs to dynamically grow and create
        /// new instances
        /// </summary>
        /// <returns></returns>
        protected abstract T CreateNew();

        /// <summary>
        /// Override this to destroy an existing instance that was returned, but is no longer needed because the pool
        /// is at over capacity (If for example at run-time someone reduced the pool size)
        /// </summary>
        /// <param name="instance"></param>
        protected abstract void DestroyExisting(T instance);

        /// <summary>
        /// Immediately revert and destroy all instances, borrowed or not
        /// </summary>
        public void DepopulateAll()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                DestroyExisting(_pool[i]);
            }
            var borrowed = new List<T>(_borrowed);
            for (int i = 0; i < borrowed.Count; i++)
            {
                UnityEngine.Debug.LogError("DepopulateAll called while not all objects were reverted!!!");
                DestroyExisting(borrowed[i]);
            }
            _pool.Clear();
        }

        /// <summary>
        /// Pre-cache the pool with a given amount of instances
        /// </summary>
        /// <param name="amount"></param>
        public void PreCache(int amount)
        {
            for (int i = 0; (i < amount) && (_pool.Count + _borrowed.Count < MaxSize); i++)
            {
                var obj = CreateNew();
                _pool.Add(obj);
            }
        }

        /// <summary>
        /// Borrow an instance from the pool
        /// </summary>
        /// <returns>Borrowed instance</returns>
        /// <exception cref="MaxPoolSizeException">If the pool is at max size and there are no cached objects</exception>
        public T Borrow()
        {
            T obj;
            // First, check if we have something in the pool
            if (_pool.Count > 0)
            {
                obj = _pool[0];
                _pool.RemoveAt(0);
                _borrowed.Add(obj);
                OnBorrow(obj);
                return obj;
            }
            // Otherwise we check if we can generate a new one
            if (Size >= MaxSize)
            {
                throw new MaxPoolSizeException();
            }
            // Generate a new object of the pool is empty
            obj = CreateNew();
            _borrowed.Add(obj);
            OnBorrow(obj);
            return obj;
        }

        public void RevertAll()
        {
            var allBorrowedInstances = _borrowed.ToArray();
            foreach (var borrowedInstance in allBorrowedInstances)
            {
                Revert(borrowedInstance);   
            }
        }
        
        /// <summary>
        /// Revert a borrowed instance
        /// </summary>
        /// <param name="obj">Borrowed instance</param>
        public void Revert(T obj)
        {
            
            // Verify its a borrowed object and remove it from the borrowed set
            var canReturnToPool = PreRevert(obj);
            _borrowed.Remove(obj);
            if (canReturnToPool)
            {
                _pool.Add(obj);
            }
            else
            {
                PreCache(1);
            }
        }

        /// <summary>
        /// Optionally override this to perform manipulations on the borrowed instance on borrow
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnBorrow(T obj)
        {
            
        }

        /// <summary>
        /// Optionally override this to perform manipulations on the returned instance
        ///
        /// For example: Reset values, free memory, etc..
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Whether the instance is uncorrupted and can be put back into the pool</returns>
        protected virtual bool PreRevert(T obj)
        {
            return true;
        }

    }
}