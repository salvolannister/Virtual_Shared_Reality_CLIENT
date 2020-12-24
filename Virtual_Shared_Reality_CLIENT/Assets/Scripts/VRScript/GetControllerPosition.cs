using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


/* takes the position of the controller in order to create the
 * origin (the gizmo) in that position*/

public class GetControllerPosition : MonoBehaviour
{
    public GameObject controller;
    public GameObject origin; 
    public SteamVR_Input_Sources handType; 
    public SteamVR_Action_Boolean gripAction; // action connected to the A or the Grip button
    public Canvas levelCanvas;
    public GameObject PR_Pointer;
    private Vector3 pos ;
    private Quaternion rot ;
    //private float offset = 0.041f; // HTC vive offset
    private float offset = 0.044f; // Oculus offset 

    /// <summary>
    /// Gets the grip pression in HTC Vive controller 
    /// or the A Button Pressure in  the Oculus one
    /// </summary>
    /// <returns></returns>
    public bool GetGrip() 
    {

        return gripAction.GetStateDown(handType);
    }

   
    void Update()
    {
        
            if (GetGrip())
            { 
                SetTheTable();
            }

    }
    /// <summary>
    /// Set the table position throught the controller position 
    /// making some adjustment 
    /// </summary>
    private void SetTheTable()
    {
        Transform body = controller.transform.Find("Model/body/attach");

        pos = body.position;
        rot = body.rotation;

        /* the position of the y is a little bit upper than the one 
              * desired so we decrease that in an emphiric way*/
        pos.y -= offset;

        Vector3 forward = rot * Vector3.forward;
        forward.y = 0; // project z axis on the plane y=0 
        Vector3 normalized = forward.normalized;
        rot = Quaternion.LookRotation(normalized, Vector3.up);
        origin.transform.SetPositionAndRotation(pos, rot);
     
    }
}
