using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Use this script to calculate radius and center.
/// If u need to create a new level enable is centroid, this will calculate the characteristic and then send a signal 
/// in order to write them in a file
/// </summary>
public class PropManager : MonoBehaviour
{   
    [Tooltip("Radius used to calculate the aproximated collider")]
    public float radius = 0.2f;
    public float scaledR;
    public Vector3 centroid;
    public float  lossyScale;
    [Tooltip("Set to true if you want to adjust off centered 3D models")]
    public bool UseCentroid = false;
    public void Start()
    {
        try
        {
        
            if( UseCentroid !=false)
            {
                CreateCollisionProperties();
                scaledR = GetScaledRadius();
                lossyScale = transform.lossyScale.x;
                SignalDone();
            }
               
        }
        catch{
                    Debug.Log("No mesh is present");
                    
        }
    }

    public void Awake()
    {
        Init();
    }

    public void CreateCollisionProperties()
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;


        if (UseCentroid)
        {
            centroid = FindCentroid(mesh);
            //Vector3 centroid = FindCentroid(mesh);
            GameObject center = new GameObject
            {
                name = "centroid"
               

            };
            center.transform.parent = gameObject.transform;
            center.transform.localPosition = centroid;
        }
        else
        {

            centroid = transform.position;
        }


        radius = GetFurthestVertex(mesh, centroid); // can be improved using boun
    }

    public virtual void Init() { }

    public float GetScaledRadius()
    {

        scaledR = radius * gameObject.transform.lossyScale.x;
        
        return scaledR;
    }
  
    public float GetFurthestVertex(Mesh mesh, Vector3 point)
    {
        float max = 0f;
        
        float distance = 0;
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            distance = Vector3.Distance(point, mesh.vertices[i]);
            if (distance > max)
                max = distance;
        }
        return max;
    }

    public Vector3 FindCentroid(Mesh mesh)
    {
        Vector3 centroid = Vector3.zero;
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            centroid += mesh.vertices[i];

        }
        return centroid /= mesh.vertexCount;
    }



    void SignalDone()
    {
        if (CreateLevelScene.Instance == null)
            Debug.LogError("CreateLevel is missing");
        else
        {
            CreateLevelScene.Instance.AddCount();
        }
    }

}
