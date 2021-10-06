using Mirror;
using System.Collections;
using System.Collections.Generic;
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

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
