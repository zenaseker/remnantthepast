using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 缓存池
/// </summary>
public class PoolManage
{
    private static PoolManage __instance;
    public static PoolManage Instance
    { 
        get
        {
            if (__instance == null)
            {
                __instance = new PoolManage();
            }
            return __instance; 
        }
    }
    [MenuItem("Jobs/清空缓存池")]
    public static void ClearAll()
    {
        __instance?.poollist?.Clear();
        __instance?._pools?.Clear();
        __instance?.spritepool?.Clear();
    }
    #region 实例池
    private Dictionary<string, Queue<GameObject>> poollist = new Dictionary<string, Queue<GameObject>>();
    /// <summary>
    /// 获取缓存实例
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="poolName">名称</param>
    /// <param name="parent">父对象</param>
    /// <returns>没有则返回新实例</returns>
    public GameObject GetPoolGameObject(string type, string poolName,Transform parent = null)
    {
        GameObject pool;
    Start:
        if (poollist.ContainsKey(poolName) && poollist[poolName].Count > 0)
        {
            pool = poollist[poolName].Dequeue();
            if (pool == null)
            {
                goto Start;
            }
            pool.transform.SetParent(parent);
        }
        else
        {
            pool = GameObject.Instantiate(Resources.Load<GameObject>($"Prefab/{type}/{poolName}"),parent);
        }
        pool.SetActive(true);
        pool.name = poolName;
        return pool;
    }

    /// <summary>
    /// 获取缓存实例
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="position">世界坐标</param>
    /// <param name="rotation">选择</param>
    /// <param name="poolName">名称</param>
    /// <param name="parent">父对象</param>
    /// <returns>没有则返回新实例</returns>
    public GameObject GetPoolGameObject(string type, string poolName,Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject pool;
    Start:
        if (poollist.ContainsKey(poolName) && poollist[poolName].Count > 0)
        {
            pool = poollist[poolName].Dequeue();
            if (pool == null)
            {
                goto Start;
            }
            pool.transform.SetParent(parent);
            pool.transform.position = position;
        }
        else
        {
            pool = GameObject.Instantiate(Resources.Load<GameObject>($"Prefab/{type}/{poolName}"), position, rotation, parent);
        }
        pool.SetActive(true);
        pool.name = poolName;
        return pool;
    }
    /// <summary>
    /// 缓存至缓存池
    /// </summary>
    /// <param name="obj">实例</param>
    /// <param name="nullparent">脱离父对象</param>
    public void PushGameObject(GameObject obj,bool nullparent = false)
    {
        if (nullparent) { obj.transform.SetParent(null); }
        obj.SetActive(false);
        string poolName = obj.name;
        if (!poollist.ContainsKey(poolName))
        {
            poollist.Add(poolName, new Queue<GameObject>());
        }
        if (!poollist[poolName].Contains(obj))
        {
            poollist[poolName].Enqueue(obj);
        }
    }
    public void ClearGameObjectPool()
    {
        foreach (var pool in poollist.Values)
        {
            pool.Clear();
        }
        poollist.Clear();
    }
    #endregion
    #region 数据类池

    private readonly Dictionary<Type, object> _pools = new Dictionary<Type, object>();

    /// <summary>
    /// 获取一个指定类型的对象
    /// </summary>
    public T GetClass<T>() where T : class, ICacheable, new()
    {
        Type type = typeof(T);
        if (!_pools.TryGetValue(type, out object poolObj))
        {
            var newPool = new Stack<T>();
            _pools[type] = newPool;
            poolObj = newPool;
        }
        var stack = (Stack<T>)poolObj;
        return stack.Count > 0 ? stack.Pop() : new T();
    }

    /// <summary>
    /// 归还对象，自动调用 Reset 重置状态
    /// </summary>
    public void Return<T>(T obj) where T : class, ICacheable
    {
        obj.Reset();
        Type type = typeof(T);
        if (!_pools.TryGetValue(type, out object poolObj))
        {
            var newPool = new Stack<T>();
            _pools[type] = newPool;
            poolObj = newPool;
        }
        ((Stack<T>)poolObj).Push(obj);
    }

    #endregion
    #region 图片暂存处
    public Dictionary<string,Sprite> spritepool = new Dictionary<string,Sprite>();
    /// <summary>
    /// 获取图片
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Sprite GetSprite(string name)
    {
        Sprite sprite;
        if (spritepool.TryGetValue(name, out sprite))
        {
            return sprite;
        }
        else
        {
            sprite = Resources.Load<Sprite>("icon/"+name);
            spritepool.Add(name, sprite);
            return sprite;
        }
    }
    #endregion
}
/// <summary>
/// 可进入数据缓存池的类
/// </summary>
public interface ICacheable
{
    /// <summary>
    /// 回收时调用，重置所有数据
    /// </summary>
    void Reset();
}