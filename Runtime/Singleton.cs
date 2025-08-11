using System;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace SPMLib
{
    public class Singleton<T> where T : class, new()
    {
        private static readonly Lazy<T> _instance = new Lazy<T>(() =>
        {
            var instance = new T();
            return instance;
        });

        public static T Instance => _instance.Value;

        private Singleton() { }
    }

    public class SingletonWithMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool applicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] '{typeof(T)}' �ν��Ͻ��� ���ø����̼� ���� �� �̹� �ı��Ǿ����ϴ�. null�� ��ȯ�մϴ�.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindInstanceInScene();

                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject($"{typeof(T)} (Singleton)");
                            _instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                        }
                        else
                        {
                            DontDestroyOnLoad(_instance.gameObject);
                            Debug.Log($"[Singleton] ���� �ν��Ͻ��� ����մϴ�: {_instance.gameObject.name}");
                        }
                    }

                    return _instance;
                }
            }
        }

        private static T FindInstanceInScene()
        {
            foreach (var rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var instance = rootObj.GetComponentInChildren<T>();
                if (instance != null)
                    return instance;
            }

            return null;
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] �ߺ��� �ν��Ͻ��� �߰ߵǾ����ϴ�. ���� �ν��Ͻ��� �����ϰ� �� ������Ʈ�� �����մϴ�: {gameObject.name}");
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public static bool IsInited() => _instance != null;
        public static bool IsApplicationQuitting() => applicationIsQuitting;
    }

    public class SingletonWithScene<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        protected virtual void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
                return;
            }
            Instance = this as T;
        }
    }

    public class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load($"Assets/{typeof(T).Name}") as T;
                    if (_instance == null)
                    {
                        _instance = CreateInstance<T>();
                    }
                }

                return _instance;
            }
        }
    }
}