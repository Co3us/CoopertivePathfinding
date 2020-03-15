/*
 * ObstManager.cs
 * set up and manage a single obstacle
 */

using System.Collections.Generic;
using UnityEngine;

public class ObstManager : MonoBehaviour
{
    //ref 
    PredictPath predictPathScript;
    GetCoveredNodes getCoveredNodesScript;
    BoxCollider boxCollider;
    Manager managerScript;
    Grid gridScript;

    public float speed;

    [HideInInspector]
    public bool isStandingStill; 
    
    //index in the array of all obstacles in scene
    int index=-1;

    bool isActive;
    
    public void init()
    {
        //set ref
        managerScript = Ref.getManagerGO().GetComponent<Manager>();
        gridScript = Ref.getGridScript();
        boxCollider = GetComponent<BoxCollider>();
        predictPathScript = GetComponent<PredictPath>();
        getCoveredNodesScript = GetComponent<GetCoveredNodes>();


        if (managerScript.isLocalAvoidance)
        {
            getCoveredNodesScript.getPredictBox().SetActive(false);
            //change layer of obstacle to Unwalkable
            gameObject.layer = 9;
        }
    }

    //public void shiftNodes(int shiftAmount)
    //{
    //    if (isStandingStill == false)
    //    {
    //        gridScript.shiftAllObstNodes(index, shiftAmount);
    //    }
    //}
    public void setIndex(int _index)
    {
        index = _index;
        getCoveredNodesScript.setIndex(_index);
    }

    public List<Node> getCoveredNodes()
    {
        return gridScript.getNodesInsideBox(boxCollider, 0);
    }

    public int getIndex()
    {
        return index;
    }

    public void setIsActive(bool _isActive)
    {
        isActive = _isActive;
        predictPathScript.setIsActive(_isActive);
    }

    public void lockUnlockObts(bool isLocked)
    {
        getCoveredNodesScript.setLocked(isLocked);
    }

    public void cleanPath()
    {
        gridScript.cleanObstPath(index,true);
    }

    public void recalculatePath()
    {
        predictPathScript.recalculatePath(true);
    }

    public void fillTillEnd()
    {
        gridScript.fillTillEnd(index);
    }
}
