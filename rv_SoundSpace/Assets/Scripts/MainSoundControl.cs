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
        if(eventData.state.source.handedness == InteractionSourceHandedness.Right)
            CreateEmitter(new Vector3(0, 0, 0));
    }

    public void CreateEmitter(Vector3 position)
    {
        GameObject newEmitter = Instantiate(emitterPrefab, position, Quaternion.identity);
        newEmitter.GetComponent<WaveEmitter>().MakeNoise();
    }
}
