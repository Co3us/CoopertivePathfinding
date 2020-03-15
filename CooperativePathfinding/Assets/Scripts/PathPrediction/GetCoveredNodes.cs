/*
 * GetCoveredNodes.cs
 * get all nodes that are inside a collider
 */
using System.Collections.Generic;
using UnityEngine;

public class GetCoveredNodes : MonoBehaviour
{
    //ref 
    public GameObject predictedPathBox;
    FollowPath followPathScript;
    Grid gridScript;
    Manager managerScript;
    BoxCollider predictedPathBoxCollider;
    BoxCollider pathBounds;
    ObstManager obstManagerScript;

    //num of time frames to predict ahead;
    float predictAheadTimeFrames;

    //used to skip a frame for each box position reset
    int updateFrameCounter = 0;

    //box will move as long as this is true
    bool moveBoxAlongPath;

    //all the covered nodes for each time step
    List<List<Node>> nodeGroups = new List<List<Node>>();

    //set inital position of box back
    float amountOfTimeToMoveForward;

    //used when obstacle isn't moving
    bool getBoxesInOnePlace;

    bool isFirst = false;
    int timeFrameCounter = 0;
    float speed;
    int indexObstList = 0;
    Vector3 predictedPoint;
    float prevTime = 0;
    bool isLocked;
    float predictedPathBoxXScale;

    public void init()
    {
        //set ref
        managerScript = Ref.getManagerGO().GetComponent<Manager>();
        predictedPathBoxCollider = predictedPathBox.GetComponent<BoxCollider>();
        gridScript = Ref.getGridScript();
        pathBounds = predictedPathBox.GetComponent<BoxCollider>();
        obstManagerScript = GetComponent<ObstManager>();
        followPathScript = managerScript.getActiveAgent().GetComponent<FollowPath>();

        predictedPathBoxXScale = predictedPathBox.transform.localScale.x;      
        indexObstList = obstManagerScript.getIndex();
    }

    void Update()
    {
        if (moveBoxAlongPath)
        {
            if (updateFrameCounter > 0)
            {
                //get covered points at specific frames
                if (timeFrameCounter == 0)
                {
                    UpdateBox(predictedPoint, 1);
                    timeFrameCounter++;
                }
                else
                {
                    if (getBoxesInOnePlace)
                    {
                        for (int i = 0; i < predictAheadTimeFrames; i++)
                            nodeGroups.Add(gridScript.getNodesInsideBox(pathBounds, i));

                        updateGraph(indexObstList, nodeGroups, true);
                        return;
                    }

                    nodeGroups.Add(gridScript.getNodesInsideBox(pathBounds, timeFrameCounter - 1));
                    timeFrameCounter++;

                    if (timeFrameCounter > predictAheadTimeFrames)
                    {
                        updateGraph(indexObstList, nodeGroups, false);
                        return;
                    }

                    UpdateBox(predictedPoint, timeFrameCounter);
                }
                prevTime = Time.time;

                //this is done so that every other frame gets skipped 
                updateFrameCounter = 0;
            }
            
            else
            {
                updateFrameCounter++;
            }
        }
    }

    public void startMoveBox(float _speed, Vector3 _predictedPoint, bool _getBoxesInOnePlace)
    {
        speed = _speed;
        predictedPoint = _predictedPoint;
        getBoxesInOnePlace = _getBoxesInOnePlace;

        predictedPathBoxCollider.enabled = true;
        moveBoxAlongPath = true;
        amountOfTimeToMoveForward = managerScript.timeResolution- followPathScript.getTimeSinceNewNodeSelected();
        
        timeFrameCounter = 0;
        prevTime = 0;
        updateFrameCounter = 1;

        nodeGroups.Clear();
    }

    void UpdateBox(Vector3 pointInFuturePredicted, int timeFrame)
    {
        float distanceInOneFrame = speed * managerScript.timeResolution;
        float distanceToMoveForward = speed * amountOfTimeToMoveForward;
        Vector3 pathVec = (pointInFuturePredicted - transform.position);

        predictedPathBox.transform.position = transform.position + pathVec.normalized * distanceInOneFrame
            * timeFrame - pathVec.normalized * distanceInOneFrame / 2 + pathVec.normalized * distanceToMoveForward;
    }
    public void updateGraph(int indexObstList, List<List<Node>>nodeGroups, bool isStandingStill)
    {
        gridScript.updateGraph(indexObstList, nodeGroups, isStandingStill);
        followPathScript.isLocked = false;
        moveBoxAlongPath = false;
    } 
    public void setIndex(int _index)
    {
        indexObstList = _index;
    }
    public void setPredictAheadTime(float _predictAheadTimeFrames)
    {
        predictAheadTimeFrames = _predictAheadTimeFrames;
    }
    public List<List<Node>> getNodeGroups()
    {
        return nodeGroups;
    }
    public GameObject getPredictBox()
    {
        return predictedPathBox;
    }
    public void setLocked(bool _isLocked)
    {
        isLocked = _isLocked;
    }

}
