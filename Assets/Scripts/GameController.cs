using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    #region Objects
    [SerializeField]
    GameObject floorTile = null;
    [SerializeField]
    GameObject computerCharacter = null;
    GameObject startPos;
    GameObject finishPos;
    GameObject tileParent;
    #endregion
    #region Lists
    List<List<GameObject>> coordinates;
    List<TileController> hValuePulseOrder = new List<TileController>();
    List<TileController> gValuePulseOrder = new List<TileController>();
    #endregion
    #region Bools
    bool finishedG = false;
    bool finishedH = false;
    bool isGeneratingGrid;
    #endregion
    #region Grid Variables
    [SerializeField] int gridSizeX = 10;
    [SerializeField] int gridSizeY = 10;
    [SerializeField] int wallChance = 10;
    int positionsSet = 0;
    #endregion
    //Creates the object that will be the parent to all the tiles and then executes the generate grid function to generate the grid
    void Start()
    {
        if (gridSizeX > 150) { gridSizeX = 100; }
        if (gridSizeY > 150) { gridSizeY = 100; }
        Camera mainCam = Camera.main;
        mainCam.orthographicSize = Mathf.Max(gridSizeY * 0.5f, gridSizeX * 0.29f);
        mainCam.transform.position = new Vector3(gridSizeX * 0.5f, gridSizeY * 0.5f, -10);
        tileParent = new GameObject("TileParent");
        StartCoroutine(GenerateGrid());
    }
    //Lets the user place the 2 points and then initializes the pathfinding
    private void Update()
    {
        if (positionsSet < 3) { SetupConditions(); }
        if (Input.GetKeyDown(KeyCode.Return)) { SceneManager.LoadScene(0); }
    }
    void SetupConditions()
    {
        if (Input.GetMouseButtonDown(0) && positionsSet < 2 && !isGeneratingGrid)
        { SetPosition(); }
        if (positionsSet == 2)
        {
            if (computerCharacter != null) { Instantiate(computerCharacter); }
            else { print("Error: Computer object not assigned"); }
            positionsSet++;
        }
    }
    void SetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.tag == "Floor" && hit.collider.GetComponent<MeshRenderer>() != null && hit.collider.GetComponent<TileController>() != null)
            {
                switch (positionsSet)
                {
                    case 0:
                        startPos = hit.collider.gameObject;
                        startPos.GetComponent<MeshRenderer>().material.color = Color.green;
                        break;
                    case 1:
                        finishPos = hit.collider.gameObject;
                        finishPos.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        break;
                }
                positionsSet++;
            }
        }
    }
    //A method sending out a pulse from start and finish through all the searched tiles and changes their value to the correct value
    public void CalculateValues()
    {
        if(startPos != null && finishPos != null)
        {
            startPos.GetComponent<MeshRenderer>().material.color = Color.blue;
            finishPos.GetComponent<MeshRenderer>().material.color = Color.yellow;
            StartCoroutine("CalulateHValue");
            StartCoroutine("CalulateGValue");
        }
        else
        { print("Error: There is no finish or start point set"); }
    }
    //Makes sure that no tile gets the wrong value by making sure every tile changes themselves in the correct order
    private IEnumerator CalulateHValue()
    {
        int x = 0;
        hValuePulseOrder.Add(finishPos.GetComponent<TileController>());
        hValuePulseOrder[0].SetPulsedHStatusTrue();
        while(hValuePulseOrder.Count != 0)
        {
            x++;
            hValuePulseOrder[0].SendHValuePulse();
            hValuePulseOrder.RemoveAt(0);
            if (x % 50 == 0) { yield return null; }
        }
        finishedH = true;
    }
    private IEnumerator CalulateGValue()
    {
        int x = 0;
        gValuePulseOrder.Add(startPos.GetComponent<TileController>());
        if (gValuePulseOrder[0] != null)
        { gValuePulseOrder[0].SetPulsedGStatusTrue();}
        while (gValuePulseOrder.Count != 0)
        {
            x++;
            gValuePulseOrder[0].SendGValuePulse();
            gValuePulseOrder.RemoveAt(0);
            if (x % 50 == 0) { yield return null; }
        }
        finishedG = true;
    }
    //Double for loop adding new tiles in a list with lists of gameobjects, the placing them correctly and set their parent to an empy gameobject so that they dont take up too much space in the hierarchy
    private IEnumerator GenerateGrid()
    {
        GameObject[] duplicates = GameObject.FindGameObjectsWithTag("GameController");
        for (int i = 0; i < duplicates.Length; i++)
        {
            if (duplicates[i] != gameObject)
            {
                print("Warning! Found object with tag GameController: " + duplicates[i] + ". There should only be one object with the tag GameController");
                duplicates[i].tag = "Untagged";
            }
        }
        if (floorTile != null)
        {
            isGeneratingGrid = true;
            coordinates = new List<List<GameObject>>();
            for (int y = 0; y < gridSizeY; y++)
            {
                coordinates.Add(new List<GameObject>());
                for (int x = 0; x < gridSizeX; x++)
                {CreateTile(y, x); }
                yield return null;
            }
            StartCoroutine(AddConnections());
        }
        else { print("Error: Tile Object is not assigned"); }
       
    }
    //Creates the tile and places it in the correct position
    void CreateTile(int y, int x)
    {
        coordinates[y].Add(Instantiate(floorTile));
        coordinates[y][x].transform.position = Vector3.up * (y + 0.5f) + Vector3.right * (x + 0.5f);
        coordinates[y][x].transform.parent = tileParent.transform;
    }
    //Same as the generate grid function except it adds connections to the tiles so that every tile knows who is its neighbours (and sets their color for a nice curtain effect)
    private IEnumerator AddConnections()
    {
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                try
                {
                    if (y > 0)
                    { coordinates[y][x].GetComponent<TileController>().AddConnectedBlock(coordinates[y - 1][x].GetComponent<TileController>()); }
                    if (y < gridSizeY - 1)
                    { coordinates[y][x].GetComponent<TileController>().AddConnectedBlock(coordinates[y + 1][x].GetComponent<TileController>()); }
                    if (x > 0)
                    { coordinates[y][x].GetComponent<TileController>().AddConnectedBlock(coordinates[y][x - 1].GetComponent<TileController>()); }
                    if (x < gridSizeX - 1)
                    { coordinates[y][x].GetComponent<TileController>().AddConnectedBlock(coordinates[y][x + 1].GetComponent<TileController>()); }
                    if (coordinates[y][x].GetComponent<TileController>().GetTileType() != 1)
                    { coordinates[y][x].GetComponent<MeshRenderer>().material.color = Color.white; }
                }
                catch { print("Error: The tile object being created does not have the necessary component: TileController"); }
            }
            yield return null;
        }
        isGeneratingGrid = false;
    }
    #region Getters and Setters
    public void AddHValueCalcOrder(TileController target)
    { hValuePulseOrder.Add(target); }
    public void AddGValueCalcOrder(TileController target)
    { gValuePulseOrder.Add(target); }
    public List<List<GameObject>> GetCoordinates()
    { return coordinates; }
    public GameObject GetStart()
    { return startPos; }
    public GameObject GetFinish()
    { return finishPos; }
    public bool IsFinishedCalculating()
    {
        if(finishedG && finishedH){return true;}
        else { return false; }
    }
    public int GetWallChance()
    { return wallChance; }
    public float GetArea()
    { return gridSizeX * gridSizeY; }
    #endregion
}
