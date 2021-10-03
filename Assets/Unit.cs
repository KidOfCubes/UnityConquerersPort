using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Unit : NetworkBehaviour
{
    public DataHolder data;
    bool selected=false;
    GameObject selectCircle;
    GameObject targetLine;
    public Vector3 target;
    [SyncVar]
    public int Team;
    [SyncVar(hook = nameof(OnHealthChanged))]
    public float Health;
    public float Cost;
    public float MaxHealth;
    public int UnitIndex;
    public Unit ParentUnit;
    public HealthBarScript healthBarScript;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("START");
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
                foreach (Behaviour component in meshFilter.GetComponents<Behaviour>())
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
            data.ActiveUnits.Add(gameObject);
        }
        GameObject tempthing = Instantiate(data.HealthBarPrefab);
        healthBarScript = tempthing.GetComponent<HealthBarScript>();
        healthBarScript.MaxHealth = MaxHealth;
        healthBarScript.Health = Health;
        healthBarScript.Height = 0.05f;
        healthBarScript.Width = 0.5f;
        healthBarScript.UpdateValues();
        healthBarScript.transform.SetParent(transform, false);
        healthBarScript.transform.localPosition = new Vector3(0, 0.3f, 0);
    }
    private void OnDisable()
    {
        if (isServer)
        {
            data.ActiveUnits.Remove(gameObject);
        }
    }
    private void OnDestroy()
    {
        if (isServer)
        {
            data.ActiveUnits.Remove(gameObject);
        }
    }
    private void OnHealthChanged(float oldValue, float newValue)
    {
        if (Health <= 0)
        {
            OnDisable();
            data.Commander.ClickOnObject(0, gameObject);
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
    public bool select(int team)
    {
        if(team == Team)
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
    public void setTarget(Vector3 position, int team)
    {
        if (team == Team)
        {
            target = position+new Vector3(Random.Range(-0.5f, 0.5f),0, Random.Range(-0.5f, 0.5f));
            if (targetLine == null) targetLine = Instantiate(data.TargetLine);

            targetLine.transform.parent = transform;
            targetLine.transform.position = new Vector3((target.x + transform.position.x) / 2, (target.y + transform.position.y) / 2, (target.z + transform.position.z) / 2);
            targetLine.transform.localScale = new Vector3(0.1f, 0.1f, Vector3.Distance(transform.position, target));
            targetLine.transform.LookAt(target);
        }
    }
    private void FixedUpdate()
    {
        if (targetLine != null)
        {
            targetLine.transform.position = new Vector3((target.x + transform.position.x) / 2, (target.y + transform.position.y) / 2, (target.z + transform.position.z) / 2);
            targetLine.transform.localScale = new Vector3(0.1f, 0.1f, Vector3.Distance(transform.position, target));
            targetLine.transform.LookAt(target);
            //MoveTo(transform.position + (((target - transform.position).normalized) / 60f));
            transform.position += ((target - transform.position).normalized) / 60f;
            if ((target - transform.position).magnitude < 1f / 60f)
            {
                transform.position = target;
                //MoveTo(target);
                Destroy(targetLine);
                targetLine = null;
            }
        }
    }
    [Command]
    private void MoveTo(Vector3 pos)
    {
        transform.position = pos;
    }

}
