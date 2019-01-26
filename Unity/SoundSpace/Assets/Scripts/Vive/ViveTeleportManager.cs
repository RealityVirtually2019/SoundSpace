using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

public class ViveTeleportManager : MonoBehaviour
{
    private VivePointer leftHandPointer;

    // Start is called before the first frame update
    void Start()
    {
        leftHandPointer = GetComponent<VivePointer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SteamVR_Input._default.inActions.Teleport.GetStateDown(SteamVR_Input_Sources.Any))
        {
            Teleport();
        }
    }

    private void Teleport()
    {
        GameObject mainCameraObject = GameObject.Find("[CameraRig]");
        if(leftHandPointer.isHit)
        {
            Vector3 offset = GameObject.Find("Camera").transform.localPosition;
            offset.y = 0;
            mainCameraObject.transform.position = leftHandPointer.hitResult.point - offset;
        }
            
    }
}
