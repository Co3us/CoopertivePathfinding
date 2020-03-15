/*
 * Pathfinding.cs
 * finds a shortest between two nodes
 * Adopted from:https://github.com/SebLague/Pathfinding
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpatialAStar : MonoBehaviour
{
    List<Node> path;

    //refrences
    Grid gridScript;
    FollowPath followPathScript;

    float speed = 2;
    bool searchPath = true;
    int pathIndex = 0;
   
    Heap<Node> openSet;
    HashSet<Node> closedSet;
    Node startNode;
    Node targetNode;
    Vector3 startPos;
    Vector3 targetPos;
    Manager managerScript;
    Node[,] grid;

    public void init(Node[,]_grid)
    {
        path = new List<Node>();
        Grid gridScriptRef = GameObject.Find("Grid").GetComponent<Grid>();
        gridScript = gridScriptRef;

        grid = new Node[gridScript.GridSizeX, gridScript.GridSizeY];
        for (int i = 0; i < gridScript.GridSizeX; i++)
        {
            for (int j = 0; j < gridScript.GridSizeY; j++)
            {
                grid[i, j] = new Node(_grid[i, j].walkable, _grid[i,j].worldPosition, i, j, 0);
            }
        }

        GameObject managerGO = GameObject.Find("Manager");
     
        managerScript = managerGO.GetComponent<Manager>();
        openSet = new Heap<Node>(gridScript.MaxSize);
        closedSet = new HashSet<Node>();
    }

    //A* algorithm
    public List<Node> FindPathSpatial(Vector3 _startPos, Vector3 _targetPos, bool initialSearch, Node stopNode)
    {

        int nodesVisited = 0;
        if (initialSearch)
        {
            startPos = _startPos;
            targetPos = _targetPos;
            startNode = gridScript.NodeFromWorldPoint(_startPos);
            targetNode = gridScript.NodeFromWorldPoint(_targetPos);
            openSet = new Heap<Node>(gridScript.MaxSize);
            closedSet = new HashSet<Node>();

            startNode = grid[startNode.gridX, startNode.gridY];
            targetNode = grid[targetNode.gridX, targetNode.gridY];
            openSet.Add(startNode);
        }


        while (openSet.Count > 0)
        {
            nodesVisited++;
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);
         
            foreach (Node neighbour in GetNeighboursSpatial(currentNode))
            {
                if (closedSet.Contains(neighbour) || neighbour.walkable == false)
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
                
            }
            if (initialSearch)
            {
                if (currentNode.gridX == targetNode.gridX && currentNode.gridY == targetNode.gridY)
                {
                    managerScript.addToSpatialScore(nodesVisited);
                    return RetracePath(startNode, currentNode) ;
                }
            }
            else
            {
                if (closedSet.Contains(stopNode))
                {
                    managerScript.addToSpatialScore(nodesVisited);
                    return null;
                }
            }

            ;
        }
        managerScript.addToSpatialScore(nodesVisited);
        return null;
    }
    public int getGCost(int gridX, int gridY)
    {
        Node node = grid[gridX, gridY];

        if (node.walkable == false)
        {
            return int.MaxValue/2;
        }
        else if (closedSet.Contains(node))
        {
            return node.gCost;
        }
        else
        {
            FindPathSpatial(startPos, targetPos, false, node);
            return node.gCost;
        }

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
        path.Reverse();

        return path;
    }
    //private Node.UnwalkableDirection getDirection(Node fromNode, Node toNode)
    //{
    //    int x1 = fromNode.gridX;
    //    int y1 = fromNode.gridY;
    //    int x2 = toNode.gridX;
    //    int y2 = toNode.gridY;
    //    if (x1 == x2)
    //    {
    //        if (y1 == y2)
    //        {
    //            return Node.UnwalkableDirection.ALL;
    //        }
    //        else if (y1 < y2)
    //        {
    //            return Node.UnwalkableDirection.UP;
    //        }
    //        else
    //        {
    //            return Node.UnwalkableDirection.DOWN;
    //        }
    //    }
    //    else
    //    {
    //        if (x1 > x2)
    //        {
    //            return Node.UnwalkableDirection.LEFT;
    //        }
    //        else if (x1 < x2)
    //        {
    //            return Node.UnwalkableDirection.RIGHT;
    //        }
    //    }
    //    return Node.UnwalkableDirection.ALL;
    //}
    //private void setNodeUnwalkableDirection(Node prevNode, Node currentNode)
    //{
    //    int x1 = prevNode.gridX;
    //    int y1 = prevNode.gridY;
    //    int x2 = currentNode.gridX;
    //    int y2 = currentNode.gridY;
    //    if (x1 == x2)
    //    {
    //        if (y1 == y2)
    //        {
    //            prevNode.unwalkableDirection = Node.UnwalkableDirection.ALL;
    //        }
    //        else if (y1 < y2)
    //        {
    //            prevNode.unwalkableDirection = Node.UnwalkableDirection.UP;
    //        }
    //        else
    //        {
    //            prevNode.unwalkableDirection = Node.UnwalkableDirection.DOWN;
    //        }
    //    }
    //    else
    //    {
    //        if (x1 > x2)
    //        {
    //            prevNode.unwalkableDirection = Node.UnwalkableDirection.LEFT;
    //        }
    //        else if (x1 < x2)
    //        {
    //            prevNode.unwalkableDirection = Node.UnwalkableDirection.RIGHT;
    //        }
    //    }
    //}

    List<Node> GetNeighboursSpatial(Node node)
    {
        List<Node> neighbours = new List<Node>();
        int visitedNodes = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if ((x == 0 && y == 0) || (Mathf.Abs(x) == Mathf.Abs(y)))
                    continue;

                visitedNodes++;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridScript.GridSizeX && checkY >= 0 && checkY < gridScript.GridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        //managerScript.addToSpatialScore(visitedNodes);
        return neighbours;
    }

    void printGcosts()
    {
        for (int i = 0; i < gridScript.GridSizeX; i++)
        {
            string lineString = "line " + i+" ";
            for (int j = 0; j < gridScript.GridSizeY; j++)
            {

                lineString += grid[i, j].gCost + "(" + i + "," + j + ")";
            }
            print(lineString);
        }
    }
    //distance between two nodes
    int GetDistance(Node nodeA, Node nodeB)
    {
        //if(nodeB)
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        return dstX + dstY;
        //if (dstX > dstY)
        //    return 14 * dstY + 10 * (dstX - dstY);
        //return 14 * dstX + 10 * (dstY - dstX);
        //if (dstX > dstY)
        //    return 10 * dstY + 10 * (dstX - dstY) + dstT;
        //return 10 * dstX + 10 * (dstY - dstX) + dstT;
    }


}
