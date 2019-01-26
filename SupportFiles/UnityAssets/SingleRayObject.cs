using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleRayObject : MonoBehaviour
{
    public int lengthOfLineRenderer = 20;

    int counter = 0;

    public Vector3[] points = new Vector3[20];


    Vector3 TravelVector = new Vector3(1, 0, 0);
    


    void Update()
    {

       
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        

        Vector3 OldPosition = points[lengthOfLineRenderer-1];
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

        
        NewPosition.x = OldPosition.x + TravelVector.x;
        NewPosition.y = OldPosition.y + TravelVector.y;
        NewPosition.z = OldPosition.z + TravelVector.z;



        points[lengthOfLineRenderer-1] = NewPosition;
        lineRenderer.SetPositions(points);
        counter++;

    }
}
