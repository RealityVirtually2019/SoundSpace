using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshWave : MonoBehaviour
{
    public bool enableMesh = true;
    private WaveEmitter waveEmitter;
   
    // Start is called before the first frame update
    void Start()
    {       
        waveEmitter = GetComponent<WaveEmitter>();
    }

    // Update is called once per frame
    void Update()
    {
        if(enableMesh)
            GenerateMesh();
    }

    public void GenerateMesh()
    {
        var mesh = new Mesh();
        if (!transform.GetComponent<MeshFilter>() || !transform.GetComponent<MeshRenderer>()) //If you havent got any meshrenderer or filter
        {
            transform.gameObject.AddComponent<MeshFilter>();
            transform.gameObject.AddComponent<MeshRenderer>();
        }

        transform.GetComponent<MeshFilter>().mesh = mesh;


        mesh.vertices = waveEmitter.GetVectorsListAtIndex(1);
        mesh.triangles = TriangleList.tris;
        mesh.RecalculateNormals();
    }
       
    
}
