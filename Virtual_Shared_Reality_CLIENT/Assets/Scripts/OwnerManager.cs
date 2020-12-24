using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwnerManager : MonoBehaviour
{
    static public int myID; 
    private int ID;
    private readonly object ownerlock = new object();
    
    public Outline outline;
    void Awake()
    {
       
        ID = -1;       
    }

    private void Start()
    {
        outline = gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.enabled = false;
            Debug.Log(" Object without outline is  " + name);
        }
          
        outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
        outline.OutlineWidth = 5f;
    }

    public void ChangeOwnerId(int OtherID)
    {
        lock (ownerlock)
        {
            
            ID = OtherID;
            
        }

      
        if (ID == myID)
        {
            /* im taking posses of the object so I change is color */


            SetToGreen();

        }
        else if( ID != -1)
        {

            SetToRed();
        }
    }

    public void SetToGreen()
    {
        outline.OutlineColor = Color.green;
        outline.enabled = true;
    }

    public int getID()
    {
        
          
            return ID;
        
    }

    public void FreeObject()
    {
        lock (ownerlock)
        {
            ID = -1;
        }
        outline.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetToRed()
    {
        outline.OutlineColor = Color.red;
        outline.enabled = true;
    }
    internal void setID(int id)
    {
        ID = id;
    }

    public void Block()
    {
        outline.OutlineColor = Color.yellow;
        outline.enabled = true;
    }
}
