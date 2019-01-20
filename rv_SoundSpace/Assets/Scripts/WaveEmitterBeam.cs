﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEmitterBeam : MonoBehaviour
{

    //Line Render Color

    float colorCounter = 1.0f;
    float colorFalloff = 0.001f;
    float colorWallHitFalloff = 0.05f;
    public float[] colorArrayAlpha = new float[1200];

    public int pause = 0; //PAUSE THE GAME
    

    public int lengthOfLineRenderer = 5; // The number of positions on the line renderer
    public GameObject linePrefab; // reference the linestyle prefab
    public GameObject spherePrefab;

    //General Variables

    public int counter = 0;  // global counting variable 

    public int numOfVectors; // the number of mesh verticies... this number would change based on emitter throw angle
    public float stepFactor = 0.5f; //distance per step... this decreases the size + speed, and increases the resolution
    //public float stepFactor1 = 0.5f; //distance per step... this decreases the size + speed, and increases the resolution
    public int hasBeenPressed = 0; //check if mouse has been pressed, this could just be a boolean....


    // Empty Arrays

    public Vector3[][] myarrays = new Vector3[1200][]; //the 3D Points. there are 2562 lines (numOfVectors) made up of 20 points (lengthOfLineRenderer) to make a line

    public GameObject[] EmptyRayHolders; //Each line renderer requires its own GameObject. These are those gameobjects.
    private Vector3[] TravelVectors; //Vectors set in the start below



    void Start()
    {
        for (int m = 0; m < numOfVectors; m++)
        {
            colorArrayAlpha[m] = 1.0f;
        }
            //TriangleList.emitType = TriangleList.EmitType.Beam;
            TravelVectors = TriangleList.GetVectorList(TriangleList.EmitType.Beam);
        numOfVectors = TriangleList.GetVectorCount(TriangleList.EmitType.Beam);
        //myarrays = new Vector3[numOfVectors][];

        Quaternion rot = gameObject.transform.rotation;
        for(int i = 0;i<numOfVectors;i++)
        {
            TravelVectors[i] = rot * TravelVectors[i];
        }


    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            PauseSound();
        }



        if (hasBeenPressed == 1)
        {

            for (int m = 0; m < numOfVectors; m++)
            {


                LineRenderer lineRenderer = EmptyRayHolders[m].GetComponent<LineRenderer>();

                Vector3 OldPosition = myarrays[m][lengthOfLineRenderer - 1];
                Vector3 NewPosition = new Vector3(0, 0, 0);



                for (int i = 0; i < lengthOfLineRenderer - 1; i++)
                {
                    int j = i + 1;
                    myarrays[m][i] = myarrays[m][j];

                }



                RaycastHit hit;
                if (Physics.Raycast(OldPosition, TravelVectors[m], out hit, stepFactor))
                {
                    TravelVectors[m] = Vector3.Reflect(TravelVectors[m], hit.normal);

                    //Instantiate(spherePrefab, hit.point, Quaternion.identity);

                    //colorArrayAlpha[m] = colorArrayAlpha[m] - colorWallHitFalloff;
                    colorArrayAlpha[m] = colorArrayAlpha[m] * (1 - GetNRC.getNRCFromCollider(hit.collider));

                }


                // Change the NewPosition Vector's x and y components
                NewPosition.x = OldPosition.x + (TravelVectors[m].x * stepFactor);
                NewPosition.y = OldPosition.y + (TravelVectors[m].y * stepFactor);
                NewPosition.z = OldPosition.z + (TravelVectors[m].z * stepFactor);

                Color c1 = new Color(.2f, .6f, 1, colorArrayAlpha[m]);
                Color c2 = new Color(1, 1, 1, 0);

                myarrays[m][lengthOfLineRenderer - 1] = NewPosition;
                lineRenderer.SetPositions(myarrays[m]);
                lineRenderer.SetColors(c2, c1);

                //Set material color and opacity


                colorArrayAlpha[m] = colorArrayAlpha[m] - colorFalloff;

            }
            
            colorCounter = colorCounter - colorFalloff;

            if (pause == 1) // to pause the game
            {
                hasBeenPressed = 0;
            }

            if (colorCounter < .01)
            {
                for (int m = 0; m < numOfVectors; m++)
                {
                    Destroy(EmptyRayHolders[m]);
                }
                Destroy(gameObject);
                hasBeenPressed = 0;

            }
            counter++;
            //Debug.Log(Time.deltaTime);

        }
    }

    public void MakeNoise()
    {
        //hasBeenPressed = 1;
        for (int x = 0; x < numOfVectors; x++)
        {
            myarrays[x] = new Vector3[5];
            for (int y = 0; y < 5; y++)
            {
                //myarrays[x][y] = new Vector3(0.5f, 0.5f, 0.5f); // change this variable to change where the sound is emitted
                myarrays[x][y] = gameObject.transform.position; // change this variable to change where the sound is emitted
                
            }
        }

        Debug.Log("MousePressed");
        hasBeenPressed = 1;


        EmptyRayHolders = new GameObject[numOfVectors];
        for (int i = 0; i < numOfVectors; i++)
        {
            GameObject currentGameObject = Instantiate(linePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            EmptyRayHolders[i] = currentGameObject;
        }
    }

    public void PauseSound()
    {
        if (hasBeenPressed == 0)
        {
            hasBeenPressed = 1;
        }
        else
        {
            hasBeenPressed = 0;
        }
    }


        public Vector3[] GetVectorsListAtIndex(int index)
    {
        Vector3[] vects = new Vector3[numOfVectors];
        for (int i = 0; i < numOfVectors; i++)
        {
            vects[i] = myarrays[i][1] - gameObject.transform.position;
        }
        return vects;
    }
}