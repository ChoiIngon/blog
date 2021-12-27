using UnityEngine;

namespace Gamnet.Util
{
    public class Singleton<T> where T : class, new()
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                }
                return _instance;
            }
        }
    }

    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;
        private static bool applicationQuit = false;
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    if (true == applicationQuit)
                    {
                        return null;
                    }
                    _instance = (T)GameObject.FindObjectOfType(typeof(T));
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        container.name = typeof(T).Name;
                        _instance = container.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        private void OnApplicationQuit()
        {
            applicationQuit = true;
        }
    }

}
