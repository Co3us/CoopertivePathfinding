using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;

public class MoveObstacleToPoint : MonoBehaviour
{
    public Vector3 targetPoint;
    Vector3 returnPoint;
    public  Vector3 currentTarget;
    float speed = 2f;
    float rotSpeed = 2;
    float nodeDiameter = 0.5f;
    Vector3 velocity;
    int T = 30;
    int samplingFreq = 3;
    int frameCount = 0;
    Vector3 pointInFuture;
    ObstManager obstManagerScript;
    bool changeRot;
    public bool isBackAndForth;
    // Start is called before the first frame update
    void Start()
    {
        //targetPoint = new Vector3(-5, transform.position.y, 0.5f);
        obstManagerScript = GetComponent<ObstManager>();
        speed = obstManagerScript.speed;
        returnPoint = transform.position;
        currentTarget = targetPoint;
    }

    // Update is called once per frame
    void Update()
    {
        //moving obstacle
        velocity = (currentTarget - transform.position).normalized * speed;
        if (Mathf.Abs(Vector3.Distance(transform.position, currentTarget)) < 0.1f)
        {
            transform.position = currentTarget;

            if (currentTarget == targetPoint&&isBackAndForth)
                currentTarget = returnPoint;
            else
                currentTarget = targetPoint;

        }
        else
        {
            transform.position = transform.position + velocity * Time.deltaTime;
            pointInFuture = transform.position + velocity * Time.deltaTime * T;
        }

        //rotation
        if (currentTarget == targetPoint)
        {
            Vector3 targetDir = currentTarget - transform.position;

            float step = rotSpeed * Time.deltaTime;

            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
           // Debug.DrawRay(transform.position, newDir, Color.red);

            transform.rotation = Quaternion.LookRotation(newDir);
        }




    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetPoint, 0.15f);
        //Gizmos.DrawLine(transform.position, pointInFuture);
    }
}
