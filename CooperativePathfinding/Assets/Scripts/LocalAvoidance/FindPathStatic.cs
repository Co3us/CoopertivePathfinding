/*
 * FindPathStatic.cs
 * Logic for findig path with static obstacles used by local avoidance.
 */
using System.Collections.Generic;
using UnityEngine;

public class FindPathStatic : MonoBehaviour
{
    Grid gridScript;

    Node[,] grid;
    float prevTime = 0;

    public Transform target;

    Transform agent;
    AgentProps agentPropsScript;
    List<Node> coveredNodes=new List<Node>();
    List<Node> path = null;
    FollowPath followPathScript;
    SpatialAStar spatialAStarScript;
    VisibleObstManager visibleObstManagerScript;
    Manager managerScript;

    void Start()
    {
        managerScript = Ref.getManagerGO().GetComponent<Manager>();
        gridScript = Ref.getGridScript();
        visibleObstManagerScript=GetComponent<VisibleObstManager>();
      
        agentPropsScript = GetComponent<AgentProps>();
        saveInitalGrid();
        agent = transform;

        spatialAStarScript = GetComponent<SpatialAStar>();
        spatialAStarScript.init(grid);
        managerScript.increaseFrameCount();

        path = spatialAStarScript.FindPathSpatial(agent.position, target.position, true, null);

        followPathScript = agent.GetComponent<FollowPath>();
        followPathScript.init();
        followPathScript.setNewPath(path);


    }
    void saveInitalGrid()
    {
        Node[,,] mainGrid = gridScript.getGrid();
        grid = new Node[gridScript.GridSizeX, gridScript.GridSizeY];
        for (int i = 0; i < gridScript.GridSizeX; i++)
        {
            for (int j = 0; j < gridScript.GridSizeY; j++)
            {
                grid[i, j] = new Node(mainGrid[i, j, 0].walkable, mainGrid[i, j, 0].worldPosition, i, j, 0);
            }
        }
    }
    public void restartPath()
    {
        Node[,,] gridGlobal = gridScript.getGrid();
        foreach (Node node in coveredNodes)
        {
            gridScript.markNode(gridGlobal[node.gridX, node.gridY, 0],true);
        }
        coveredNodes = visibleObstManagerScript.getAllObstCoveredNodes();
        foreach(Node node in coveredNodes)
        {
            gridScript.getGrid()[node.gridX, node.gridY, 0].walkable = false;
        }
        saveInitalGrid();
        spatialAStarScript.init(grid);
        managerScript.increaseFrameCount();

        path = spatialAStarScript.FindPathSpatial(agent.position, target.position, true, null);

        followPathScript = agent.GetComponent<FollowPath>();
        followPathScript.setNewPath(path);

    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        if (path != null)
            foreach (Node n in path)
            {
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (1 - .05f));
            }
    }
}
