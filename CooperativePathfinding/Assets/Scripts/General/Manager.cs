/*
 * Manager.cs
 * asigns initial simulation parameters and logs results.
 * This is the initiation starting point.
 */
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    //contains an instance of local avoidance agent and predictive pathfinding agent
    public GameObject[] agents;

    //simulation parameters
    public bool isLocalAvoidance;
    public float agentSpeed;
    public float timeResolution=1;


    //flag to make sure results are displayed only once
    bool simEnded;

    float time;
    int spatialNodesVisited = 0;
    int spaceTimeNodesVisited = 0;
    int spatialCallCounter;
    int spaceTimeCallCounter;
   // int optimalPathLength;
    GameObject activeAgent;
    int frameCount;
    List<int> nodeCountsPerFrame = new List<int>();

    void Awake()
    {
        //init the references
        Ref.init();

        //set active agent params
        if (isLocalAvoidance)
            activeAgent = setAgentParameters(agents[1], agents[0]);
        else
            activeAgent = setAgentParameters(agents[0], agents[1]);

        //time resolution determines how long a single time step represents;
        timeResolution = 1 / activeAgent.GetComponent<AgentProps>().speed;

        //init the simulation
        time = Time.time;
        Ref.setActiveAgent(activeAgent);
        Ref.getGridGO().GetComponent<Grid>().init();
        foreach (GameObject obst in Ref.getObstacles())
        {
            obst.GetComponent<ObstManager>().init();
        }
    }

    GameObject setAgentParameters(GameObject activeAgent, GameObject passiveAgent)
    {
        activeAgent.SetActive(true);
        passiveAgent.SetActive(false);
        activeAgent.GetComponent<AgentProps>().speed = agentSpeed;
        return activeAgent;
    }

    public void stopSimulation(bool isCrash=false)
    {
        if (simEnded == false)
        {
            simEnded = true;
            if (isCrash)
            {
                return;
            }
            time = Time.time - time;
            print("finished in: " + time + "seconds");
            //print("spatial: " + spatialNodesVisited);
            //print("space time: " + spaceTimeNodesVisited);
            //print(nodeCountsPerFrame);
            print("totalNodes: " + (spatialNodesVisited + spaceTimeNodesVisited));
            //print("optimal path lenght:")
            //print("avgNodesPerCall: " + (spatialNodesVisited/Mathf.Max(spatialCallCounter,1) + spaceTimeNodesVisited/Mathf.Max(spaceTimeCallCounter,1)));
            int sum = 0;
            for (int i = 0; i < nodeCountsPerFrame.Count; i++)
            {
                sum += nodeCountsPerFrame[i];
            }
            print("avgNodesPerCall: " + sum/Mathf.Max(nodeCountsPerFrame.Count,1));
        }
    }
    public void addToSpatialScore(int score)
    {
        spatialNodesVisited += score;
        spatialCallCounter++;
        nodeCountsPerFrame[frameCount]+=score;
    }

    public void addToSpaceTimeScore(int score)
    {
       
        spaceTimeNodesVisited += score;
        spaceTimeCallCounter++;
        nodeCountsPerFrame[frameCount]+=score;
    }

    //public void setOptimalPathLength(int _optimalPathLength)
    //{
    //    optimalPathLength = _optimalPathLength;
    //}

    public GameObject getActiveAgent()
    {
        return activeAgent;
    }
    public void increaseFrameCount()
    {
        if (nodeCountsPerFrame.Count != 0)
        {
              frameCount++;
        }
        nodeCountsPerFrame.Add(0);
      
    }
}
