/*
 * Node.cs
 * represents a node or cell in a grid.
 * Adopted from:https://github.com/SebLague/Pathfinding
 */
using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node>
{
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int t;
    public int gCost;
    public int hCost;
 
    public bool walkable;
    public bool isWaitingNode;
    public bool wasPartOfAgentPath;
    public Node parent;
    
    public int posInChain=1;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _t, bool _isWaitingNode=false)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        t = _t;
        isWaitingNode = _isWaitingNode;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex { get; set; }

    public int CompareTo(Node nodeToCompare)
    {
        //returns 0 if same, -1 if nodeToCompare lower, 1 if compare node higher
        int compare = fCost.CompareTo(nodeToCompare.fCost);

        //if fcost the same we take one with lower h cost
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return compare;
    }
}
