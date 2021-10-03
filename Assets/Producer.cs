using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Building))]
public class Producer : NetworkBehaviour
{
    public DataHolder data;
    public Building building;
    public GameObject[] ProducedUnits;
    private GameObject ProduceGui;
    // Start is called before the first frame update
    void Start()
    {
        building = GetComponent<Building>();
    }
    public void Produce(int index)
    {
        Debug.Log("CLICKED "+index);
        if(data.money>= ProducedUnits[index].GetComponent<Unit>().Cost)
        {
            data.Commander.SpawnUnit(System.Array.IndexOf(data.Units,ProducedUnits[index]) , transform.position + new Vector3(0, 1, 0), building.Team);
            data.Commander.SpendMoney(ProducedUnits[index].GetComponent<Unit>().Cost);
        }
    }

    // Update is called once per frame
    void Update()
    {
/*        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) if (hit.transform == transform) OnClick(0);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) if (hit.transform == transform) OnClick(1);
        }
        if (Input.GetMouseButtonDown(2))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) if (hit.transform == transform) OnClick(2);
        }*/
    }
    public void OnClick(int mouseButton, int team)
    {
        if (System.Math.Abs(team - building.Team) < 1)
        {
            if (mouseButton == 1)
            {
                if (ProduceGui != null)
                {

                    ProduceGui.SetActive(!ProduceGui.activeSelf);
                }
                else
                {
                    ProduceGui = Instantiate(data.ProducerPrefabs[0]);
                    ProduceGui.transform.SetParent(gameObject.transform, false);
                    ProduceGui.transform.localPosition = new Vector3(0, 1, 0);
                    ProduceGui.GetComponent<Canvas>().worldCamera=Camera.main;
                    for(int i = 0; i < ProducedUnits.Length; i++)
                    {
                        GameObject tempButton = Instantiate(data.ProducerPrefabs[1]);
                        string name = ProducedUnits[i].name;
                        tempButton.GetComponent<TextMeshProUGUI>().text = ProducedUnits[i].GetComponent<Unit>().Cost+"$ "+name;
                        int temp = i;
                        tempButton.GetComponent<Button>().onClick.AddListener(delegate {
                            Debug.Log("index is " + temp);
                            Produce(temp);
                        });
                        tempButton.transform.SetParent(ProduceGui.transform, false);
                        tempButton.transform.localPosition = new Vector3(0, (((ProducedUnits.Length/2f)-i) / 10f),0);
                    }
                    foreach (GameObject thing in ProducedUnits)
                    {
                    }
                }
                /*                data.Commander.SpawnUnit(0, transform.position + new Vector3(0, 1, 0),team);
                                Debug.Log("Produced");*/
            }
        }
    }
    

}
