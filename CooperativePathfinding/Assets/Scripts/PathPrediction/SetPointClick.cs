using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPointClick : MonoBehaviour
{
    Camera cam;
   public GameObject Obstacle;
    MoveObstacleToPoint moveObstScript;
    // Start is called before the first frame update
    void Start()
    {
        moveObstScript = Obstacle.GetComponent<MoveObstacleToPoint>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 clickPoint= cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
                moveObstScript.currentTarget = new Vector3(clickPoint.x, moveObstScript.targetPoint.y, clickPoint.z);
            }
        }

    }
}
