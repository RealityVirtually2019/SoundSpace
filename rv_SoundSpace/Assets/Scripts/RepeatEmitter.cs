using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatEmitter : MonoBehaviour
{

    GameObject emitterPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startEmitting(GameObject emitter)
    {
        emitterPrefab = emitter;
        GameObject newEmitter = Instantiate(emitterPrefab, gameObject.transform.position, Quaternion.identity);
        newEmitter.GetComponent<WaveEmitter>().MakeNoise();

        StartCoroutine(emit(3));
        StartCoroutine(emit(6));
                
    }

    IEnumerator emit(int seconds)
    {
       
        yield return new WaitForSeconds(seconds);
        GameObject newEmitter = Instantiate(emitterPrefab, gameObject.transform.position, Quaternion.identity);
        newEmitter.GetComponent<WaveEmitter>().MakeNoise();

    }
}
