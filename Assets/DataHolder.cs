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
    public Dictionary<int, CommanderScript> ActiveCommanders = new Dictionary<int, CommanderScript>();
    public Color[] TeamColors;
    //public GameObject TargetLine;
    //public GameObject HealthBarPrefab;
    //public GameObject[] ProducerPrefabs;
    public GameObject[] MiscPrefabGameObjects;
    public Dictionary<string, GameObject> MiscPrefabs = new Dictionary<string, GameObject>();

    public CommanderScript Commander;

    public float money;
}