/*
 * FollowPath.cs
 * logic for following a path
 * Adopted from:https://github.com/SebLague/Pathfinding
 */
using UnityEngine;
using System.Collections.Generic;

public class FollowPath : MonoBehaviour
{
    float speed = 0.5f;
    public List<Node> path = null;
    List<Node> newPath;
    int targetIndex;
    float waitTime = 1;
    float prevTime;
    Node startNode;
    public int agentIndexInlist;
    [HideInInspector]
    public int PathIndex { get; set; }
    public bool IsLastCycle { get; set; }
    public bool isWaiting { get; set; }
    bool newPathPrimed;
    Grid gridScript;
    float timeSinceNewPathSelected;
    float timeSinceNewNodeSelected;
    int pathHalfPointIndex;
    FindPath findPathScript;
    public bool isLocked;
    bool shouldGetNewPathChunk;
    int minChainLen;
    Node nextNodeTobeVisited;

    Vector3 prevPos;
    Manager managerScript;
    public Transform target;
    int lastFrame = 0;

    public void init()
    {
        managerScript = GameObject.Find("Manager").GetComponent<Manager>();

        speed = GetComponent<AgentProps>().getSpeed();
        findPathScript = GetComponent<FindPath>();
        gridScript = GameObject.Find("Grid").GetComponent<Grid>();
        waitTime = managerScript.timeResolution;
        startNode = gridScript.NodeFromWorldPoint(transform.position);
        prevPos = transform.position;
        pathHalfPointIndex = (managerScript.getActiveAgent().GetComponent<AgentProps>().cooperationLenght * 2) / 2;
    }

    private void Update()
    {
        if (transform.position == target.position)
        {
            managerScript.stopSimulation();
        }
        if (path != null)
        {

            if (isWaiting == false && PathIndex < path.Count)
            {
                prevTime = Time.time;

                //if node reached move on to next node or attach new path
                if (transform.position == path[PathIndex].worldPosition)
                {
                    if (PathIndex < path.Count - 1)
                    {
                        lastFrame = PathIndex;
                        nextNodeTobeVisited = path[PathIndex + 1];
                    }

                    if (newPathPrimed)
                    {
                        setNewPath(newPath);
                        newPathPrimed = false;
                    }
                    else
                    {
                        PathIndex++;
                        timeSinceNewNodeSelected = 0;


                        if (PathIndex == pathHalfPointIndex)//path.Count - 1)// pathHalfPointIndex)
                        {
                            shouldGetNewPathChunk = true;
                        }

                    }

                }
                if (findPathScript != null && shouldGetNewPathChunk && isLocked == false)
                {
                    findPathScript.requestNewPathChunk(PathIndex, path[path.Count - 1]);
                    shouldGetNewPathChunk = false;
                }

                if (PathIndex < path.Count)
                {
                    if (path[PathIndex].isWaitingNode)
                    {
                        isWaiting = true;
                        return;
                    }

                    transform.position = Vector3.MoveTowards(transform.position, path[PathIndex].worldPosition, speed * Time.deltaTime);
                }

            }
            else if (isWaiting)
            {
                if (Time.time - prevTime > waitTime)
                {
                    isWaiting = false;
                    path[PathIndex].isWaitingNode = false;
                }
            }


            timeSinceNewPathSelected += Time.deltaTime;
            timeSinceNewNodeSelected += Time.deltaTime;

        }
    }
    public void primeNewPath(List<Node> _newPath)
    {
        newPath = _newPath;
        newPathPrimed = true;
    }
    public void setNewPath(List<Node> _path)
    {
        if (_path != null)
        {
            List<Node> nodeList = new List<Node>();
            for (int i = 0; i < _path.Count; i++)
            {
                Node node = _path[i];
                if (true)
                {
                    Node newNode = new Node(node.walkable, node.worldPosition, node.gridX, node.gridY, node.t, node.isWaitingNode);
                    nodeList.Add(newNode);
                }
            }
            path = nodeList;

            if (gridScript != null)
                gridScript.path = _path;

            if (findPathScript != null)
                findPathScript.setPath(_path);

            PathIndex = 0;
            // pathHalfPointIndex = path.Count / 2;
            timeSinceNewPathSelected = 0;
            lastFrame = 0;
        }
    }
    public float getTimeSinceNewNodeSelected()
    {
        return timeSinceNewNodeSelected;
    }
    public void setMinChainLen(int _minChainLen)
    {
        minChainLen = _minChainLen;
    }
    public Node getNextNodeToBeVisited()
    {
        //return lastVisitedNode;
        return nextNodeTobeVisited;
    }
    public int getCurrentTimeFrame()
    {
        //return (int)timeSinceNewPathSelected;
        return lastFrame;
    }
    public void resetLastFrame()
    {
        lastFrame = 0;
    }
}
