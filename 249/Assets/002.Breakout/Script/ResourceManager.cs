using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : Gamnet.Util.MonoSingleton<ResourceManager>
{
    [SerializeField]
    public GameObject[] prefabs;
    private Dictionary<string, GameObject> dictPrefabs = new Dictionary<string, GameObject>();

    public ResourceManager()
    {
        foreach (GameObject go in prefabs)
        {
            dictPrefabs.Add(go.name, go);
        }
    }

    public GameObject Instantiate(string name)
    {
        GameObject go = null;
        if (false == dictPrefabs.TryGetValue(name, out go))
        {
            return null;
        }

        return Instantiate<GameObject>(go);
    }
}
