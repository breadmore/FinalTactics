using Unity.Netcode;
using UnityEngine;

public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[NetworkSingleton] Duplicate instance of {typeof(T).Name} detected. Destroying...");
            Destroy(gameObject);
            return;
        }

        _instance = (T)this;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
