using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComputerController : MonoBehaviour
{
    #region Script Variables
    GameController gameControllerScript;
    List<TileController> closedList = new List<TileController>();
    List<TileController> openList = new List<TileController>();
    TileController currentTileScript;
    #endregion
    int steps = 0;
    GameObject finishTile;
    [SerializeField] bool showTestedTiles = false;
    [SerializeField] int thresholdForSearchMethod = 6000;
    TileController start;
    // Sets the object correctly and sets up the variables
    void Start()
    {
        try
        {
            gameControllerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            start = gameControllerScript.GetStart().GetComponent<TileController>();
            ResetComputer();
            StartCoroutine("PathFind");
        }
        catch
        { print("Error: Something went wrong with the computer, possible errors: Game Controller does not have the necessary tag (GameController), Gamecontroller does not have the necessary script, Gamecontroller does not contain a definition for a start tile");}
    }
    IEnumerator PathFind()
    {
        //The x variable is to optimse so that the computer searches multiple tiles every frame
        int b = 0;
        while (true)
        {
            b++;
            //The computer searches until it arrives at the finish tile then it switches over to the mode where it sets up all the tiles with the correct values and shows the shortest route it found
            if (currentTileScript.gameObject != gameControllerScript.GetFinish())
            {
                currentTileScript = FindClosestTile();
                if(currentTileScript == null) { break; }
                UpdateLists();
                if (showTestedTiles) { currentTileScript.gameObject.GetComponent<MeshRenderer>().material.color = Color.red; }
                if(b % 50 == 0) { yield return null; b = 0; }
            }
            else
            {
                openList.Remove(start);
                gameControllerScript.CalculateValues();
                if(b % 50 == 0) { yield return null; b = 0; }
                for (int i = 0; i < openList.Count; i++)
                { openList[i].SetGValue(99999); }
                if (gameControllerScript.IsFinishedCalculating())
                {
                    currentTileScript = start;
                    StartCoroutine("ShowShortestPath");
                    break;
                }
            }
        }
    }
    //Makes sure the computer does not go back to the same tile
    void UpdateLists()
    {
        openList.Remove(currentTileScript);
        closedList.Add(currentTileScript);
    }
    //This is a coroutine so that the computer is not limited to one tile per second
    IEnumerator ShowShortestPath()
    {
        int b = 0;
        while (true)
        {
            //Since the shortest path always will have the same F value (unless the tiles have a movecost) the computer simply shows a path where the F cost is the same from start to finish
            b++;
            if (b % 50 == 0) { yield return null; b = 0; }
            if (currentTileScript.gameObject != gameControllerScript.GetFinish())
            {
                TileController tempScript = currentTileScript;
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
            { break; }
        }
    }
    //Only used at the beginning to set the computer to the first tile
    void ResetComputer()
    {
        openList.Clear();
        closedList.Clear();
        closedList.Add(start);
        steps = 0;
        currentTileScript = start;
        CalculateTile(currentTileScript);
    }
    //Most of the pathfinding logic
    TileController FindClosestTile()
    {
        //Increases the steps taken every time the computer takes a step
        steps = currentTileScript.GetGValue() + 1;
        //Searches the current tiles neighbors and if they are not walls and they are not already int the closed or open list, it adds them to the open list and calulates their F value
        for (int i = 0; i < currentTileScript.GetConnectedBlocks().Count; i++)
        {
            if (!(closedList.Contains(currentTileScript.GetConnectedBlocks()[i])) && !(openList.Contains(currentTileScript.GetConnectedBlocks()[i])))
            {
                CalculateTile(currentTileScript.GetConnectedBlocks()[i]);
                openList.Add(currentTileScript.GetConnectedBlocks()[i]);
            }
        }
        //Searches through the open list and goes to the tile with the lowest F cost and depending on mapsize either prioritize tiles which have a lower H cost(meaning they are closer to finish) or checks all paths
        //If the open list is empty, meaning there are no more tiles to check because the ai is boxed in, it stops the path finding and prints an error message
        if (openList.Count != 0)
        {
            TileController tempScript = openList[0];
            for (int i = 0; i < openList.Count; i++)
            {
                if (gameControllerScript.GetArea() > thresholdForSearchMethod)
                {
                    if (tempScript.GetDistance() >= openList[i].GetDistance() || tempScript.GetHValue() >= openList[i].GetHValue())
                    { tempScript = openList[i]; }
                }
                else
                {
                    if (tempScript.GetDistance() >= openList[i].GetDistance() && tempScript.GetHValue() >= openList[i].GetHValue())
                    { tempScript = openList[i]; }
                }
                if (openList[i] == gameControllerScript.GetFinish().GetComponent<TileController>())
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
    //A method to calculate the G and H value of a tile when the computer checks it
    void CalculateTile(TileController target)
    {
        if(target.GetGValue() > steps || target.GetGValue() == 0)
        {target.SetGValue(steps);}
        target.CalculateH();
    }
    //A getter
    public List<TileController> GetClosedList()
    { return closedList; }
}
