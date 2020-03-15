/*
 * ProximityCheck.cs
 * casts rays in all direction around player and continuously checks for imminent collisions.
 */
using System.Collections.Generic;
using UnityEngine;

public class ProximityCheck : MonoBehaviour
{
    Vector3[] dirs = new Vector3[8];
    bool steeringAway;
    GameObject agent;
    AgentProps agentProps;
    FollowPath followPathScript;
    FindPathStatic findPathStaticScript;
    float timeSteering = 0;
    List<Vector3> possibleDirs = new List<Vector3>();
    Vector3 steerDir = new Vector3();

    private void Start()
    {
        agent = transform.parent.gameObject;
        findPathStaticScript = agent.GetComponent<FindPathStatic>();
        agentProps = agent.GetComponent<AgentProps>();
        Vector3 dir = transform.TransformDirection(Vector3.forward);
        followPathScript = agent.GetComponent<FollowPath>();

        //all dirs
        for (int i = 0; i < 8; i++)
        {
            dirs[i] = Quaternion.Euler(0, 45*i, 0) * dir;
        }
    }

    void Update()
    {
        possibleDirs.Clear();
        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3 dir = dirs[i];
            Debug.DrawRay(transform.position, dir * 0.5f, Color.blue);
            RaycastHit objectHit;

            bool hit = false;
            if (Physics.Raycast(transform.position, dir, out objectHit, 0.5f))
            {
                if (objectHit.transform.tag=="Obstacle")
                {
                    steeringAway = true;
                    followPathScript.enabled = false;
                    timeSteering = 0;
                    steerDir = dirs[(i + 4) % 8];
                    break;
                }
            }
          
        }
        if (steeringAway)
        {
            if (timeSteering > agentProps.speed/2)
            {
                steeringAway = false;
                followPathScript.enabled = true;
                findPathStaticScript.restartPath();
                return;
            }
            Vector3 target = agent.transform.position + steerDir * 2;
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, target, agentProps.speed * Time.deltaTime);
            timeSteering += Time.deltaTime;
        }
       
    }
}
