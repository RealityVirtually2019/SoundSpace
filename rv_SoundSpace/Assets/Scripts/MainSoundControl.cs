using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.WSA.Input;
//using HoloToolkit.Unity.InputModule;

public class MainSoundControl : MonoBehaviour
{

    public GameObject emitterPrefab;

    // Start is called before the first frame update
    void Start()
    {
        InteractionManager.InteractionSourcePressed += OnControllerPressed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateEmitter(new Vector3(0, 0, 0));
        }
    }

    public void OnControllerPressed(InteractionSourcePressedEventArgs eventData)
    {
        GameObject HMD = GameObject.Find("MixedRealityCameraParent");
        Vector3 offset = new Vector3(0, 0, 0);
        if (HMD!=null)
        {
            offset = HMD.transform.position;
        }
        if(eventData.state.source.handedness == InteractionSourceHandedness.Right && (eventData.pressType == InteractionSourcePressType.Grasp))
        {
            Vector3 pos;
            Vector3 forward;

            if (eventData.state.sourcePose.TryGetPosition(out pos) && (eventData.state.sourcePose.TryGetForward(out forward)))
                CreateEmitter(pos + offset + forward);
        }
            
    }

    public void CreateEmitter(Vector3 position)
    {
        GameObject newEmitter = Instantiate(emitterPrefab, position, Quaternion.identity);
        newEmitter.GetComponent<WaveEmitter>().MakeNoise();
    }
}
