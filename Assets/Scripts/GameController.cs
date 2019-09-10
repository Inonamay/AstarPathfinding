using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //Objects used by the script
    #region
    [SerializeField]
    GameObject floorTile = null;
    [SerializeField]
    GameObject computerCharacter = null;
    GameObject startPos;
    GameObject finishPos;
    GameObject tileParent;
    #endregion
    //Lists
    #region
    List<List<GameObject>> coordinates;
    List<FloorScript> hValuePulseOrder = new List<FloorScript>();
    List<FloorScript> gValuePulseOrder = new List<FloorScript>();
    #endregion
    //Bools
    #region
    bool finishedG = false;
    bool finishedH = false;
    bool isGeneratingGrid;
    #endregion
    //Grid related variabels
    #region
    [SerializeField] int gridSizeX = 10;
    [SerializeField] int gridSizeY = 10;
    [SerializeField] int wallChance = 10;
    int positionsSet = 0;
    #endregion
    //Creates the object that will be the parent to all the tiles and then executes the generate grid function to generate the grid
    void Start()
    { tileParent = new GameObject("TileParent"); StartCoroutine("GenerateGrid"); }
    //Lets the user place the 2 points and then initializes the pathfinding
    private void Update()
    {if(positionsSet < 3) {SettingPoints();}}
    void SettingPoints()
    {
        if (Input.GetMouseButtonDown(0) && positionsSet < 2 && !isGeneratingGrid)
        {
           Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
           RaycastHit hit;
           if (Physics.Raycast(ray, out hit))
           {
               if (hit.collider.tag == "Floor")
               {
                  switch (positionsSet)
                  {
                     case 0: startPos = hit.collider.gameObject; startPos.GetComponent<MeshRenderer>().material.color = Color.green;break;
                     case 1: finishPos = hit.collider.gameObject; finishPos.GetComponent<MeshRenderer>().material.color = Color.yellow; break;
                  }
                  positionsSet++;
               }
           }
        }
        if (positionsSet == 2)
        { Instantiate(computerCharacter); positionsSet++; }
    }
    //A method sending out a pulse from start and finish through all the searched tiles and changes their value to the correct value
    public void CalculateValues()
    {
        startPos.GetComponent<FloorScript>().GetComponent<MeshRenderer>().material.color = Color.blue;
        finishPos.GetComponent<FloorScript>().GetComponent<MeshRenderer>().material.color = Color.yellow;
        StartCoroutine("CalulateHValue");
        StartCoroutine("CalulateGValue");
    }
    //Makes sure that no tile gets the wrong value by making sure every tile changes themselves in the correct order
    IEnumerator CalulateHValue()
    {
        int x = 0;
        hValuePulseOrder.Add(finishPos.GetComponent<FloorScript>());
        hValuePulseOrder[0].SetPulseH();
        while(hValuePulseOrder.Count != 0)
        {
            x++;
            hValuePulseOrder[0].SendHValuePulse();
            hValuePulseOrder.RemoveAt(0);
            if (x % 50 == 0) { yield return null; }
        }
        finishedH = true;
    }
    IEnumerator CalulateGValue()
    {
        int x = 0;
        gValuePulseOrder.Add(startPos.GetComponent<FloorScript>());
        if (gValuePulseOrder[0] != null)
        { gValuePulseOrder[0].SetPulseG();}
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
        isGeneratingGrid = true;
        coordinates = new List<List<GameObject>>();
        for (int y = 0; y < gridSizeY; y++)
        {
            coordinates.Add(new List<GameObject>());
            for (int x = 0; x < gridSizeX; x++)
            {
                coordinates[y].Add(Instantiate(floorTile));
                coordinates[y][x].transform.position = Vector3.up * (y + 0.5f) + Vector3.right * (x + 0.5f);
                coordinates[y][x].transform.parent = tileParent.transform;
            }
            yield return null;
        }
        StartCoroutine("AddConnections");
    }
    //Same as the generate grid function except it adds connections to the tiles so that every tile knows who is its neighbours (and sets their color for a nice curtain effect)
    private IEnumerator AddConnections()
    {
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if(y > 0) { coordinates[y][x].GetComponent<FloorScript>().AddConnectedBlock(coordinates[y - 1][x].GetComponent<FloorScript>()); }
                if (y < gridSizeY - 1) { coordinates[y][x].GetComponent<FloorScript>().AddConnectedBlock(coordinates[y + 1][x].GetComponent<FloorScript>()); }
                if(x > 0) { coordinates[y][x].GetComponent<FloorScript>().AddConnectedBlock(coordinates[y][x - 1].GetComponent<FloorScript>()); }
                if(x < gridSizeX - 1) { coordinates[y][x].GetComponent<FloorScript>().AddConnectedBlock(coordinates[y][x + 1].GetComponent<FloorScript>()); }
                if (coordinates[y][x].GetComponent<FloorScript>().GetTileType() != 1) { coordinates[y][x].GetComponent<MeshRenderer>().material.color = Color.white; }
            }
            yield return null;
        }
        isGeneratingGrid = false;
    }
    //Getters and Setters
    #region
    public void AddHValueCalcOrder(FloorScript target)
    { hValuePulseOrder.Add(target);}
    public void AddGValueCalcOrder(FloorScript target)
    { gValuePulseOrder.Add(target); }
    public List<List<GameObject>> GetCoordinates()
    {return coordinates;}
    public GameObject GetStart()
    {return startPos;}
    public GameObject GetFinish()
    { return finishPos;}
    public bool IsFinishedCalculating()
    {
        if(finishedG && finishedH) { return true; }
        else { return false; }
    }
    public int GetWallChance()
    {return wallChance;}
    public float GetArea()
    {
        return gridSizeX * gridSizeY;
    }
    #endregion
}
