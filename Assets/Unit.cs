using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(NetworkTransform))]
public class Unit : NetworkBehaviour
{
    public DataHolder data;
    bool selected=false;
    ShooterScript shooterScript;
    GameObject selectCircle;
    GameObject targetLine;
    [SyncVar]
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
        if(TryGetComponent<ShooterScript>(out ShooterScript _shooterScript))
        {
            shooterScript = _shooterScript;
        }
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
            target = new Vector3(69,69,69);
            Health = MaxHealth;
        }
        data.ActiveUnits.Add(gameObject);
        makeHealthBar();
    }
    public void makeHealthBar()
    {
        GameObject tempthing = Instantiate(data.MiscPrefabs["HealthBar"]);
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
        Debug.Log("GETTING DISABLED " + gameObject.name);
        if (isServer)
        {
            data.ActiveUnits.Remove(gameObject);
        }
        data.ActiveUnits.Remove(gameObject);
        if (targetLine != null) { Destroy(targetLine); }
        if (healthBarScript != null) { Destroy(healthBarScript.gameObject); }
    }
    private void OnDestroy()
    {
        Debug.Log("GETTING DESTROYED " + gameObject.name);
        if (isServer)
        {
            data.ActiveUnits.Remove(gameObject);
        }
        data.ActiveUnits.Remove(gameObject);
        if (targetLine != null) { Destroy(targetLine); }
        if (healthBarScript != null) { Destroy(healthBarScript.gameObject); }
    }
    public void UpdateMesh()
    {
        if (ParentUnit != null)
        {
            GetComponent<MeshFilter>().mesh = ParentUnit.GetComponent<MeshFilter>().mesh;
            GetComponent<MeshCollider>().sharedMesh = ParentUnit.GetComponent<MeshCollider>().sharedMesh;
            GetComponent<MeshRenderer>().materials = ParentUnit.GetComponent<MeshRenderer>().materials;
        }
    }
    private void OnHealthChanged(float oldValue, float newValue)
    {
        if (data.Commander != null)
        {
            if (data.Commander.Team == Team)
            {
                if (Health <= 0)
                {
                    if (selected)
                    {
                        Debug.Log("REMOVED FROM SELECTED");
                        data.Commander.SelectedInGui(gameObject, false);
                        data.Commander.selected.Remove(gameObject);
                    }
                    //OnDisable();
                    die();


                }
            }
            else
            {
                Debug.Log("COMMANDER team " + data.Commander.Team + " my team " + Team);
            }
        }
        else
        {
            Debug.Log("COMMANDER NULL");
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
    [Command]
    public void die()
    {
        Destroy(gameObject);
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
            Debug.Log("I AM NOW " + selected + "ed");
            return selected;
        }
        else
        {
            return false;
        }
    }
    [Command]
    public void setTarget(Vector3 position, int team)
    {
        if (team == Team)
        {
            target = position+new Vector3(Random.Range(-0.5f, 0.5f),0, Random.Range(-0.5f, 0.5f));
        }
    }
    private bool hasShooterTarget()
    {
        if (shooterScript != null)
        {
            return shooterScript.Target != null;
        }
        else
        {
            return false;
        }
    }
    private void FixedUpdate()
    {

        if ((target - transform.position).magnitude >= (1f / 60f)&&target!=new Vector3(69, 69, 69))
        {
            if (isServer)
            {
                if (!hasShooterTarget())
                {
                    transform.LookAt(target);
                }
                //MoveTo(transform.position + (((target - transform.position).normalized) / 60f));
                //Debug.Log("Moved");
                MoveTo(transform.position + (((target - transform.position).normalized) / 60f));
                //transform.position += (((target - transform.position).normalized) / 60f);
                if ((target - transform.position).magnitude < 1f / 60f)
                {
                    MoveTo(target);
                    //MoveTo(target);
                }
            }
            if (Team == data.Commander.Team)
            {
                if (targetLine == null) targetLine = Instantiate(data.MiscPrefabs["TargetLine"]);
                targetLine.transform.position = new Vector3((target.x + transform.position.x) / 2, (target.y + transform.position.y) / 2, (target.z + transform.position.z) / 2);
                targetLine.transform.localScale = new Vector3(0.1f, 0.1f, Vector3.Distance(transform.position, target));
                targetLine.transform.LookAt(target);
                if ((target - transform.position).magnitude < 1f / 60f)
                {
                    Destroy(targetLine);
                    targetLine = null;
                }
            }

        }

    }
    //[Command]
    private void MoveTo(Vector3 pos)
    {
        transform.position = pos;
    }

}
