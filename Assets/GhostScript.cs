using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostScript : MonoBehaviour
{
    public DataHolder data;
    public Building building;
    public bool canPlace = true;
    List<Collider> currentColliding = new List<Collider>();
    private void Start()
    {
        building = GetComponent<Building>();
        GetComponent<Rigidbody>().isKinematic = true;
        foreach (Material mat in GetComponent<MeshRenderer>().materials)
        {
            MaterialTools.ToTransparentMode(mat);
        }
        UpdateMaterial();
    }
    private void FixedUpdate()
    {
        UpdateMaterial();
    }
    private void UpdateMaterial()
    {
        //Debug.Log("COST IS " + building.Cost + " AND I HAVE " + data.money + "$ and i am colliding with " + currentColliding.Count);
        if (currentColliding.Count == 0&& building.canBuild())
        {
            canPlace = true;
            foreach (Material mat in GetComponent<MeshRenderer>().materials)
            {
                mat.color = new Color(0, 255, 0, 0.5f);
            }
        }
        
        if (currentColliding.Count > 0 ||!building.canBuild())
        {
            canPlace = false;
            foreach (Material mat in GetComponent<MeshRenderer>().materials)
            {
                mat.color = new Color(255, 0, 0, 0.5f);
            }
        }
    }
    public bool CanPlace()
    {
        return canPlace && building.canBuild();
    }
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("TRIGGERENTER as " + gameObject.name);
        if (!currentColliding.Contains(other))
        {
            currentColliding.Add(other);
        }
        UpdateMaterial();
    }
    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("TRIGGEREXIT as " + gameObject.name);
        if (currentColliding.Contains(other))
        {
            currentColliding.Remove(other);
        }
        UpdateMaterial();
    }
}
