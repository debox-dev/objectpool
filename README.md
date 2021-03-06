# GameObjectPool
Unity game object pool

## Installation instructions 
### Quick Installation 
Put this in your `Packages/manigest.json` file 
``` 
"com.debox.objectpool": "https://github.com/debox-dev/objectpool.git",
```

## Example usage
```
using UnityEngine;
using Smackware.ObjectPool;

public class Test : MonoBehaviour
{

  public GameObject Prefab1;
  public MyScript Prefab2;

  private GameObject instance1;
  private MyScript instance2;

  private void Start()
  {
    // Borrow a simple game object. The pool will either grant us an existing instance it has, or create a new instance
    instance1 = GameObjectPoolManager.Borrow(Prefab1);
    instance1.transform.position = Vector3.zero;
    
    // Borrow a gameobject holding a specific component. Set this transform as its new parent immediately
    instance2 = GameObjectPoolManager.Borrow<MyScript>(Prefab2, Transform);
    instance2.transform.position = Vector3.one;
    
    // Return the instances to the pool in 5 seconds.
    Invoke("ReturnObjectsToPool", 5);
  }
  
  private void ReturnObjectsToPool()
  {
    // This will also deactivate both gameobjects and place and set the pool manager object (hidden) as their parent
    GameObjectPoolManager.Revert(instance1);
    GameObjectPoolManager.Revert(instance2);
  }
}
```
