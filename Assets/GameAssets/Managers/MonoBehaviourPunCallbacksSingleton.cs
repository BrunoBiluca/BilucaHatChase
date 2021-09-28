using Photon.Pun;
using UnityEngine;

public abstract class MonoBehaviourPunCallbacksSingleton<T> 
    : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacksSingleton<T>
{

    private static MonoBehaviourPunCallbacksSingleton<T> instance;
    public static T Instance {
        get {
            return (T)instance;
        }
    }

    [SerializeField] private bool destroyOnLoad;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            if(!destroyOnLoad) DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        OnAwake();
    }

    protected virtual void OnAwake() { }
}
