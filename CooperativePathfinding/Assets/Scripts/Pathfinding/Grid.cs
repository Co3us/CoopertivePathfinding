/*
 * Grid.cs
 * Split map into cells, grid helper functions and gizmo drawing 
 * Adopted from:https://github.com/SebLague/Pathfinding
 */
using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    //ref
    public AgentProps agentPropsScript;
    public FindPath findPathScript;
    public FollowPath followPathScript;
   // public GameObject predictedPath;
    Manager managerScript;
    BoxCollider pathBounds;
    VisibleObstManager visObstManagerScript;

    //grid settings
    public TextAsset levelTxt;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public int GridSizeX { get; set; }
    public int GridSizeY { get; set; }
    public int GridSizeZ { get; set; }

    string[] lines;
    Node[,,] grid;
    Node[,,] gridDraw;
    float nodeDiameter;
    int timeFrameOffset;

    Vector3 worldBottomLeft;

    //At every time step we have a list of nodes the obstacle is occupying and we have this collection for each obstacle
    List<List<List<Node>>> obstPaths = new List<List<List<Node>>>();

    //flag to check if the predicted path clashes with the planned agent path on any node
    bool pathCrossesAgentPath;

    public void init()
    {
        //set ref
        GameObject agent = Ref.getActiveAgent();
        GameObject managerGO = Ref.getManagerGO();
        GameObject plane = Ref.getPlane();
        managerScript = managerGO.GetComponent<Manager>();
        findPathScript = agent.GetComponent<FindPath>();
        followPathScript = agent.GetComponent<FollowPath>();
        agentPropsScript = agent.GetComponent<AgentProps>();
        
        visObstManagerScript = agent.GetComponent<VisibleObstManager>();

        nodeDiameter = nodeRadius * 2;

        if (levelTxt != null)
        {
            lines = levelTxt.text.Split('\n');
            string firstLine = lines[0];
            gridWorldSize.x = int.Parse(firstLine.Split(' ')[0]);
            gridWorldSize.y = int.Parse(firstLine.Split(' ')[1]);

        }
        GridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        GridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        GridSizeZ = agentPropsScript.cooperationLenght * 2 + 1;

        plane.transform.localScale = new Vector3(GridSizeX / 10f, 1, GridSizeY / 10f);
        worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        CreateGrid();
    }

    public void CreateGrid()
    {
        grid = new Node[GridSizeX, GridSizeY, GridSizeZ];
        gridDraw= new Node[GridSizeX, GridSizeY, GridSizeZ];
        //check if any obstacles collide with cells
        for (int k = 0; k < GridSizeZ; k++)
        {
            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                    grid[x, y, k] = new Node(walkable, worldPoint, x, y, k);
                    gridDraw[x,y,k]= new Node(true, worldPoint, x, y, k);
                }
            }
        }

        //mark cells as unwalkable according to the saved level
        if (levelTxt != null)
        {
            for (int i = 1; i < lines.Length; i++)
            {
                int x = 0;
                if (int.TryParse(lines[i].Split(' ')[0], out x))
                {
                    int y = int.Parse(lines[i].Split(' ')[1]);
                    gridDraw[x, y, 0].walkable = false;
                    for (int k = 0; k < GridSizeZ; k++)
                    {
                        grid[x, y, k].walkable = false;
                    }
                }
            }
        }

    }

    public List<Node> GetNeighboursSpaceTime(Node node)
    {
        List<Node> neighbours = new List<Node>();

        if (node.t > GridSizeZ - 1)
            return neighbours;

        neighbours = get4Neighbours(node, 1);

        // pause neighbour
        neighbours.Add(grid[node.gridX, node.gridY, node.t + 1]);

        return neighbours;
    }

    public List<Node> GetNeighbours2D(Node node)
    {
        List<Node> neighbours = new List<Node>();

        if (node.t > GridSizeZ - 1)
            return neighbours;

        return get4Neighbours(node, 0);
    }

    public List<Node> get4Neighbours(Node node, int timeShift)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if ((Mathf.Abs(x) == Mathf.Abs(y)))
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;


                if (checkX >= 0 && checkX < GridSizeX && checkY >= 0 && checkY < GridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY, node.t + timeShift]);
                }
            }
        }
        return neighbours;
    }

    public void clearParents()
    {
        for (int k = 0; k < GridSizeZ; k++)
        {
            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    grid[x, y, k].parent = null;
                }
            }
        }
    }

    public List<Node> getNodesInsideBox(BoxCollider box, int timeFrame)
    {
        List<Node> nodes = new List<Node>();
        Vector2 centerInGridCoord = new Vector2((GridSizeX * nodeDiameter) / 2, (GridSizeY * nodeDiameter) / 2);

        int indexXMin = (int)((box.bounds.min.x + centerInGridCoord.x) / nodeDiameter);
        int indexXMax = Mathf.CeilToInt((box.bounds.max.x + centerInGridCoord.x) / nodeDiameter);
        int indexYMin = (int)((box.bounds.min.z + centerInGridCoord.y) / nodeDiameter);
        int indexYMax = Mathf.CeilToInt((box.bounds.max.z + centerInGridCoord.y) / nodeDiameter);

        for (int i = indexXMin; i <= indexXMax; i++)
        {
            for (int j = indexYMin; j <= indexYMax; j++)
            {
                if (i >= 0 & j >= 0 & i < GridSizeX && j < GridSizeY)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (i * nodeDiameter + nodeRadius) + Vector3.forward * (j * nodeDiameter + nodeRadius);

                    if ((Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask)))
                        nodes.Add(grid[i, j, timeFrame]);
                }
            }
        }

        return nodes;
    }

    //update the graph by replacing the used nodes for obstacle at index with nodes stored in nodeGroups
    public void updateGraph(int index, List<List<Node>> nodeGroups, bool isStandingStill)
    {
        pathCrossesAgentPath = false;

        timeFrameOffset = followPathScript.getCurrentTimeFrame();

        //if obstacle is not moving we want to fill it's current position in all time frames
        //we don't shift back to prevent loss of nodes at the end where they overflow.
        if (isStandingStill)
            timeFrameOffset = 0;

        //if path for index exist
        if (obstPaths.Count > index)
        {
            cleanObstPath(index, false);
            obstPaths[index] = new List<List<Node>>();

            foreach (List<Node> nodeGroup in nodeGroups)
            {

                List<Node[]> nodeList = getNodeListAndMarkNodes(nodeGroup);

                List<Node> beforeNodes = new List<Node>();
                List<Node> currentNodes = new List<Node>();
                List<Node> afterNodes = new List<Node>();

                foreach (Node[] nodeBeforeAfterCurrent in nodeList)
                {
                    beforeNodes.Add(nodeBeforeAfterCurrent[0]);
                    currentNodes.Add(nodeBeforeAfterCurrent[1]);
                    afterNodes.Add(nodeBeforeAfterCurrent[2]);
                }
                beforeNodes.RemoveAll(item => item == null);
                currentNodes.RemoveAll(item => item == null);
                afterNodes.RemoveAll(item => item == null);

                //in first iteration we add lists for current, after and before(if it exists)
                //in second iteration the current nodes get added to the previous after list
                //and before nodes get added to the previous current list
                if (obstPaths[index].Count == 0)
                {
                    if (beforeNodes.Count > 0)
                        obstPaths[index].Add(beforeNodes);

                    obstPaths[index].Add(currentNodes);
                }
                else
                {
                    foreach (Node node in beforeNodes)
                    {
                        obstPaths[index][obstPaths[index].Count - 2].Add(node);
                    }
                    foreach (Node node in currentNodes)
                    {
                        obstPaths[index][obstPaths[index].Count - 1].Add(node);
                    }
                }

                if (obstPaths[index].Count < GridSizeZ)
                    obstPaths[index].Add(afterNodes);
            }
        }

        visObstManagerScript.addToRecalcObstCounter();
        //pathCrossesAgent path is global var that gets set in the markNodeWithOffset method
        //this happens as soon as any collision with the agent path is found
        if (pathCrossesAgentPath)
        {
            findPathScript.addToRecalcQueue(index);
        }
    }

    public void cleanObstPath(int index, bool complete)
    {
        foreach (List<Node> nodeGroup in obstPaths[index])
        {
            foreach (Node node in nodeGroup)
            {
                if (complete)
                    markNode(node, true);
                else if (node.wasPartOfAgentPath == false)
                    markNode(node, true);
            }
        }
    }

    //mark each node and it's same position neighbours in nodeGroup
    List<Node[]> getNodeListAndMarkNodes(List<Node> nodeGroup)
    {
        List<Node[]> nodeList = new List<Node[]>();
        foreach (Node node in nodeGroup)
        {
            Node[] beforeCurrentAfterNodes = new Node[3];
            
            //we need to shift obstacle to current time frame the agent is on
            int agentPositionOffset = node.t + timeFrameOffset;

            beforeCurrentAfterNodes[0] = markNodeWithOffset(node, agentPositionOffset, -1);
            beforeCurrentAfterNodes[1] = markNodeWithOffset(node, agentPositionOffset,0);
            beforeCurrentAfterNodes[2] = markNodeWithOffset(node, agentPositionOffset,  1);
            nodeList.Add(beforeCurrentAfterNodes);

        }

        return nodeList;
    }
    Node markNodeWithOffset(Node node, int agentPositionOffset, int localOffset)
    {
        int shiftedNodeLocalOffset = agentPositionOffset + localOffset;
        int originalNodeLocalOffset = node.t + localOffset;
        if (shiftedNodeLocalOffset >= 0 && shiftedNodeLocalOffset < GridSizeZ)
        {
            Node frameAdjustedNode = grid[node.gridX, node.gridY, shiftedNodeLocalOffset];

            //if the node is already marked as unwalkable by another obstcle we can return
            if (frameAdjustedNode.walkable == false && frameAdjustedNode.wasPartOfAgentPath == false)
                return null;

            if (frameAdjustedNode.wasPartOfAgentPath)
            {
                pathCrossesAgentPath = true;
                node.wasPartOfAgentPath = false;
            }
        }

        if(originalNodeLocalOffset >= 0 && originalNodeLocalOffset < GridSizeZ) {

            Node normalNode = grid[node.gridX, node.gridY, originalNodeLocalOffset];
            markNode(normalNode, false);
            return normalNode;

            //markNode(frameAdjustedNode, false);
            //return frameAdjustedNode;
        }
        return null;

    }

    public void markNode(Node node, bool walkable, bool isAgent = false, bool preserveWaiting = false)
    {
        node.walkable = walkable;
  

        if (isAgent)
            node.wasPartOfAgentPath = !walkable;
        else
            node.wasPartOfAgentPath = false;

        if (walkable && preserveWaiting == false)
        {
            node.isWaitingNode = false;
        }
    }

    public void shiftAllObstNodes(int index, int shiftAmount)
    {
        if (obstPaths.Count != 0)
        {
            List<List<Node>> nodeGroups = obstPaths[index];
            for (int i = 0; i < nodeGroups.Count; i++)
            {
                for (int j = 0; j < nodeGroups[i].Count; j++)
                {
                    Node newNode = shiftNode(nodeGroups[i][j], shiftAmount);

                    if (newNode != null)
                        nodeGroups[i][j] = newNode;

                }
            }

        }
    }

    Node shiftNode(Node node, int shiftAmount)
    {
        markNode(node, true);
        Node newNode = null;
        if (node.t - shiftAmount >= 0)
        {
            newNode = grid[node.gridX, node.gridY, node.t - shiftAmount];
            markNode(newNode, false);
        }
        return newNode;

    }
    
    public void fillTillEnd(int index)
    {
        List<Node> lastTimeFrameNodes = obstPaths[index][obstPaths[index].Count - 1];
        foreach (Node node in lastTimeFrameNodes)
        {
            for (int i = node.t + 1; i < GridSizeZ; i++)
            {
                Node newNode = grid[node.gridX, node.gridY, i];
                markNode(newNode, false);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition, int startT = 0)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((GridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((GridSizeY - 1) * percentY);
        return grid[x, y, startT];
    }

    public List<Node> path = new List<Node>();
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        if (grid != null)
        {
            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    Node n = gridDraw[x, y, 0];
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .05f));
                }
            }
        }
        Gizmos.color = Color.black;

        foreach (Node n in path)
        {
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .05f));
        }

        //Gizmos.color = Color.green;
        //Matrix4x4 rotationMatrix = Matrix4x4.TRS(predictedPath.transform.position, predictedPath.transform.rotation, new Vector3(1, 1, 1));
        //Gizmos.matrix = rotationMatrix;
        //Gizmos.DrawWireCube(new Vector3(0, 0, 0), predictedPath.transform.lossyScale);

    }
    public void addObstPath()
    {
        obstPaths.Add(new List<List<Node>>());
    }


    int maxSize;
    public int MaxSize
    {
        get
        {
            return GridSizeX * GridSizeY * GridSizeZ;
        }
    }
    public Node[,,] getGrid()
    {
        return grid;
    }
}