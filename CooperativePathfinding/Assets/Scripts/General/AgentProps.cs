/*
*AgentProps.cs
*stores agent properties
*/
using UnityEngine;

public class AgentProps : MonoBehaviour
{
    public float speed;
    public int cooperationLenght;
    public bool localAvoidanceOnly;

    public float getSpeed()
    {
        return speed;
    }
}
