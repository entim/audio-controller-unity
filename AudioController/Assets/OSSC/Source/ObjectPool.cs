using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a pool of different prefabs when someone requests a GameObject.
/// </summary>
public class ObjectPool : MonoBehaviour {
    #region Public fields
    /// <summary>
    /// The list of Prefab based pools
    /// </summary>
    public List<PrefabBasedPool> pools;
    #endregion
    #region Public methods and properties

    /// <summary>
    /// Gets a Free GameObject.
    /// </summary>
    /// <param name="prefab">The kind of GameObject to return</param>
    /// <returns>Returns the requested GameObject instance</returns>
    public GameObject GetFreeObject(GameObject prefab = null) {
        if (prefab == null)
            return null;

        PrefabBasedPool pool = pools.Find((x) => {
            return x.prefab == prefab;
        });

        if (pool != null)
            return pool.GetFreeObject();

        pool = new PrefabBasedPool(prefab);
        GameObject parent = new GameObject();
        parent.name = pool.prefab.name + " ::: POOL";
        parent.transform.parent = this.gameObject.transform;
        pool.parent = parent.transform;
        pools.Add(pool);
        return pool.GetFreeObject();
    }

    public void Deactivate() {
        foreach (PrefabBasedPool pool in pools) {
            pool.Active = false;
        }
    }

    #endregion

    #region Monobehaviour methods
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake() {
        pools = new List<PrefabBasedPool>();
    }

    // Prevent despawning during destroy
    void OnApplicationQuit() {
        Deactivate();
    }
    #endregion
}

[System.Serializable]
public class PrefabBasedPool {

    public GameObject prefab;
    public Stack<GameObject> pool;

    /// <summary>
    /// Where pooled objects will reside.
    /// </summary>
    public Transform parent;

    private bool _active = true;

    public PrefabBasedPool(GameObject prefab) {
        pool = new Stack<GameObject>();
        this.prefab = prefab;
    }

    public GameObject GetFreeObject() {
        GameObject freeObj = null;

        lock(pool) {
            if (pool.Count > 0) {
                freeObj = pool.Pop();
            }
        }

        if (freeObj != null) {
            freeObj.SetActive(true);
        } else {
            freeObj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
            freeObj.SetActive(true);
            var objPoolable = freeObj.GetComponent<IPoolable>();
            objPoolable.Pool = this;
        }
        return freeObj;
    }

    public void Despawn(GameObject obj) {
        if (_active) {
            obj.transform.SetParent(parent, false);
            obj.transform.position = Vector3.zero;
            obj.SetActive(false);
            pool.Push(obj);
        }
    }

    public bool IsActive(GameObject obj) {
        return !pool.Contains(obj);
    }

    public bool Active {
        get { return _active; }
        set { _active = value; }
    }
}
