
using UnityEngine;

/// <summary>
/// æ≤Ã¨÷ß≥÷
/// </summary>
/// <typeparam name="T">¿‡</typeparam>
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            return instance;
        }
    }
    void Awake()
    {
        if (instance == null) instance = (T)this;
        OnAwake();
    }
    protected virtual void OnAwake()
    {

    }
}
