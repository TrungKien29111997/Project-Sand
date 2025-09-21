using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;

    public static bool instanceExists
    {
        get
        {
            return _instance != null;
        }
    }

    public static T Instance
    {
        get
        {
            InitInstance();

            return _instance;
        }
    }

    public static void InitInstance()
    {
        if (_instance != null) return;

        // if null, finds an existing one
        if (_instance == null) _instance = (T)FindObjectOfType(typeof(T));

        // attempt to laod from resources
        if (_instance == null)
        {
            GameObject go = Resources.Load<GameObject>(typeof(T).Name);
            if (go != null)
            {
                _instance = Instantiate(go).GetComponent<T>();
                _instance.name = "(singleton)" + typeof(T).ToString();
            }
        }

        // if still null, instantiates
        if (_instance == null)
        {
            Debug.Log(">> Instantiating Singleton: " + typeof(T).Name + "\n" + System.Environment.StackTrace);
            GameObject singleton = new GameObject();
            _instance = singleton.AddComponent<T>();
            singleton.name = "(singleton)" + typeof(T).ToString();
        }

        // if not null but inactive, finds an active one
        else if (!_instance.gameObject.activeInHierarchy)
        {
            Object[] allObjsOfType = FindObjectsOfType(typeof(T));
            if (allObjsOfType.Length > 1)
            {
                GameObject[] a = (GameObject[])allObjsOfType;
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i].activeInHierarchy)
                    {
                        _instance = a[i].GetComponent<T>();
                        break;
                    }
                }
            }
        }
    }
}

