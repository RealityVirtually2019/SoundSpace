using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshWave : MonoBehaviour
{

    public GameObject go;
    private Raytrace1 raytrace;
    int rayCount = 2562;
    private bool hasMeshed = false;
    Vector3[] vects;
    
    // Start is called before the first frame update
    void Start()
    {
        vects = new Vector3[rayCount];
        raytrace = go.GetComponent<Raytrace1>();
    }

    // Update is called once per frame
    void Update()
    {
        if (raytrace.createMeshCounter > 3)
        {
            GenerateMesh();
            hasMeshed = true;
            Debug.Log("Generate Mesh");
        }
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


        mesh.vertices = raytrace.GetVectorsListAtIndex(1);
        mesh.triangles = TriangleList.tris;
        mesh.RecalculateNormals();
    }

    private void getVertexList(Vector3[][] array)
    {
       for (int i=0;i<rayCount;i++)
        {
            vects[i] = array[i][2];
        }
    }
    
}
