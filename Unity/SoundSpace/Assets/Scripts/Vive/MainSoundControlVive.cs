using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;
//using HoloToolkit.Unity.InputModule;

public class MainSoundControlVive : MonoBehaviour
{
    public VivePointer rightHandPointer;
    public GameObject emitterPrefab;
    public GameObject emitterBeamPrefab;
    public GameObject repeatEmitterPrefab;
    public bool MeshesAreVisible = true;
    TriangleList.EmitType emitType = TriangleList.EmitType.Beam;

    // Start is called before the first frame update
    void Start()
    {
        //InteractionManager.InteractionSourcePressed += OnControllerPressed;
        //InteractionManager.InteractionSourceUpdated += OnSourceUpdated;

        //TurnCrowdOff();
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateEmitter(new Vector3(0, 0, 0));
        }

        if (SteamVR_Input._default.inActions.EmitSound.GetStateDown(SteamVR_Input_Sources.Any))
        {
            if (rightHandPointer.isHit)
                CreateEmitter(rightHandPointer.hitResult.point + rightHandPointer.hitResult.normal.normalized);
        }
    }
    


    public void CreateEmitter(Vector3 position)
    {
        GameObject newEmitter = Instantiate(emitterPrefab, position, Quaternion.identity);
        newEmitter.GetComponent<WaveEmitter>().MakeNoise();

       }

    public void ChangeTypeToBeam()
    {
        emitType = TriangleList.EmitType.Beam;

    }
    public void ChangeTypeToSphere()
    {
        emitType = TriangleList.EmitType.LowRes;

    }

    public void ToggleMeshes(bool newVisiblity)
    {
        MeshesAreVisible = newVisiblity;
        MeshWave[] wavelist = GameObject.FindObjectsOfType<MeshWave>();
        foreach(MeshWave mw in wavelist)
        {
            mw.GetComponent<MeshRenderer>().enabled = MeshesAreVisible;
        }
        MeshWaveBeam[] beamlist = GameObject.FindObjectsOfType<MeshWaveBeam>();
        foreach (MeshWaveBeam m in beamlist)
        {
            m.GetComponent<MeshRenderer>().enabled = MeshesAreVisible;
        }
    }


    public void CreateRepeatEmitter(Vector3 position, Quaternion rotation, GameObject emitter)
    {
        GameObject newEmitter = Instantiate(repeatEmitterPrefab, position, rotation);
        newEmitter.GetComponent<RepeatEmitter>().startEmitting(emitter, emitType);
    }

    public void TurnCrowdOn()
    {
        WaveEmitterCrowd[] crowd = GameObject.FindObjectsOfType<WaveEmitterCrowd>();
        foreach(WaveEmitterCrowd c in crowd)
        {
            //c.gameObject.SetActive(true);
            c.reEnable();
        }
    }

    public void TurnCrowdOff()
    {
        WaveEmitterCrowd[] crowd = GameObject.FindObjectsOfType<WaveEmitterCrowd>();
        foreach (WaveEmitterCrowd c in crowd)
        {
            //c.gameObject.SetActive(false);
            c.clearPoints();
        }
    }

}
