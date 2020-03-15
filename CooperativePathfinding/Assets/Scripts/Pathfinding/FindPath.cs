/*
 * FindPath.cs
 * Logic for finding and managing an agent path
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FindPath : MonoBehaviour
{
    //ref 
    public Transform target;
    Transform agent;
    Grid gridScript;
    SpaceTimeAStar spaceTimeAStarScript;
    VisibleObstManager visibleObstManager;
    SpatialAStar spatialAStarScript;
    FollowPath followPathScript;
    Manager managerScript;

    Node[,] grid;
    float prevTime = 0;
    bool initComplete = false;
    float recalculateTime;
    int prevPathCurrentInd;
    Node prevPathEndNode;


    List<Node> path = null;
    List<Node> prevPath = new List<Node>();

    //que for every obstcale that requests recalculation
    Stack<int> recalcQue = new Stack<int>();

    void Start()
    {
        init();
    }

    public void init()
    {
        if (initComplete == false)
        {
            //set ref
            gridScript = Ref.getGridScript();
            visibleObstManager = GetComponent<VisibleObstManager>();
            managerScript = Ref.getManagerGO().GetComponent<Manager>();
            recalculateTime = GetComponent<AgentProps>().cooperationLenght;
            spatialAStarScript = GetComponent<SpatialAStar>();
            spaceTimeAStarScript = GetComponent<SpaceTimeAStar>();
            agent = transform;
            followPathScript = agent.GetComponent<FollowPath>();

            //saves initial grid in grid var
            saveInitalGrid();

            spaceTimeAStarScript.init();
            spatialAStarScript.init(grid);
            spaceTimeAStarScript.setSpacialAStarScript(spatialAStarScript);
            managerScript.increaseFrameCount();

            //here we switch the role of target and agent
            spatialAStarScript.FindPathSpatial(target.position, agent.position, true, null);

            path = spaceTimeAStarScript.FindPath(agent.position, target.position, true);

            followPathScript.init();
            followPathScript.setNewPath(path);
        }
        initComplete = true;
    }

    void Update()
    {

        if (recalcQue.Count != 0)
        {
            recaluclateOnObstaclePathIntercept();
            recalcQue.Pop();
        }

    }

    //triggers when predicted obstacle path crosses current agent path 
    public void recaluclateOnObstaclePathIntercept()
    {
        //get starting node
        Node newStartNode = followPathScript.getNextNodeToBeVisited();
        int debugTime = followPathScript.getCurrentTimeFrame();

        prevPath = path;

        clearPathInGrid();

        //get new path
        if (newStartNode.worldPosition != target.position)
        {
            managerScript.increaseFrameCount();
            List<Node> currentPath = spaceTimeAStarScript.FindPath(newStartNode.worldPosition, target.position, true);
            if (currentPath != null)
            {
                path = currentPath;

                //the path is primed so that it get's swapped out only when an agent reaches the center of his next cell
                followPathScript.primeNewPath(currentPath);

                //this is done so that if there is a second recalc before the path is reset the obstacle path won't be shifted 
                //as it has already been reset by the previous recalc
                followPathScript.resetLastFrame();
            }
            //this means agent got stuck and restart of spatial A* is needed
            else
            {
                restartPath();
                return;
            }
        }
        //mark path
        foreach (Node node in path)
        {
            gridScript.markNode(node, false, true);
        }

        //restart time for getting new chunk
        prevTime = Time.time;

    }

    public void requestNewPathChunk(int _prevPathCurrentInd, Node _prevPathEndNode)
    {

        prevPathCurrentInd = _prevPathCurrentInd;
        prevPathEndNode = _prevPathEndNode;

        //lock path prediction while new chunk is being calculated
        visibleObstManager.lockUnlockAllObst(true);

        visibleObstManager.setReturnSignal(true);
        visibleObstManager.recalcAllObst();

    }
    public void getNewPathChunk()
    {
        if (prevPathCurrentInd >= path.Count)
        {
            return;
        }

        clearPathInGrid();

        managerScript.increaseFrameCount();
        List<Node> currentPath = spaceTimeAStarScript.FindPath(path[prevPathCurrentInd].worldPosition, target.position, false, 0);

        //if agent is stuck
        if (currentPath == null)
        {
            restartPath();
            return;
        }

        visibleObstManager.lockUnlockAllObst(false);

        followPathScript.primeNewPath(currentPath);
    }
    void clearPathInGrid()
    {
        foreach (Node node in path)
        {
            if (node.wasPartOfAgentPath)
            {
                gridScript.markNode(node, true, true);
            }
        }
    }
    void restartPath()
    {
        saveInitalGrid();
        spatialAStarScript.init(grid);
        managerScript.increaseFrameCount();
        //here we switch the role of target and agent
        spatialAStarScript.FindPathSpatial(target.position, agent.position, true, null);

        path = spaceTimeAStarScript.FindPath(agent.position, target.position, true);
        followPathScript = agent.GetComponent<FollowPath>();
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

    public void setPath(List<Node> _path)
    {
        path = _path;

        foreach (Node node in prevPath)
        {
            if (node.wasPartOfAgentPath)
            {
                gridScript.markNode(node, true, true, true);
            }
        }

        //mark new path
        foreach (Node node in path)
        {
            gridScript.markNode(node, false, true);
        }

        //when the path is set we can unlock obstacle predictions
        visibleObstManager.lockUnlockAllObst(false);
    }
    public void addToRecalcQueue(int obstIndex)
    {
        recalcQue.Push(obstIndex);
    }

}
