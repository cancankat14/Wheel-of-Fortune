using UnityEngine;

namespace Vertigo.Wheel
{
    [DefaultExecutionOrder(-9000)]
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        [SerializeField] bool dontDestroyOnLoad = false;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this as T;
            if (dontDestroyOnLoad && gameObject.scene.IsValid())
                DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}