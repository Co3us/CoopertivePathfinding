/*
 * PredictPath.cs
 * predicts in which direction the obstacle will move and passes it to GetCoveredNodes.cs
 */

using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;

public class PredictPath : MonoBehaviour
{
    //ref
    AgentProps agentPropsScript;
    BoxCollider boxCollider;
    Manager managerScript;
    GetCoveredNodes getCoveredNodesScript;
    ObstManager obstManagerScript;
    FollowPath followPathScript;
    Grid gridScript;

    List<Vector3> lastNPoints;
    Vector3 velocity;
    Vector3 pointInFuturePredicted;

    float predictAheadTimeFrames = 2;
    float prevTime = 0;
    int T;
    int samplingFreq = 3;
    int frameCount = 0;
    int numberOfPastPoints = 5;

    Vector3 prevPredictedPathVec;
    Vector3 startPoint;
    Vector3 prevBoundsExtends;
    bool isFirstIteration = true;
    float prevSpeed;
    Vector3 standingStillPoint;
    float standingInPlaceTime = 0;

    bool isInPlace;
    float speed;
    float timeFrequency;
    bool isActive;
    bool isLocked;

    void Start()
    {
        //set ref
        managerScript = Ref.getManagerGO().GetComponent<Manager>();
        gridScript = Ref.getGridScript();
        GameObject activeAgent = managerScript.getActiveAgent();
        followPathScript = activeAgent.GetComponent<FollowPath>();
        agentPropsScript = activeAgent.GetComponent<AgentProps>();
        boxCollider = GetComponent<BoxCollider>();
        obstManagerScript = GetComponent<ObstManager>();

        predictAheadTimeFrames = (agentPropsScript.cooperationLenght * 2 + 1);
        getCoveredNodesScript = GetComponent<GetCoveredNodes>();
        getCoveredNodesScript.setPredictAheadTime(predictAheadTimeFrames);
        getCoveredNodesScript.init();
        lastNPoints = new List<Vector3>();
    }

    void Update()
    {
        if (isActive == false)
        {
            return;
        }
        //sampling points
        if (frameCount >= samplingFreq)
        {
            if (lastNPoints.Count == 0)
            {
                prevTime = Time.time;
            }

            if (transform.position != standingStillPoint)
            {
                lastNPoints.Add(transform.position);
            }
            else if (Time.time - standingInPlaceTime > (predictAheadTimeFrames * managerScript.timeResolution) /2)
            {
                recalculatePath();
                standingInPlaceTime = Time.time;
            }

            frameCount = 0;

        }
        frameCount++;

        if (lastNPoints.Count >= numberOfPastPoints)
        {
            speed = getSpeed(lastNPoints, Time.time - prevTime);

            if (speed > 0)
            {
                isInPlace = false;
                obstManagerScript.isStandingStill = false;
                Vector3 vel = getDirectionVector(lastNPoints) * speed;

                T = Mathf.CeilToInt((predictAheadTimeFrames/speed) / Time.deltaTime);
                pointInFuturePredicted = transform.position + vel * Time.deltaTime * T;
                Vector3 currentPredPathVec = pointInFuturePredicted - transform.position;

                //reset box state
                if (isFirstIteration || Vector3.Angle(prevPredictedPathVec, currentPredPathVec) > 0.5f
                    || Vector3.Distance(transform.position, startPoint) > (predictAheadTimeFrames * speed) / 2
                    || Mathf.Abs(speed - prevSpeed) > 0.1f || boxCollider.bounds.extents != prevBoundsExtends)
                {
                    recalculatePath();
                    isFirstIteration = false;
                    startPoint = transform.position;
                    prevPredictedPathVec = currentPredPathVec;
                    prevSpeed = speed;
                }
            }
            else
            {
                isInPlace = true;
                if (prevSpeed > 0)
                {
                    standingInPlaceTime = Time.time;
                    recalculatePath();
                    standingStillPoint = transform.position;
                    obstManagerScript.isStandingStill = true;
                }
              
                prevSpeed = 0;
                isFirstIteration = true;
            }
            lastNPoints.Clear();
        }
        prevBoundsExtends = boxCollider.bounds.extents;
    }
    Vector3 getDirectionVector(List<Vector3> points)
    {
        int len = points.Count;
        double[] xdata = new double[len];
        double[] zdata = new double[len];
        for (int i = 0; i < len; i++)
        {
            xdata[i] = points[i].x;
            zdata[i] = points[i].z;
        }

        Tuple<double, double> p = Fit.Line(xdata, zdata);
        float b = (float)p.Item1;
        float a = (float)p.Item2;
        Vector2 dirVector;
        if (float.IsNaN(b))
        {
            if (zdata[0] < zdata[1])
                dirVector = new Vector2(0, 1);
            else
                dirVector = new Vector2(0, -1);
        }
        else
        {
            Vector2 A = new Vector2(points[0].x, a * points[0].x + b);
            Vector2 B = new Vector2(points[len - 1].x, a * points[len - 1].x + b);
            dirVector = (B - A).normalized;
        }

        return new Vector3(dirVector.x, 0, dirVector.y);
    }

    float getSpeed(List<Vector3> points, float elapsedTime)
    {
        float distanceTraveled = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            distanceTraveled += Vector3.Distance(points[i], points[i + 1]);
        }
        return distanceTraveled / elapsedTime;
    }

    public void setIsActive(bool _isActive)
    {
        isActive = _isActive;
    }

    public void setIsLocked(bool _isLocked)
    {
        isLocked = _isLocked;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawLine(transform.position, pointInFuturePredicted);
    //}
    public void recalculatePath(bool ignoreLock=false)
    {
        //TODO lock and ignore lock bool in parameter
        if (isLocked == false || ignoreLock)
        {
            getCoveredNodesScript.startMoveBox(speed, pointInFuturePredicted, isInPlace);
            followPathScript.isLocked = true;
        }
    }
}
