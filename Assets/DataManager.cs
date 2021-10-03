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
        foreach (GameObject prefab in data.Units)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
        foreach (GameObject prefab in data.Buildings)
        {
            NetworkClient.RegisterPrefab(prefab);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
