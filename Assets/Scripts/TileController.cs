using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    #region Bools
    bool passable;
    bool hasBeenPulsedH = false;
    bool hasBeenPulsedG = false;
    #endregion
    #region Ints
    int distanceToGoalH = 0;
    int stepsTakenG = 0;
    int tileType = 0;
    #endregion
    GameController gc;
    List<TileController> connectedBlocks = new List<TileController>();
    //Assigns all variables, changes the color of the tile and randomises whether the tile is a wall or not
    void Awake()
    {
        try
        {
            gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            tileType += Random.Range(1, gc.GetWallChance());
            GetComponent<MeshRenderer>().material.color = Color.grey;
            passable = true;
            if (tileType == 1) { passable = false; gameObject.tag = "Wall"; }
            else { gameObject.tag = "Floor"; }
        }
        catch
        {
            print("Error: Something went wrong with the computer, possible errors: Game Controller does not have the necessary tag(GameController), Gamecontroller does not have the necessary script");
            Destroy(this);
        }
    }
    //When the computer checks the tiles around it it calls the calculate function which estimates the distance from that tile to the goal
    public void CalculateH()
    {
        distanceToGoalH = Mathf.RoundToInt(Mathf.Max(gc.GetFinish().transform.position.y - 0.5f, transform.position.y - 0.5f) - Mathf.Min(gc.GetFinish().transform.position.y - 0.5f, transform.position.y - 0.5f));
        distanceToGoalH += Mathf.RoundToInt(Mathf.Max(gc.GetFinish().transform.position.x - 0.5f, transform.position.x - 0.5f) - Mathf.Min(gc.GetFinish().transform.position.x - 0.5f, transform.position.x - 0.5f));
    }
    //The pulses are sent when the computer has found the finishpoint and makes sure that all values are the correct ones
    public void SendHValuePulse()
    {
        ComputerController cc;
        try
        {cc = GameObject.FindGameObjectWithTag("Player").GetComponent<ComputerController>();}
        catch
        {
            cc = new ComputerController();
            print("Error: Computer not found");
            Destroy(this);
        }
        for (int i = 0; i < connectedBlocks.Count; i++)
        {
            if (cc.GetClosedList().Contains(connectedBlocks[i]) && !connectedBlocks[i].GetPulseHStatus())
            {
                connectedBlocks[i].SetHValue(distanceToGoalH + 1);
                connectedBlocks[i].SetPulsedHStatusTrue();
                gc.AddHValueCalcOrder(connectedBlocks[i]);
            }
        }
    }
    public void SendGValuePulse()
    {
        hasBeenPulsedG = true;
        ComputerController cc;
        try
        { cc = GameObject.FindGameObjectWithTag("Player").GetComponent<ComputerController>(); }
        catch {
            cc = new ComputerController();
            print("Error: Computer not found");
            Destroy(this);
        }
        for (int i = 0; i < connectedBlocks.Count; i++)
        {
            if (cc.GetClosedList().Contains(connectedBlocks[i]) && !connectedBlocks[i].GetPulseGStatus())
            {
                connectedBlocks[i].SetGValue(stepsTakenG + 1);
                connectedBlocks[i].SetPulsedGStatusTrue();
                gc.AddGValueCalcOrder(connectedBlocks[i]);
            }
        }
    }
    //Getters and Setters (i should be using properties i know but i realised that after i made all the getters and setters)
    #region Getters and Setters
    public bool Passable { get { return passable; } set { passable = value; } }
    public void SetGValue(int i)
    { stepsTakenG = i; }
    public void SetHValue(int value)
    { distanceToGoalH = value; }
    public int GetGValue()
    { return stepsTakenG; }
    public int GetHValue()
    { return distanceToGoalH; }
    public int GetDistance()
    { return stepsTakenG + distanceToGoalH; }
    public bool GetPulseHStatus()
    { return hasBeenPulsedH; }
    public bool GetPulseGStatus()
    { return hasBeenPulsedG; }
    public void SetPulsedHStatusTrue()
    { hasBeenPulsedH = true; }
    public void SetPulsedGStatusTrue()
    { hasBeenPulsedG = true; }
    public int GetTileType()
    { return tileType; }
    public List<TileController> GetConnectedBlocks()
    { return connectedBlocks; }
    public void AddConnectedBlock(TileController target)
    { if (target.passable){connectedBlocks.Add(target);} }
    #endregion
}
