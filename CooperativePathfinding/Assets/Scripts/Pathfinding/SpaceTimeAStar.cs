/*
 * SpaceTimeAStar.cs
 * finding shortest path in 3D grid : (x,y,t) - where (x,y) is location and t is time
 */
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class SpaceTimeAStar : MonoBehaviour
{
    //refrences
    FollowPath followPathScript;
    SpatialAStar spatialAStarScript;
    AgentProps agentPropsScript;
    Manager managerScript;
    Grid gridScript;

    Node[,,] grid;
    List<Node> path;

    int windowLenght = 0;
    int prevMinHCost = int.MaxValue;

    public void init()
    {
        //set ref
        agentPropsScript = GetComponent<AgentProps>();
        GameObject managerGO = Ref.getManagerGO();
        managerScript = managerGO.GetComponent<Manager>();
      
        followPathScript = GetComponent<FollowPath>();
        gridScript = Ref.getGridScript();
        grid = gridScript.getGrid();

        windowLenght = agentPropsScript.cooperationLenght * 2;
        float agentSpeed = agentPropsScript.getSpeed();

    }


    //A* algorithm Space-time
    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos, bool resetPrevMinGCost = false, int startT = 0)
    {
        path = new List<Node>();
        int minHCost = int.MaxValue;

        if (resetPrevMinGCost)
            prevMinHCost = int.MaxValue;

        //set parent property to null for all nodes in graph
        gridScript.clearParents();

        //initialization
        Node startNode = gridScript.NodeFromWorldPoint(startPos, startT);
        Node targetNode = gridScript.NodeFromWorldPoint(targetPos);
        Heap<Node> openSet = new Heap<Node>(gridScript.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        startNode.hCost = GetTrueDistanceToTarget(startNode);
        openSet.Add(startNode);
        int nodeVisited = 0;

        //main loop
        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            nodeVisited++;

            bool pathEndsAtTarget = false;
            if (currentNode.gridX == targetNode.gridX && currentNode.gridY == targetNode.gridY)
                pathEndsAtTarget = true;

            //ending condition
            if (currentNode.t == windowLenght || pathEndsAtTarget)
            {
                managerScript.addToSpaceTimeScore(nodeVisited);

                if (prevMinHCost <= minHCost && minHCost > 0 && !pathEndsAtTarget)
                    return null;

                prevMinHCost = minHCost;
                RetracePath(startNode, currentNode);
                return path;
            }

            // keep track of min h cost in path
            if (currentNode.hCost < minHCost)
            {
                minHCost = currentNode.hCost;
            }

            List<Node> neighbours = gridScript.GetNeighboursSpaceTime(currentNode);

            foreach (Node neighbour in neighbours)
            {
                if (closedSet.Contains(neighbour))
                    continue;
                if (neighbour.walkable == false)
                    continue;

                //all directions have a move g cost of 1
                int movementCostToNeighbour = currentNode.gCost + 1;

                if (movementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = movementCostToNeighbour;
                    neighbour.hCost = GetTrueDistanceToTarget(neighbour);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
        return null;
    }
    List<Node> RetracePath(Node startNode, Node endNode)
    {
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);

        //set waiting to node in path with repeating positions
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (path[i].gridX == path[i + 1].gridX && path[i].gridY == path[i + 1].gridY)
                path[i].isWaitingNode = true;
        }

        path.Reverse();

        return path;
    }

    int GetManhattaneBetweenNodes(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        int dstT = Mathf.Abs(nodeA.t - nodeB.t);

        return dstX + dstY + dstT;
    }
    int GetTrueDistanceToTarget(Node node)
    {
        return spatialAStarScript.getGCost(node.gridX, node.gridY);
    }

    //getter and setters
    public void setSpacialAStarScript(SpatialAStar _spatialAStarScript)
    {
        spatialAStarScript = _spatialAStarScript;
    }
    public SpatialAStar getSpacialAStarScript()
    {
        return spatialAStarScript;
    }

}
