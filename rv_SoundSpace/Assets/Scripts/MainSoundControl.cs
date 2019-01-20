using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.WSA.Input;
//using HoloToolkit.Unity.InputModule;

public class MainSoundControl : MonoBehaviour
{

    public GameObject emitterPrefab;
    public GameObject emitterBeamPrefab;
    public GameObject repeatEmitterPrefab;

    

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
        //change gameobject based on emittype

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
            Quaternion rot;

            if (eventData.state.sourcePose.TryGetPosition(out pos) && (eventData.state.sourcePose.TryGetForward(out forward)) && eventData.state.sourcePose.TryGetRotation(out rot))
                CreateRepeatEmitter(pos + offset + forward, rot, emitterBeamPrefab);
        }
            
    }

    public void CreateEmitter(Vector3 position)
    {
        GameObject newEmitter = Instantiate(emitterPrefab, position, Quaternion.identity);
        newEmitter.GetComponent<WaveEmitter>().MakeNoise();

       }






    public void CreateRepeatEmitter(Vector3 position, Quaternion rotation, GameObject emitter)
    {
        GameObject newEmitter = Instantiate(repeatEmitterPrefab, position, rotation);
        newEmitter.GetComponent<RepeatEmitter>().startEmitting(emitter);
    }
}
