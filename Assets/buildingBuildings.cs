using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class buildingBuildings : NetworkBehaviour
{
    bool placing = false;
    GameObject ghost;
    public DataHolder data;
    CommanderScript commanderScript;
    private int BuildingIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        commanderScript = GetComponent<CommanderScript>();
        if (!isLocalPlayer)
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleGhost();
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (placing)
            {
                if (ghost.GetComponent<GhostScript>().CanPlace())
                {
                    Build();
                }
            }
            
        }
        if (Input.mouseScrollDelta.y != 0)
        {
            if (placing)
            {
/*                Debug.Log("Y WAS1 " + Input.mouseScrollDelta.y);
                Debug.Log("Y WAS " + data.Buildings.Length);
                Debug.Log("Y WAS " + (BuildingIndex + (int)Input.mouseScrollDelta.y));*/
                BuildingIndex = (BuildingIndex + (int)Input.mouseScrollDelta.y);
                while (BuildingIndex >= data.Buildings.Length)
                {
                    BuildingIndex -= data.Buildings.Length;
                }
                while (BuildingIndex < 0)
                {
                    BuildingIndex += data.Buildings.Length;
                }
                ToggleGhost();
                ToggleGhost();
            }
        }
    }
    private void FixedUpdate()
    {
        if (placing)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject.layer == 3)
                {
                    ghost.transform.position = hit.point + (new Vector3(0, 0.01f, 0));
                }
                else
                {
                    ghost.transform.position = hit.point;
                }
            }
            
        }
    }
    void Build()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        { 
            RemoveMoney(ghost.GetComponent<Building>().Cost);
            SpawnBuilding(BuildingIndex,hit.point);
            placing = true;
            ToggleGhost();
        }
        
    }
    [Command]
    void SpawnBuilding(int index, Vector3 position)
    {
        GameObject thing = Instantiate(data.Buildings[index], position, Quaternion.identity);
        thing.GetComponent<Building>().BuildingIndex = BuildingIndex;
        thing.GetComponent<Building>().Team = commanderScript.Team;
        NetworkServer.Spawn(thing);
    }

    [Command]
    void RemoveMoney(float cost)
    {
        commanderScript.Money -= cost;
    }
    void ToggleGhost()
    {
        placing = !placing;
        if (placing)
        {
            ghost = Instantiate(data.Buildings[BuildingIndex]);
            foreach (Behaviour component in ghost.GetComponents<Behaviour>())
            {
                if (!(component.GetType()).IsSubclassOf(typeof(Renderer)))
                {
                    if (!((component.GetType()) == (typeof(MeshFilter)) || (component.GetType()) == (typeof(Transform)) || (component.GetType()) == (typeof(Rigidbody))))
                    {
                        component.enabled = false;
                        //Destroy(component);
                    }
                }
            }
            MeshFilter[] meshFilters = ghost.GetComponentsInChildren<MeshFilter>();
            Material[] materials = new Material[meshFilters.Length-1];
            CombineInstance[] combine = new CombineInstance[meshFilters.Length-1];
            int j = 0;
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.gameObject != ghost)
                {
                    combine[j].mesh = meshFilter.sharedMesh;
                    combine[j].transform = meshFilter.transform.localToWorldMatrix;
                    meshFilter.gameObject.SetActive(false);
                    materials[j] = meshFilter.GetComponent<MeshRenderer>().material;
                    j++;
                }
            }
            ghost.transform.GetComponent<MeshFilter>().mesh = new Mesh();
            ghost.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, false, true);
            ghost.transform.GetComponent<MeshCollider>().sharedMesh = new Mesh();
            ghost.transform.GetComponent<MeshCollider>().sharedMesh.CombineMeshes(combine, false, true);
            ghost.transform.GetComponent<MeshCollider>().isTrigger = true;
            ghost.transform.GetComponent<MeshRenderer>().materials = materials;
            ghost.transform.GetComponent<MeshCollider>().isTrigger = true;
            ghost.transform.GetComponent<Building>().Team = commanderScript.Team;
            ghost.transform.gameObject.SetActive(true);
            ghost.AddComponent<GhostScript>();
            ghost.GetComponent<GhostScript>().data = data;
            for (int i = 0; i < ghost.transform.childCount; i++)
            {
                //ghost.transform.GetChild(i).gameObject.SetActive(false);
                foreach(Component component in ghost.transform.GetChild(i).GetComponents<Component>())
                {
                    if (!(component.GetType()).IsSubclassOf(typeof(Renderer)))
                    {
                        if (!((component.GetType()) == (typeof(MeshFilter)) || (component.GetType()) == (typeof(Transform))))
                        {
                            Destroy(component);
                        }
                    }
                }
/*                if(ghost.transform.GetChild(i).gameObject.TryGetComponent<Renderer>(out Renderer renderer))
                {
                    Color color = renderer.material.color;
                    color.a = 0.5f;
                    renderer.material.color = color;
                    renderer.material.shader = Shader.Find("UI/Unlit/Transparent");

                }*/

            }
        }
        else
        {
            Destroy(ghost);
            //Destroy(ghost);
            ghost = null;
        }
    }
}
