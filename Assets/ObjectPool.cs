using System.Collections.Generic;
using UnityEngine;

// 对象池代码
// 子弹、敌机、道具都从对象池中调用，用完就继续放入池中
// 不用反复 Instantiate / Destroy
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    // key 用预制体名字（"Bullet" / "Enemy"），不用处理后缀
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, int> maxSizes = new Dictionary<string, int>();
    private Transform poolRoot; // 复用对象都挂在这里

    void Awake()
    {
        Instance = this;

        // 池对象统一收进一个子节点，防止 Hierarchy 根上铺一片
        GameObject root = new GameObject("PooledObjects");
        root.transform.SetParent(transform, false);
        poolRoot = root.transform;
    }

    // 提前预热一些，否则会时不时卡顿
    // prewarm 是预热几个，maxSize 是池子最多存几个，0 表示不限
    public void CreatePool(GameObject prefab, int prewarm = 0, int maxSize = 0)
    {
        if (prefab == null) return;
        string key = prefab.name;

        // 同一个 prefab 调两次只更新上限，不再预热一遍
        // 否则池里会多出一倍的空对象
        if (poolDictionary.ContainsKey(key))
        {
            if (maxSize > 0) maxSizes[key] = maxSize;
            return;
        }

        poolDictionary[key] = new Queue<GameObject>();
        maxSizes[key] = maxSize;

        for (int i = 0; i < prewarm; i++)
        {
            GameObject go = Instantiate(prefab, poolRoot);
            go.name = key;
            go.SetActive(false);
            poolDictionary[key].Enqueue(go);
        }
    }

    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        string key = prefab.name;

        // 没建过池就进行新建
        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary[key] = new Queue<GameObject>();
            maxSizes[key] = 0;
        }

        GameObject objToSpawn;

        if (poolDictionary[key].Count > 0)
        {
            // 池里有就直接复用，先摆好位置再激活
            objToSpawn = poolDictionary[key].Dequeue();
            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;
            objToSpawn.SetActive(true);
        }
        else
        {
            // 池子空了才去创建
            objToSpawn = Instantiate(prefab, position, rotation, poolRoot);
            objToSpawn.name = key; // 去掉 (Clone)，好统一管理
        }

        return objToSpawn;
    }

    public void ReturnObject(GameObject obj)
    {
        if (obj == null) return;

        // 已经回收过就不再入队
        if (!obj.activeSelf) return;

        string key = obj.name.Replace("(Clone)", "").Trim();
        obj.SetActive(false);

        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary[key] = new Queue<GameObject>();
        }

        // 超过上限就直接销毁
        int max = maxSizes.ContainsKey(key) ? maxSizes[key] : 0;
        if (max > 0 && poolDictionary[key].Count >= max)
        {
            Destroy(obj);
        }
        else
        {
            poolDictionary[key].Enqueue(obj);
        }
    }
}
