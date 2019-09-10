using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorScript : MonoBehaviour
{
    //Bools
    #region
    bool passable;
    bool hasBeenPulsedH = false;
    bool hasBeenPulsedG = false;
    #endregion
    //Ints
    #region
    int distanceToGoalH = 0;
    int stepsTakenG = 0;
    int tileType = 0;
    #endregion
    GameController gc;
    List<FloorScript> connectedBlocks = new List<FloorScript>();
    void Awake()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        tileType += Random.Range(1, gc.GetWallChance());
        GetComponent<MeshRenderer>().material.color = Color.grey;
        passable = true;
        if(tileType == 1) { passable = false; gameObject.tag = "Wall"; }
    }
    public void CalculateH()
    {
        distanceToGoalH = Mathf.RoundToInt(Mathf.Max(gc.GetFinish().transform.position.y - 0.5f, transform.position.y - 0.5f) - Mathf.Min(gc.GetFinish().transform.position.y - 0.5f, transform.position.y - 0.5f));
        distanceToGoalH += Mathf.RoundToInt(Mathf.Max(gc.GetFinish().transform.position.x - 0.5f, transform.position.x - 0.5f) - Mathf.Min(gc.GetFinish().transform.position.x - 0.5f, transform.position.x - 0.5f));
    }
    public void SendHValuePulse()
    {
        ComputerController cc = GameObject.FindGameObjectWithTag("Player").GetComponent<ComputerController>();
        for (int i = 0; i < connectedBlocks.Count; i++)
        {
            if (cc.GetClosedList().Contains(connectedBlocks[i]) && !connectedBlocks[i].GetPulseH())
            {
                connectedBlocks[i].SetHValue(distanceToGoalH + 1);
                connectedBlocks[i].SetPulseH();
                gc.AddHValueCalcOrder(connectedBlocks[i]);
            }
        }
    }
    public void SendGValuePulse()
    {
        hasBeenPulsedG = true;
        ComputerController cc = GameObject.FindGameObjectWithTag("Player").GetComponent<ComputerController>();
        for (int i = 0; i < connectedBlocks.Count; i++)
        {
            if (cc.GetClosedList().Contains(connectedBlocks[i]) && !connectedBlocks[i].GetPulseG())
            {
                connectedBlocks[i].SetGValue(stepsTakenG + 1);
                connectedBlocks[i].SetPulseG();
                gc.AddGValueCalcOrder(connectedBlocks[i]);
            }
        }
    }
    //Getters and Setters
    #region
    public void SetGValue(int i)
    { stepsTakenG = i; }
    public void SetHValue(int value)
    {distanceToGoalH = value; }
    public int GetGValue()
    { return stepsTakenG;}
    public int GetHValue()
    { return distanceToGoalH; }
    public int GetDistance()
    {return stepsTakenG + distanceToGoalH;}
    public bool GetPassabilityStatus()
    {return passable; }
    public void Setpassability(bool a)
    {passable = a; }
    public bool GetPulseH()
    {return hasBeenPulsedH;}
    public bool GetPulseG()
    { return hasBeenPulsedG; }
    public void SetPulseH()
    { hasBeenPulsedH = true; }
    public void SetPulseG()
    { hasBeenPulsedG = true; ; }
    public int GetTileType()
    {return tileType; }
    public List<FloorScript> GetConnectedBlocks()
    { return connectedBlocks; }
    public void AddConnectedBlock(FloorScript target)
    { if (target.GetPassabilityStatus()) { connectedBlocks.Add(target); } }
    #endregion
}
