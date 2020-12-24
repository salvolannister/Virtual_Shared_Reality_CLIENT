using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using SignalR;
using System;
using Microsoft.AspNetCore.SignalR.Client;

/*This class has the purpose of making possible to connect object to the controllers
 * and send information about their movement while they are on the end of both players*/

public class ControllerGrabObject : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean grabAction;
    public SteamVR_Action_Boolean clickAction;
    private GameObject objectInHand; 
    public OpenConnection openConnection;
    
   
    private Vector3 _grabbedPositionOnController;
    private Quaternion _grabbedRotationOnController;
    private Collider m_deviceCollider;

    private void Start()
    {
        m_deviceCollider = gameObject.GetComponent<Collider>();

    }


    private void OnEnable()
    {
        HmdObjectSelector.OnSelected += GrabObject;
        HmdObjectSelector.OnDeselected += ReleaseObject;
    }

   
    private void OnDisable()
    {
        HmdObjectSelector.OnSelected -= GrabObject;
        HmdObjectSelector.OnDeselected -= ReleaseObject;
     
    }

  


    private void Update()
    {
       


        if (objectInHand != null && !GetClickState())
        {
            Transform grabbedTransform = objectInHand.transform;
            grabbedTransform.position = transform.localToWorldMatrix.MultiplyPoint(_grabbedPositionOnController);

            grabbedTransform.rotation = transform.rotation * _grabbedRotationOnController;

            UpdatePositionHub.Instance.CheckCollision(grabbedTransform.gameObject, m_deviceCollider);
        }

       
    }
   
 
    private bool GetClickState()
    {
        return clickAction.GetStateDown(handType);
    }

    
    private void GrabObject(GameObject collidingObject, SteamVR_Input_Sources hand)
    {
      
        if (!hand.Equals(handType))
            return;
      
        objectInHand = collidingObject;


        Transform grabbedTransform = objectInHand.transform;
        
        _grabbedPositionOnController = transform.worldToLocalMatrix.MultiplyPoint(grabbedTransform.position);
        _grabbedRotationOnController = Quaternion.Inverse(transform.rotation) * grabbedTransform.rotation;
        
       
    }


    private void ReleaseObject(GameObject objectInHand, SteamVR_Input_Sources hand)
    {
       
       
        if ( objectInHand == this.objectInHand && hand.Equals(handType))
        {
           
            this.objectInHand = null;
        }
        
    }

    
    
}
