
using System;
using UnityEngine;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#if UNITY_WSA
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Input;
using HoloToolkit.Unity;
#endif
#else
using UnityEngine.VR;
#if UNITY_WSA
using UnityEngine.VR.WSA.Input;
#endif
#endif

using HoloToolkit.Unity.InputModule;


using Autodesk.Forge.ARKit;

public class RaycastEffectTest : MonoBehaviour
{

    private IPointingSource currentPointingSource;
    private uint currentSourceId;

    // Start is called before the first frame update
    void Start()
    {
        InteractionManager.InteractionSourcePressed += testHit;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis(InputMappingAxisUtility.CONTROLLER_LEFT_TRIGGER) > 0.8)
        {
            //Debug.Log("hello!");
        }
    }

    public void testHit(InteractionSourcePressedEventArgs eventData)
    {
        Debug.Log("hello");
        Vector3 pos;
        Vector3 dir;
        RaycastHit hit;
        if (eventData.state.sourcePose.TryGetPosition(out pos))
            if(eventData.state.sourcePose.TryGetForward(out dir))
            {
                Ray ray = new Ray(pos, dir);
                if (Physics.Raycast(ray, out hit))
                {                    
                    ForgeProperties fp = hit.collider.gameObject.GetComponentInParent<ForgeProperties>();
                    Debug.Log(fp.PropertiesString);
                    
                }
            }
    }
    
}
