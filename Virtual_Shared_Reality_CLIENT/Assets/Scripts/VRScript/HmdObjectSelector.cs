using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using SignalR;
using System;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// Check if an object collides with collider and set it as collidingObject,
/// if the button trigger is pressed then the collidingObject becomes the objectInhand 
/// and the event OnSelected is triggered
/// </summary>
public class HmdObjectSelector : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean grabAction;
    public OpenConnection openConnection;
   
    public UpdatePositionHub UpPos;
    private Collider m_coll;
    private GameObject collidingObject;
    private GameObject objectInHand;
    public delegate void HmdEvent(GameObject other, SteamVR_Input_Sources handType);
    
    public static event HmdEvent OnSelected;
    public static event HmdEvent OnDeselected;
    // Start is called before the first frame update
    public bool go = true;
  

    private void Start()
    {
        if (!UpdatePositionHub.fired)
            UpdatePositionHub.UpPosReady += GetUpPos;
        else UpPos = (UpdatePositionHub)UpdatePositionHub.Instance;
        m_coll = gameObject.GetComponent<Collider>();
    }
    public void GetUpPos()
    {
        UpPos = (UpdatePositionHub)UpdatePositionHub.Instance;
    }
    // Update is called once per frame
    void Update()
    {
        if (grabAction.GetLastStateDown(handType))
        {
            if (collidingObject && UpdatePositionHub.Instance.TryMove(collidingObject))
            {
                OnSelected.Invoke(collidingObject, handType);
                this.objectInHand = collidingObject;
                
            }
            collidingObject = null;
        }

        if (grabAction.GetLastStateUp(handType))
        {
           
            if (objectInHand)
            {
                
                if (!UpdatePositionHub.Instance.FreeObject(objectInHand)) 
                    Debug.LogWarning(" You tried to release " + objectInHand.name + " that wasn't yours");
               
                    OnDeselected.Invoke(objectInHand, handType);
                    ReleaseObject();
                
            }
        }else if (objectInHand && !ClientManager.Instance.TUTORIAL)
        {
            UpdatePositionHub.Instance.ReactToUpdate(objectInHand);
        }

    }

  
    private void ReleaseObject()
    {
        objectInHand = null;
    }
    private void SetCollidingObject(Collider col)
    {
        // 1
        if (collidingObject)
        {
            return;
        }

        collidingObject = col.gameObject;
    }

   
    public void OnTriggerEnter(Collider other)
    {
        //Debug.Log("<color=green> colliding</color>+ me "+name+ " with "+ other.transform.name);
        if (m_coll != other && !other.isTrigger)
            SetCollidingObject(other);
    }

    
    public void OnTriggerStay(Collider other)
    {
        if (m_coll != other && !other.isTrigger)
        {
            SetCollidingObject(other);

        }

    }

    
    public void OnTriggerExit(Collider other)
    {
        if (collidingObject != other.gameObject)
        {
            return;
        }

        collidingObject = null;
    }

   
}
