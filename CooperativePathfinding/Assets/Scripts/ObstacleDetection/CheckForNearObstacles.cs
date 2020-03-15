/*
 * CheckForNearObstacles.cs
 * Maintains a list of obstacles near to the player.
 */

using UnityEngine;

public class CheckForNearObstacles : MonoBehaviour
{
    public VisibleObstManager visibleObstManagerScript;
    SphereCollider sphereCollider;
    public AgentProps agentPropsScript;
    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = (agentPropsScript.cooperationLenght*2+1)*agentPropsScript.getSpeed();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Obstacle")
        {
            visibleObstManagerScript.addObstacle(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Obstacle")
        {
            visibleObstManagerScript.removeObstacle(other.gameObject);
        }
    }

}
