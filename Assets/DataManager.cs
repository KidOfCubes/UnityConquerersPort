using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public DataHolder data;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("MANAGED");
        foreach (GameObject prefab in data.Units)
        {
            Debug.Log("REGISTERED " + prefab.name);
            NetworkClient.RegisterPrefab(prefab);
        }
        foreach (GameObject prefab in data.Buildings)
        {
            Debug.Log("REGISTERED " + prefab.name);
            NetworkClient.RegisterPrefab(prefab);
        }
        string[] tempMiscPrefabKeys = new string[data.MiscPrefabGameObjects.Length];
        for (int i = 0; i < tempMiscPrefabKeys.Length; ++i)
            tempMiscPrefabKeys[i] = data.MiscPrefabGameObjects[i].name;

        data.MiscPrefabs = tempMiscPrefabKeys.Zip(data.MiscPrefabGameObjects, (k, v) => new { k, v })
              .ToDictionary(x => x.k, x => x.v);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
