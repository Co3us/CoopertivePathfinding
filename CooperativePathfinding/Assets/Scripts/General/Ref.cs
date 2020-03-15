/*
 * Ref.cs
 * RefrencePoint for commonly used refrences 
 */

using System.Collections.Generic;
using UnityEngine;

public static class Ref
{
    static GameObject GridGO;
    static Grid grid;
    static List<GameObject> ObstaclesGO= new List<GameObject>();
    static GameObject ManagerGO;
    static GameObject activeAgent;
    static GameObject Plane;
    // Start is called before the first frame update
    public static void init()
    {
        GridGO = GameObject.Find("Grid");
        grid = GridGO.GetComponent<Grid>();
        ManagerGO = GameObject.Find("Manager");
        Plane= GameObject.Find("Plane");
        foreach (GameObject obst in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            ObstaclesGO.Add(obst);
        }
    }

    public static GameObject getGridGO()
    {
        return GridGO;
    }
    public static Grid getGridScript()
    {
        return grid;
    }
    public static GameObject getManagerGO()
    {
        return ManagerGO;
    }
    public static List<GameObject> getObstacles()
    {
        return ObstaclesGO;
    }
    public static void setActiveAgent(GameObject agent)
    {
        activeAgent = agent;
    }
    public static GameObject getActiveAgent()
    {
        return activeAgent;
    }
    public static GameObject getPlane()
    {
        return Plane;
    }
}
