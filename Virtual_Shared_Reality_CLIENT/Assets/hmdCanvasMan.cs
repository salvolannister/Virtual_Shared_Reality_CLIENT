using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class hmdCanvasMan : MonoBehaviour
{
    public SteamVR_Action_Boolean click;
    public HmdObjectSelector hmdSendChangeScript;
   
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        /*deactivate older actions*/
        hmdSendChangeScript.enabled = false;
        /*enable laser*/
    }

    private void OnDisable()
    {
        //deactivate laser
        //    activate older actions
        hmdSendChangeScript.enabled = true;
    }
    void Update()
    {
        
    }
}
