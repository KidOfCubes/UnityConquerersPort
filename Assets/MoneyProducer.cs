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
            this.enabled = false;
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
            data.ActiveCommanders[Team].GetComponent<CommanderScript>().Money += CashMoney;
        }
    }
}
