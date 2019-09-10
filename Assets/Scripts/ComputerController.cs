using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComputerController : MonoBehaviour
{
    //Scripts
    #region
    GameController gameControllerScript;
    List<FloorScript> closedList = new List<FloorScript>();
    List<FloorScript> openList = new List<FloorScript>();
    FloorScript currentTileScript;
    #endregion
    int steps = 0;
    GameObject finishTile;
    // Sets the object correctly and sets up the variables
    void Start()
    {
        gameControllerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        ResetComputer();
        StartCoroutine("PathFind");
    }
    IEnumerator PathFind()
    {
        int x = 0;
        bool b = true;
        while (b)
        {
            x++;
            if (currentTileScript.gameObject != gameControllerScript.GetFinish())
            {
                currentTileScript = FindClosestTile();
                if(currentTileScript == null)
                { StopAllCoroutines(); yield return null; }
                UpdatePos();
                //Uncomment to se the all the paths the computer takes or tries to take
                //currentTileScript.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                if(x % 50 == 0) { yield return null; }
            }
            else
            {
                openList.Remove(gameControllerScript.GetStart().GetComponent<FloorScript>());
                gameControllerScript.CalculateValues();
                if(x % 50 == 0) { yield return null; }
                for (int i = 0; i < openList.Count; i++)
                { openList[i].SetGValue(20000); }
                if (gameControllerScript.IsFinishedCalculating())
                {
                    currentTileScript = gameControllerScript.GetStart().GetComponent<FloorScript>();
                    StartCoroutine("ShowShortestPath");
                    b = false;
                }
            }
        }
    }
    void UpdatePos()
    {
        openList.Remove(currentTileScript);
        closedList.Add(currentTileScript);
        transform.position = currentTileScript.gameObject.transform.position - Vector3.forward;
    }
    IEnumerator ShowShortestPath()
    {
        int b = 0;
        while (true)
        {
            b++;
            if (currentTileScript.gameObject != gameControllerScript.GetFinish())
            {
                FloorScript tempScript = currentTileScript;
                for (int i = 0; i < currentTileScript.GetConnectedBlocks().Count; i++)
                {
                    if (tempScript.GetDistance() >= currentTileScript.GetConnectedBlocks()[i].GetDistance() && tempScript.GetGValue() < currentTileScript.GetConnectedBlocks()[i].GetGValue())
                    { tempScript = currentTileScript.GetConnectedBlocks()[i]; }
                }
                currentTileScript = tempScript;
                currentTileScript.GetComponent<MeshRenderer>().material.color = Color.cyan;
                transform.position = currentTileScript.gameObject.transform.position - Vector3.forward;
            }
            else
            { StopCoroutine("ShowShortestPath"); }
            if(b % 50 == 0) {yield return null;}
        }
    }
    void ResetComputer()
    {
        closedList.Add(gameControllerScript.GetStart().GetComponent<FloorScript>());
        openList.Remove(gameControllerScript.GetStart().GetComponent<FloorScript>());
        steps = 0;
        currentTileScript = gameControllerScript.GetStart().GetComponent<FloorScript>();
        openList.Add(currentTileScript);
        transform.position = currentTileScript.gameObject.transform.position - Vector3.forward;
        CalculateTile(currentTileScript);
    }
    private FloorScript FindClosestTile()
    {
        steps = currentTileScript.GetGValue() + 1;
        for (int i = 0; i < currentTileScript.GetConnectedBlocks().Count; i++)
        {
            if (!(closedList.Contains(currentTileScript.GetConnectedBlocks()[i])) && !(openList.Contains(currentTileScript.GetConnectedBlocks()[i])))
            {
                CalculateTile(currentTileScript.GetConnectedBlocks()[i]);
                openList.Add(currentTileScript.GetConnectedBlocks()[i]);
                //Uncomment to see the openlist
               // currentTileScript.GetConnectedBlocks()[i].gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
            }
        }
        if (openList.Count != 0)
        {
            FloorScript tempScript = openList[0];
            for (int i = 0; i < openList.Count; i++)
            {
                if (gameControllerScript.GetArea() > 5624)
                {
                    if (tempScript.GetDistance() >= openList[i].GetDistance() || tempScript.GetHValue() >= openList[i].GetHValue())
                    { tempScript = openList[i]; }
                }
                else
                {
                    if (tempScript.GetDistance() >= openList[i].GetDistance() && tempScript.GetHValue() >= openList[i].GetHValue())
                    { tempScript = openList[i]; }
                }
                if (openList[i] == gameControllerScript.GetFinish().GetComponent<FloorScript>())
                { return openList[i]; }
            }
            return tempScript;
        }
        else
        {
            print("No path available");
            return null;
        }
    }
    void CalculateTile(FloorScript target)
    {
        if(target.GetGValue() > steps || target.GetGValue() == 0)
        {target.SetGValue(steps);}
        target.CalculateH();
    }
    public List<FloorScript> GetClosedList()
    {return closedList; }
}
