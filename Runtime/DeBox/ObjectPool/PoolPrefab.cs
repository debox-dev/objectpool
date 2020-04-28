using UnityEngine;

namespace DeBox.ObjectPool
{
    /// <summary>
    /// This class is auto-attached to borrowed instances,
    /// it contains data relevant to the origin of this borrowed instance 
    /// </summary>
    public class PoolPrefab : MonoBehaviour
    {
        /// <summary>
        /// The source prefab that this instance was generated from
        /// </summary>
        public GameObject SourcePrefab;
    }
}