using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "DataHolder", order = 1)]
public class DataHolder : ScriptableObject
{
    public GameObject[] Buildings;
    public GameObject[] Units;
    public GameObject[] SelectOutlines;
    public List<GameObject> EnergyCrystals = new List<GameObject>();
    public List<GameObject> ActiveUnits = new List<GameObject>();
    public List<GameObject> ActiveBuildings = new List<GameObject>();
    public Dictionary<int,GameObject> ActiveCommanders = new Dictionary<int, GameObject>();
    public Color[] TeamColors;
    public GameObject TargetLine;
    public GameObject HealthBarPrefab;
    public GameObject[] ProducerPrefabs;

    public CommanderScript Commander;

    public float money;
}