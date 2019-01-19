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
        if (!transform.GetComponent<MeshFilter>()) //If you havent got any meshrenderer or filter
        {           
            transform.gameObject.AddComponent<MeshFilter>();
        }

        transform.GetComponent<MeshFilter>().mesh = mesh;


        //create empty color list
        Color[] colors = new Color[2562];
        for (int i=0;i<2562;i++)
        {
            colors[i] = new Color(0f, 0f, 1f, 0.4f);
        }
        //

        
        mesh.vertices = waveEmitter.GetVectorsListAtIndex(1);
        mesh.triangles = TriangleList.tris;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }
       
    
}
