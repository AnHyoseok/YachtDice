using UnityEngine;

namespace Singleton
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instace;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] {typeof(T)} 인스턴스가 이미 종료됨.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instace == null)
                    {
                        _instace = FindAnyObjectByType<T>();

                        if (_instace == null)
                        {
                            GameObject singletonobject = new GameObject(typeof(T).Name);
                            _instace = singletonobject.AddComponent<T>();
                        }
                    }
                    return _instace;
                }
            }
        }
        protected virtual void Awake()
        {
            if (_instace == null)
            {
                _instace = this as T;
                //DontDestroyOnLoad(gameObject);
            }
            /*else if (_instace != null)
            {
                Destroy(gameObject);
            }*/
        }
        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}