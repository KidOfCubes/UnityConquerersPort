using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterScript : NetworkBehaviour
{
    public DataHolder data;
    public float Range;
    public float AttackInterval;
    public float Damage;
    private int Team;
    public GameObject Target;


    private float time = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        if (!isServer)
        {
            this.enabled = false;
        }
        else
        {
            if (TryGetComponent<Building>(out Building building)) Team = building.Team;
            if (TryGetComponent<Unit>(out Unit unit)) Team = unit.Team;
        }
    }

    // Update is called once per frame

    void Update()
    {
        time += Time.deltaTime;

        if (time >= AttackInterval)
        {
            time = time - AttackInterval;

            if (Target != null)
            {
                if ((Target.transform.position - transform.position).magnitude > Range)
                {
                    Target = null;
                }
            }
            if (Target == null)
            {
                foreach (GameObject thing in data.ActiveBuildings)
                {
                    if (thing.GetComponent<Building>().Team != Team)
                    {
                        if ((thing.transform.position - transform.position).magnitude <= Range)
                        {
                            Target = thing;
                            transform.LookAt(Target.transform);
                        }
                    }
                }
            }

            if (Target == null)
            {
                foreach (GameObject thing in data.ActiveUnits)
                {
                    if (thing.GetComponent<Unit>().Team != Team)
                    {
                        if ((thing.transform.position - transform.position).magnitude <= Range)
                        {
                            //Debug.Log("FOUND UNIT TO ATTACK");
                            Target = thing;
                            transform.LookAt(Target.transform);
                        }
                    }
                }
            }
            if (Target != null)
            {
                transform.LookAt(Target.transform);
                if (Target.TryGetComponent<Building>(out Building building))
                {
                    building.Health -= Damage;
                }
                if (Target.TryGetComponent<Unit>(out Unit unit))
                {
                    //Debug.Log("SHOT A UNIT");
                    unit.Health -= Damage;
                }
            }


        }
    }
}
