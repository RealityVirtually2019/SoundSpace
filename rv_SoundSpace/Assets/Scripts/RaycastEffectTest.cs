
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

public class RaycastEffectTest : MonoBehaviour, IInputClickHandler
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
        
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        
        if (FocusManager.Instance.TryGetPointingSource(eventData, out currentPointingSource))
        {
            currentSourceId = eventData.SourceId;

            Ray ray = currentPointingSource.Ray;
            //Debug.Log(ray.origin);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.point);
            }

                
        }
    }
}
