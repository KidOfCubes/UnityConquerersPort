using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Building : NetworkBehaviour
{
    public DataHolder data;
    bool selected = false;


    [SyncVar]
    public int Team;
    [SyncVar(hook = nameof(OnHealthChanged))]
    public float Health;
    public float MaxHealth;
    public int BuildingIndex;
    public float Cost;

    public float BuildEnemyUnitRange;
    public float BuildEnemyBuildingRange;
    public float BuildUnitRange;
    public float BuildBuildingRange;
    public float BuildCrystalRange;

    public Building ParentBuilding;
    public HealthBarScript healthBarScript;
    GameObject selectCircle;
    // Start is called before the first frame update
    public bool canBuild()
    {
        bool FriendlyRequirements = false;
        bool EnemyRequirements = true;
        bool CrystalRequirement = false;
        foreach (GameObject unit in data.ActiveUnits)
        {
            if (unit.GetComponent<Unit>().Team == Team)
            {
                if ((unit.transform.position - transform.position).magnitude <= BuildUnitRange && BuildUnitRange != 0)
                {
                    FriendlyRequirements = true;
                }
            }
            else
            {
                if ((unit.transform.position - transform.position).magnitude < BuildEnemyUnitRange && BuildEnemyUnitRange != 0)
                {
                    EnemyRequirements = false;
                }
            }
        }
        foreach (GameObject building in data.ActiveBuildings)
        {
            if (building.GetComponent<Building>().Team == Team)
            {
                if ((building.transform.position - transform.position).magnitude <= BuildBuildingRange && BuildBuildingRange!=0)
                {
                    FriendlyRequirements = true;
                    //Debug.Log("CLOSE ENOUGH");
                }
            }
            else
            {
                if ((building.transform.position - transform.position).magnitude < BuildEnemyBuildingRange && BuildEnemyBuildingRange != 0)
                {
                    EnemyRequirements = false;
                }
            }
        }
        foreach(GameObject crystal in data.EnergyCrystals)
        {
            if ((crystal.transform.position - transform.position).magnitude <= BuildCrystalRange && BuildCrystalRange != 0 && !crystal.GetComponent<EnergyCrystal>().Taken)
            {
                CrystalRequirement = true;
            }
        }
        if (BuildUnitRange == 0 && BuildBuildingRange == 0)
        {
            FriendlyRequirements = true;
        }
        if (BuildCrystalRange <= 0)
        {
            //Debug.Log("crystal req");
            CrystalRequirement = true;
        }
        //Debug.Log("FriendlyRequirements " + FriendlyRequirements + " EnemyRequirements " + EnemyRequirements + " CrystalRequirement " + CrystalRequirement + " Cost " + (data.money >= Cost));
        return FriendlyRequirements && EnemyRequirements && CrystalRequirement && data.money>=Cost;
    }
    public bool TakeCrystal()
    {
        foreach (GameObject crystal in data.EnergyCrystals)
        {
            if ((crystal.transform.position - transform.position).magnitude <= BuildCrystalRange&&!crystal.GetComponent<EnergyCrystal>().Taken)
            {
                crystal.GetComponent<EnergyCrystal>().Taken = true;
                return true;
            }
        }
        return false;
    }

    public void UpdateMesh()
    {
        if (ParentBuilding != null)
        {
            GetComponent<MeshFilter>().mesh = ParentBuilding.GetComponent<MeshFilter>().mesh;
            GetComponent<MeshCollider>().sharedMesh = ParentBuilding.GetComponent<MeshCollider>().sharedMesh;
            GetComponent<MeshRenderer>().materials = ParentBuilding.GetComponent<MeshRenderer>().materials;
        }
    }
    void Start()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<MeshCollider>().convex = true;
        Vector3 temppos = transform.position;
        transform.position = Vector3.zero;
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        Material[] materials = new Material[meshFilters.Length - 1];
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];
        int j = 0;
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.gameObject != gameObject)
            {
                combine[j].mesh = meshFilter.sharedMesh;
                combine[j].transform = meshFilter.transform.localToWorldMatrix;
                meshFilter.gameObject.SetActive(false);
                if (meshFilter.GetComponent<MeshRenderer>().material.name != "TeamMat (Instance)")
                {
                    materials[j] = meshFilter.GetComponent<MeshRenderer>().material;
                }
                else
                {
                    Material temp = meshFilter.GetComponent<MeshRenderer>().material;
                    temp.color = data.TeamColors[Team];
                    materials[j] = temp;
                }
                j++;
            }
        }
        GetComponent<MeshFilter>().mesh = new Mesh();
        GetComponent<MeshFilter>().mesh.CombineMeshes(combine, false, true);
        GetComponent<MeshCollider>().sharedMesh = new Mesh();
        GetComponent<MeshCollider>().sharedMesh.CombineMeshes(combine, false, true);
        GetComponent<MeshRenderer>().materials = materials;
        transform.position = temppos;

        if (isServer)
        {
            Health = MaxHealth;
        }
        data.ActiveBuildings.Add(gameObject);
        GameObject tempthing = Instantiate(data.HealthBarPrefab);
        healthBarScript = tempthing.GetComponent<HealthBarScript>();
        healthBarScript.MaxHealth = MaxHealth;
        healthBarScript.Health = Health;
        healthBarScript.Height = 0.05f;
        healthBarScript.Width = 0.5f;
        healthBarScript.UpdateValues();
        healthBarScript.transform.SetParent(transform, false);
        healthBarScript.transform.localPosition = new Vector3(0, 0.5f, 0);
    }
    private void OnDisable()
    {
        if (isServer)
        {
            data.ActiveBuildings.Remove(gameObject);
        }
        data.ActiveBuildings.Remove(gameObject);
        if (healthBarScript != null) { Destroy(healthBarScript.gameObject); }
    }
    private void OnDestroy()
    {
        if (isServer)
        {
            data.ActiveBuildings.Remove(gameObject);
        }
        data.ActiveBuildings.Remove(gameObject);
        if (healthBarScript != null) { Destroy(healthBarScript.gameObject); }
    }
    public bool select(int team)
    {
        if (team == Team)
        {
            selected = !selected;
            if (selected)
            {
                selectCircle = Instantiate(data.SelectOutlines[0]);
                selectCircle.transform.parent = transform;
                selectCircle.transform.localPosition = Vector3.zero;
            }
            else
            {
                Destroy(selectCircle);
            }
            return selected;
        }
        else
        {
            return false;
        }
    }
    private void OnHealthChanged(float oldValue, float newValue)
    {
        if (Health <= 0)
        {
            OnDestroy();
            OnDisable();
            if (selected)
            {
                data.Commander.ClickOnObject(0, gameObject);
            }
            data.ActiveBuildings.Remove(gameObject);
            Destroy(healthBarScript.gameObject);
            Destroy(gameObject);
        }
        if (healthBarScript != null)
        {
            Debug.Log("helt " + Health);
            healthBarScript.Health = Health;
            healthBarScript.UpdateValues();
        }
        else
        {
            Debug.Log("oh no its null");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
