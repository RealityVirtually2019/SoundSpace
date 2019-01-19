using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSoundControl : MonoBehaviour
{

    public GameObject emitterPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject newEmitter = Instantiate(emitterPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            newEmitter.GetComponent<WaveEmitter>().MakeNoise();
        }
    }
}
