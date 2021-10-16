using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommanderScript : NetworkBehaviour
{
    // Start is called before the first frame update
    public float MouseSens = 200f;
    public Camera playerCamera;
    public float Speed = 1f;
    float xRotation = 0f;
    float yRotation = 0f;
    public DataHolder data;
    public TextMeshProUGUI MoneyCounter;
    public Button SellButton;
    [SyncVar]
    public int Team;

    [SyncVar(hook = nameof(OnMoneyChanged))]
    public float Money;


    private float ScreenSelectTime;
    private Vector3 SelectScreenPoint1;
    private Vector3 SelectScreenPoint2;

    public GameObject dot;

    public GameObject selectedGameObject;

    public Vector2 SizeOfSelected;

    //[SyncVar]
    public List<GameObject> selected = new List<GameObject>();

    Dictionary<GameObject, GameObject> selectedUiGhosts = new Dictionary<GameObject, GameObject>();
    Dictionary<Vector2, GameObject> selectedUiGhostPositions = new Dictionary<Vector2, GameObject>();

    void Start()
    {
        Debug.Log("A COMMANDER CALLED " + gameObject.name);
        if (isServer)
        {
            GameManager.onJoin();
            Team = GameManager.getTeam();
            Debug.Log("MY TEAM IS NOW " + Team+ "and i added "+ GetComponent<CommanderScript>().name);
            data.ActiveCommanders.Add(Team, GetComponent<CommanderScript>());
            foreach (KeyValuePair<int, CommanderScript> pair in data.ActiveCommanders)
            {
                Debug.Log("ACTIVE COMMANDERS LIST100 " + pair.Key + ":" + pair.Value.name);
            }
        }
        Debug.LogError("CONSOLE OPEN");
        if (!isLocalPlayer)
        {
            Debug.Log("DESTROYED HAHA");
            GetComponent<CommanderScript>().enabled = false;
            Destroy(MoneyCounter.gameObject);
            //Destroy(gameObject);
        }
        else
        {
            data.Commander = GetComponent<CommanderScript>();
            data.money = Money;
            Camera.main.enabled = false;
            playerCamera.enabled = true;
            for (int x = 0; x < SizeOfSelected.x; x++)
            {
                for (int y = 0; y < SizeOfSelected.y; y++)
                {
                    selectedUiGhostPositions.Add(new Vector2(x, y), null);
                }
            }
            MoneyCounter.color = data.TeamColors[Team];
        }
    }
    private void OnDisable()
    {

        if (isLocalPlayer)
        {
            Camera.main.enabled = true;
            playerCamera.enabled = false;
        }
    }
    private void OnDestroy()
    {
        if (isServer)
        {
            data.ActiveCommanders.Remove(Team);
            GameManager.leaveTeam(Team);
        }
        if (Team == data.Commander.Team)
        {
            foreach (GameObject unit in data.ActiveUnits)
            {
                if (unit.GetComponent<Unit>().Team == Team)
                {
                    unit.GetComponent<Unit>().die();
                }
            }
            foreach (GameObject building in data.ActiveBuildings)
            {
                if (building.GetComponent<Building>().Team == Team)
                {
                    building.GetComponent<Building>().die();
                }
            }
        }

    }
    private void OnMoneyChanged(float oldValue, float newValue)
    {
        //Debug.Log("IS LOCAL PLAYER" + isLocalPlayer+" OLD VALUE "+oldValue+" NEW VALUE"+newValue);
        //Debug.Log("IS LOCAL PLAYER" + isLocalPlayer + " FROM " + gameObject.name);
        if (isLocalPlayer)
        {
            data.money = Money;
            MoneyCounter.color = data.TeamColors[Team];
            MoneyCounter.text = "Money: " + Money;
        }
    }
    [Command]
    public void SpendMoney(float amount)
    {
        Money -= amount;
    }
    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSens * Time.deltaTime;

        if (Input.GetMouseButton(1))
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            yRotation += mouseX;
        }
        //transform.Rotate((Vector3.up * mouseX)+(Vector3.right*mouseY));
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        //playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);


        if (Input.GetMouseButtonDown(0))
        {
            SelectScreenPoint1 = Input.mousePosition;
            ScreenSelectTime = Time.time;
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.TryGetComponent<NetworkIdentity>(out NetworkIdentity identity))
                {
                    Debug.Log("CLICK ON OBJECT from team " + Team);
                    ClickOnObject(1, hit.collider.gameObject);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if((Input.mousePosition-SelectScreenPoint1).magnitude>0.1f)
            //if (Time.time-ScreenSelectTime>0.1)
            {
                SelectScreenPoint2 = Input.mousePosition;
/*                GameObject tempdot = Instantiate(dot);
                tempdot.transform.parent = MoneyCounter.transform.parent;
                tempdot.GetComponent<RectTransform>().position = SelectScreenPoint1;
                tempdot = Instantiate(dot);
                tempdot.transform.parent = MoneyCounter.transform.parent;
                tempdot.GetComponent<RectTransform>().position = SelectScreenPoint2;*/

                
                Bounds bounds = new Bounds(
                    (new Vector3((SelectScreenPoint1.x + SelectScreenPoint2.x) / 2, (SelectScreenPoint1.y + SelectScreenPoint2.y) / 2, (SelectScreenPoint1.z + SelectScreenPoint2.z) / 2))
                    , (new Vector3(
                      (System.Math.Abs(SelectScreenPoint1.x - SelectScreenPoint2.x) / 2) + 50f
                    , (System.Math.Abs(SelectScreenPoint1.y - SelectScreenPoint2.y) / 2) + 50f
                    , (System.Math.Abs(SelectScreenPoint1.z - SelectScreenPoint2.z) / 2) + 50f)));
                //Debug.Log("SELECTED "+bounds);
                foreach (GameObject thing in data.ActiveUnits)
                {
                    Vector3 tempPoint = playerCamera.WorldToScreenPoint(thing.transform.position);
                    tempPoint.z = 0;
/*                    tempdot = Instantiate(dot);
                    tempdot.transform.parent = MoneyCounter.transform.parent;
                    tempdot.GetComponent<RectTransform>().position = tempPoint;*/
                    //Debug.Log(tempPoint);
                    if (bounds.Contains(tempPoint))
                    {
                        ClickOnObject(0, thing);
                    }
                }
            }
            else
            {

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log("HIT ONTO " + hit.collider.gameObject.name);
                    if (hit.collider.gameObject.TryGetComponent<NetworkIdentity>(out NetworkIdentity identity))
                    {
                        Debug.Log("CLICK ON OBJECT from team " + Team);
                        ClickOnObject(0, hit.collider.gameObject);
                    }
                    else
                    {
                        foreach (GameObject thing in selected)
                        {
                            if (thing.TryGetComponent<Unit>(out Unit unit))
                            {
                                unit.setTarget(hit.point, Team);
                            }
                        }
                    }
                }
            }
            if (Input.GetMouseButton(0))
            {

            }
        }
    }
    public void ClickOnObject(int mousebutton, GameObject thing)
    {
        if (mousebutton == 1)
        {
            if (thing.TryGetComponent<Producer>(out Producer producer))
            {
                if (producer.enabled)
                {
                    Debug.Log("CLICK produce from team " + Team);
                    producer.OnClick(mousebutton, Team);
                }
            }
        }
        if (mousebutton == 0)
        {
            if (thing.TryGetComponent<Unit>(out Unit unit))
            {
                Debug.Log("CLICK unit from team " + Team);
                if (unit.ParentUnit != null)
                {
                    unit = unit.ParentUnit;
                }
                if (unit.Team != Team) { return; }
                if (!unit.enabled) { return; }
                if (unit.select(Team))
                {
                    selected.Add(unit.gameObject);
                    SelectedInGui(unit.gameObject, true);
                }
                else
                {
                    SelectedInGui(unit.gameObject, false);
                    selected.Remove(unit.gameObject);
                }
            }
            if (thing.TryGetComponent<Building>(out Building building))
            {
                Debug.Log("CLICK unit from team " + Team);
                if (building.ParentBuilding != null)
                {
                    building = building.ParentBuilding;
                }
                if (building.Team != Team) { return; }
                if (!building.enabled) { return; }
                if (building.select(Team))
                {
                    selected.Add(building.gameObject);
                    SelectedInGui(building.gameObject, true);
                }
                else
                {
                    SelectedInGui(building.gameObject, false);
                    selected.Remove(building.gameObject);
                }
            }
        }
    }
    public void SelectedInGui(GameObject thing, bool select)
    {
        if (select)
        {
            GameObject ghost=null;
            if (thing.TryGetComponent<Building>(out Building building))
            {
                ghost = Instantiate(data.Buildings[building.BuildingIndex]);
            }
            if (thing.TryGetComponent<Unit>(out Unit unit))
            {
                ghost = Instantiate(data.Units[unit.UnitIndex]);
            }
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
            Material[] materials = new Material[meshFilters.Length - 1];
            CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];
            int j = 0;
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.gameObject != ghost)
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
            ghost.transform.GetComponent<MeshFilter>().mesh = new Mesh();
            ghost.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, false, true);
            ghost.transform.GetComponent<MeshCollider>().sharedMesh = new Mesh();
            ghost.transform.GetComponent<MeshCollider>().sharedMesh.CombineMeshes(combine, false, true);
            ghost.transform.GetComponent<MeshCollider>().isTrigger = true;
            ghost.transform.GetComponent<MeshRenderer>().materials = materials;
            ghost.transform.GetComponent<MeshCollider>().isTrigger = true;
            if (thing.TryGetComponent<Building>(out _))
            {
                ghost.transform.GetComponent<Building>().ParentBuilding = building;
                ghost.transform.GetComponent<Building>().UpdateMesh();
            }
            if (thing.TryGetComponent<Unit>(out _))
            {
                ghost.transform.GetComponent<Unit>().ParentUnit = unit;
                ghost.transform.GetComponent<Unit>().UpdateMesh();
            }
            
            ghost.layer = 5;
            ghost.transform.gameObject.SetActive(true);
            for (int i = 0; i < ghost.transform.childCount; i++)
            {
                //ghost.transform.GetChild(i).gameObject.SetActive(false);
                foreach (Component component in ghost.transform.GetChild(i).GetComponents<Component>())
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
            /*                    Destroy(ghost.transform);
                                ghost.AddComponent<RectTransform>();*/
            ghost.transform.SetParent(selectedGameObject.transform, true);
            if (thing.TryGetComponent<Building>(out _))
            {
                ghost.transform.localScale = new Vector3(55, 55, 55);
                ghost.transform.localRotation = Quaternion.Euler(-25, 315, 25);
                ghost.GetComponent<Building>().makeHealthBar();
                building.healthBarScript.otherInstances.Add(ghost.GetComponent<Building>().healthBarScript);
                building.healthBarScript.UpdateValues();

            }
            if (thing.TryGetComponent<Unit>(out _))
            {
                ghost.transform.localScale = new Vector3(150, 150, 150);
                ghost.transform.localRotation = Quaternion.Euler(10, -225, -10);
                ghost.GetComponent<Unit>().makeHealthBar();
                unit.healthBarScript.otherInstances.Add(ghost.GetComponent<Unit>().healthBarScript);
                unit.healthBarScript.UpdateValues();
            }
            //ghost.transform.localRotation = Quaternion.Euler(0,0,0);
            ghost.transform.gameObject.SetActive(true);
            selectedUiGhosts.Add(thing, ghost);
            addGhostToUi(ghost);


        }
        else
        {
            GameObject ghost = selectedUiGhosts[thing];
            selectedUiGhosts.Remove(thing);
            removeGhostFromUi(ghost);
        }
    }
    public void removeGhostFromUi(GameObject ghost)
    {
        Vector2 keytoDelete = new Vector2(0, 0);
        foreach (KeyValuePair<Vector2, GameObject> pair in selectedUiGhostPositions)
        {
            if (pair.Value == ghost)
            {
                keytoDelete = pair.Key;
            }
        }
        selectedUiGhostPositions[keytoDelete] = null;
        Destroy(ghost);
        updateSelectedUi();

    }
    public void addGhostToUi(GameObject ghost)
    {
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (selectedUiGhostPositions[new Vector2(x, y)] == null)
                {
                    selectedUiGhostPositions[new Vector2(x, y)] = ghost;
                    updateSelectedUi();
                    return;
                }
            }
        }
    }
    public void updateSelectedUi()
    {
        bool preivousempty = false;
        Vector2 previousPosition = Vector2.zero;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                KeyValuePair<Vector2, GameObject> pair = new KeyValuePair<Vector2, GameObject>(new Vector2(x, y), selectedUiGhostPositions[new Vector2(x, y)]);
                if (pair.Value == null)
                {
                    preivousempty = true;
                }
                else
                {
                    if (preivousempty)
                    {
/*                        Debug.Log("RN I HAVE " + pair.Value.name);
                        Debug.Log("MOVED SMTH " + previousPosition);
                        Debug.Log("MOVED SMTH " + pair.Value.name);*/
                        selectedUiGhostPositions[previousPosition] = pair.Value;
                        selectedUiGhostPositions[pair.Key] = null;


                    }
                }
                if (selectedUiGhostPositions[previousPosition]!=null)
                {
                    if (selectedUiGhostPositions[previousPosition].TryGetComponent<Building>(out _))
                    {
                        selectedUiGhostPositions[previousPosition].transform.localPosition = new Vector3((previousPosition.x * 60) + 30, (previousPosition.y * 60) + 30, 0);
                        selectedUiGhostPositions[previousPosition].transform.localScale = new Vector3(55, 55, 55);
                        selectedUiGhostPositions[previousPosition].transform.localRotation = Quaternion.Euler(-25, 315, 25);
                    }
                    if (selectedUiGhostPositions[previousPosition].TryGetComponent<Unit>(out _))
                    {
                        selectedUiGhostPositions[previousPosition].transform.localPosition = new Vector3((previousPosition.x * 60) + 30, (previousPosition.y * 60) + 15, 0);
                        selectedUiGhostPositions[previousPosition].transform.localScale = new Vector3(150, 150, 150);
                        selectedUiGhostPositions[previousPosition].transform.localRotation = Quaternion.Euler(10, -225, -10);
                    }
                }
                previousPosition = new Vector2(x, y);
            }
        }
        if (selectedUiGhostPositions[previousPosition] != null)
        {
            if (selectedUiGhostPositions[previousPosition].TryGetComponent<Building>(out _))
            {
                selectedUiGhostPositions[previousPosition].transform.localPosition = new Vector3((previousPosition.x * 60) + 30, (previousPosition.y * 60) + 30, 0);
                selectedUiGhostPositions[previousPosition].transform.localScale = new Vector3(55, 55, 55);
                selectedUiGhostPositions[previousPosition].transform.localRotation = Quaternion.Euler(-25, 315, 25);
            }
            if (selectedUiGhostPositions[previousPosition].TryGetComponent<Unit>(out _))
            {
                selectedUiGhostPositions[previousPosition].transform.localPosition = new Vector3((previousPosition.x * 60) + 30, (previousPosition.y * 60) + 15, 0);
                selectedUiGhostPositions[previousPosition].transform.localScale = new Vector3(150, 150, 150);
                selectedUiGhostPositions[previousPosition].transform.localRotation = Quaternion.Euler(10, -225, -10);
            }
        }

        if (selectedUiGhosts.Count == 1)
        {
            foreach (KeyValuePair<GameObject, GameObject> pair in selectedUiGhosts)
            {
                SellButton.gameObject.SetActive(true);
                if (pair.Value.TryGetComponent<Building>(out Building building))
                {
                    pair.Value.transform.localPosition = new Vector3((2.5f * 60) + 30, (1.5f * 60), 0);
                    pair.Value.transform.localScale = new Vector3(275, 275, 275);
                    SellButton.GetComponentInChildren<TextMeshProUGUI>().text = "SELL " + (building.Cost / 2f);
                }
                if (pair.Value.TryGetComponent<Unit>(out Unit unit))
                {
                    pair.Value.transform.localPosition = new Vector3((2.5f * 60) + 30, (0f * 60) + 30, 0);
                    pair.Value.transform.localScale = new Vector3(750, 750, 750);
                    SellButton.GetComponentInChildren<TextMeshProUGUI>().text = "SELL " + (unit.Cost / 2f);
                }
            }
        }
        else
        {
            SellButton.gameObject.SetActive(false);
        }
    }
    public void clickSellButton()
    {
        foreach (KeyValuePair<GameObject, GameObject> pair in selectedUiGhosts)
        {
            if (pair.Value.TryGetComponent<Building>(out Building building))
            {
                SpendMoney((building.Cost / 2f) * -1f);
                building.die();
            }
            if (pair.Value.TryGetComponent<Unit>(out Unit unit))
            {
                SpendMoney((unit.Cost / 2f) * -1f);
                unit.die();
            }
        }
        updateSelectedUi();
    }
    [Command]
    public void SpawnUnit(int index, Vector3 position, int team)
    {
        Debug.Log("SPANED UNEDT "+index);
        GameObject thing = Instantiate(data.Units[index], position, Quaternion.identity);
        thing.GetComponent<Unit>().Team = team;
        thing.GetComponent<Unit>().UnitIndex = index;
        NetworkServer.Spawn(thing, connectionToClient);
    }
    [Command]
    public void SpawnBuilding(int index, Vector3 position, int team)
    {
        GameObject thing = Instantiate(data.Buildings[index], position, Quaternion.identity);
        thing.GetComponent<Building>().Team = team;
        thing.GetComponent<Building>().BuildingIndex = index;
        NetworkServer.Spawn(thing, connectionToClient);
    }   
    public void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position = transform.position + (transform.forward*Speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position = transform.position + (transform.right*-1*Speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position = transform.position + (transform.forward*-1*Speed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position = transform.position + (transform.right* Speed);
        }
    }
}
