using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VivePointer : MonoBehaviour
{
    public bool isHit;
    public RaycastHit hitResult;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        localRaycast();
    }

    private void localRaycast()

    {
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        Ray ray = new Ray(pos, forward);
        isHit = Physics.Raycast(ray, out hitResult);
        LineRenderer line = GetComponent<LineRenderer>();
        line.enabled = isHit;
        if (isHit)
        {
            line.SetPosition(0, pos);
            line.SetPosition(1, hitResult.point);
        }
    }

}
