using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyProducer : NetworkBehaviour
{
    public DataHolder data;
    public int Team;
    private float time = 0.0f;
    public float CashInterval;
    public float CashMoney;
    
    void Start()
    {
        if (!isServer)
        {
            GetComponent<MoneyProducer>().enabled = false;
            if (TryGetComponent<Building>(out Building building)) Team = building.Team;
            if (TryGetComponent<Unit>(out Unit unit)) Team = unit.Team;
        }
        else
        {
            if (TryGetComponent<Building>(out Building building)) Team = building.Team;
            if (TryGetComponent<Unit>(out Unit unit)) Team = unit.Team;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time += Time.deltaTime;

        if (time >= CashInterval)
        {
            time = time - CashInterval;
/*            foreach(KeyValuePair<int, CommanderScript> pair in data.ActiveCommanders)
            {
                Debug.Log("ACTIVE COMMANDERS LIST " + pair.Key+":"+pair.Value.name+" TEAM IM LOOKING FOR IS "+Team+"AND I AM "+isServer+" ON SERVER");
            }*/
            //Debug.Log("THING " + gameObject.name+ " GENERATED CASH FOR " + data.ActiveCommanders[Team].name);
            data.ActiveCommanders[Team].Money += CashMoney;
        }
    }
}
