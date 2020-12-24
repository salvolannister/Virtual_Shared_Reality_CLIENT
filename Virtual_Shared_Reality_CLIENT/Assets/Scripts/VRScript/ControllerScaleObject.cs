using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class ControllerScaleObject : MonoBehaviour
{
    private GameObject objectInHand; 
    SteamVR_TrackedObject trackedObj;
    public SteamVR_Action_Boolean ClickAction;
    public SteamVR_Action_Vector2 TouchAction;
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean grabAction;


    public float scaleFactor = 1.2f;
    private bool Selected = false;
    public Collider m_coll;
    private UpdatePositionHub UpPos;
    private const float Y_MIN = 0.1f;
    private Vector3 scaleOFFSET = new Vector3(0.002f, 0.002f, 0.002f);
    private void Start()
    {
        m_coll = gameObject.GetComponent<Collider>();
        UpPos = (UpdatePositionHub)UpdatePositionHub.Instance;
    }
    private void OnEnable()
    {
        HmdObjectSelector.OnSelected += EnableScale;
        HmdObjectSelector.OnDeselected += DisableScale;
    }

    private void OnDisable()
    {
        HmdObjectSelector.OnSelected -= EnableScale;
        HmdObjectSelector.OnDeselected -= DisableScale;
    }

    private void EnableScale(GameObject other, SteamVR_Input_Sources handType)
    {
        if (handType != this.handType)
            return;
        Selected = true;
        objectInHand = other;


    }

    private void DisableScale(GameObject other, SteamVR_Input_Sources handType) 
    {
        if (handType != this.handType)
            return;
        Selected = false;
        objectInHand = null;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Selected)
        {
            if (GetClickAction())
            {
                
                Transform grabbedTransform = objectInHand.transform;
                Vector2 pos = GetTouchAction();
                scaleOFFSET = grabbedTransform.localScale / 12;
                //Debug.Log("<color=red> click action </color> " + pos.y);
                if (pos.y >= Y_MIN)
                {
                  
                    grabbedTransform.localScale += scaleOFFSET;
                }
                else
                {
                    Vector3 actualScale = grabbedTransform.localScale;
                    actualScale -= scaleOFFSET;
                    if(actualScale.x >= 0 && actualScale.y>=0 && actualScale.z >= 0 )
                     grabbedTransform.localScale = actualScale;
                }

                UpPos.CheckCollision(grabbedTransform.gameObject, m_coll);
            }
        }
    }


    public bool GetClickAction()
    {
        
        return ClickAction.GetStateDown(handType);
    }



    public Vector2 GetTouchAction()
    {
        return TouchAction.axis;
    }

    
}
