using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public DataHolder data;
    public static bool[] Teams;

    // Start is called before the first frame update
    void Start()
    {
        Teams = new bool[data.TeamColors.Length];
        for (int i = 0; i < Teams.Length; i++)
        {
            Teams[i] = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static void onJoin()
    {

    }
    public static int getTeam()
    {
        Debug.Log("ASKED FOR TEAM");
        List<int> openTeams = new List<int>();
        for(int i=0;i<Teams.Length;i++)
        {
            if (Teams[i])
            {
                openTeams.Add(i);
                Debug.Log("team " + i + " is open");
            }
        }
        int randTeam = openTeams[Random.Range(0, openTeams.Count)];
        Teams[randTeam] = false;
        return randTeam;
    }

    public static void leaveTeam(int team)
    {
        Teams[team] = true;
    }
}
