using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCrystal : MonoBehaviour
{
    public DataHolder data;
    public bool Taken = false;
    // Start is called before the first frame update
    void Start()
    {
        data.EnergyCrystals.Add(gameObject);
    }

    // Update is called once per frame
    void OnDisable()
    {

        data.EnergyCrystals.Remove(gameObject);
    }
}
