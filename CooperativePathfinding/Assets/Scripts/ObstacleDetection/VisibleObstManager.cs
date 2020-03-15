/*
 * VisibleObstManager.cs
 * Manages obstacles near player.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public class VisibleObstManager : MonoBehaviour
{
    public List<ObstManager> Obstacles = new List<ObstManager>();
    FindPath findPathScript;
    bool isInitalSearch = true;
    int heighestIndex = 0;
    public Grid gridScript;
    public Manager managerScipt;
    int recalculatedObstCounter = 0;
    int numOfObstacles = 0;
    bool shouldReturnSignal;

    private void Start()
    {
        findPathScript = GetComponent<FindPath>();
    }

    public void addObstacle(GameObject obst)
    {
        ObstManager obstManager = obst.GetComponent<ObstManager>();
        obstManager.setIsActive(true);

        //if obstacle has not yet been asigned an index
        if (obstManager.getIndex() == -1)
        {
            Obstacles.Add(obstManager);
            obstManager.setIndex(heighestIndex);
            heighestIndex++;
            numOfObstacles++;
            gridScript.addObstPath();
        }
        else
        {
            Obstacles[obstManager.getIndex()] = obstManager;
        }

        if (managerScipt.isLocalAvoidance)
        {
            obstManager.GetComponent<PredictPath>().enabled = false;
            obstManager.GetComponent<GetCoveredNodes>().enabled = false;
        }

    }
    public void removeObstacle(GameObject obst)
    {
        ObstManager obstManager = obst.GetComponent<ObstManager>();
        obstManager.setIsActive(false);
        obstManager.cleanPath();
        Obstacles[obstManager.getIndex()] = null;
        numOfObstacles--;

    }
    private void affectAllObst(Action<ObstManager> obstAction)
    {
        foreach (ObstManager obstManager in Obstacles)
        {
            if (obstManager != null)
            {
                obstAction(obstManager);
            }
        }
    }

    public void lockUnlockAllObst(bool shouldLock)
    {
        affectAllObst((ObstManager obstManager) => { obstManager.lockUnlockObts(shouldLock); });
    }

    public void recalcAllObst()
    {
        if (numOfObstacles == 0)
            sendObstRecalculatedSignal();

        recalculatedObstCounter = 0;
        affectAllObst((ObstManager obstManager) => { obstManager.recalculatePath(); });
    }
    public void addToRecalcObstCounter()
    {
        recalculatedObstCounter++;
        if (recalculatedObstCounter >=numOfObstacles && shouldReturnSignal)
        {
            sendObstRecalculatedSignal();
            shouldReturnSignal = false;
        }
    }

    public List<Node> getAllObstCoveredNodes()
    {
        List<Node> coveredNodes = new List<Node>();
        foreach (ObstManager obstManager in Obstacles)
        {
            List<Node> foundNodes = new List<Node>();
            if (obstManager != null)
            {

                foundNodes = obstManager.getCoveredNodes();
                foreach(Node node in foundNodes)
                {
                    coveredNodes.Add(node);
                }
            }
        }
        return coveredNodes;
    }

    public void setReturnSignal(bool _shouldReturnSignal)
    {
        shouldReturnSignal = _shouldReturnSignal;
    }

    void sendObstRecalculatedSignal()
    {
        findPathScript.getNewPathChunk();
    }

}
