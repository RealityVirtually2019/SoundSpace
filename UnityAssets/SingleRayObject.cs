using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleRayObject : MonoBehaviour
{
    public int lengthOfLineRenderer = 20;

    public int counter = 0;


    Vector3 TravelVector = new Vector3(1, 0, 0);
    Vector3[] points = new Vector3[20];




    void Update()
    {

       
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        

        Vector3 OldPosition = points[19];
        Vector3 NewPosition = new Vector3(0, 0, 0);



        for (int i = 0; i < lengthOfLineRenderer - 1; i++)
        {
            int j = i + 1;
            points[i] = points[j];

        }



        RaycastHit hit;
        if (Physics.Raycast(OldPosition, TravelVector, out hit, 1))
        {
            TravelVector = Vector3.Reflect(TravelVector, hit.normal);
            Debug.Log(hit.normal);

        }
        else
        {
            Debug.Log("Did not Hit");
        }

        //if raycast = true
        //find raycast hit point and normal
        //vector3.reflect using normal @ point
        //change TravelVector to this new vector

        // Change the NewPosition Vector's x and y components
        NewPosition.x = OldPosition.x + TravelVector.x;
        NewPosition.y = OldPosition.y + TravelVector.y;
        NewPosition.z = OldPosition.z + TravelVector.z;



        points[19] = NewPosition;
        lineRenderer.SetPositions(points);
        counter++;

    }
}
