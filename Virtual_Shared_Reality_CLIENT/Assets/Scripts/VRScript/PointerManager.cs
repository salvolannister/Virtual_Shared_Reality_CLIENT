using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class PointerManager : MonoSingleton<PointerManager>
{
    public float m_DefaultLength = 5.0f;
    public GameObject m_Dot;
    public VRInputModule m_InputModule;
    public GameObject Controller;
    private LineRenderer m_LineRenderer = null;
    public SteamVR_Action_Boolean laserAction;
    public SteamVR_Input_Sources handType;
    private bool PointerOn = false;
    // Start is called before the first frame update
 

    public override void Init()
    {
        base.Init();
        m_LineRenderer = GetComponent<LineRenderer>();
        m_LineRenderer.enabled = false;
        m_Dot.SetActive(false);
    }

    void Update()
    {

        if (laserAction.GetStateDown(handType))
        {
            ChangeLaserStatus(true);
        }

       

        if (laserAction.GetStateUp(handType))
        {
            ChangeLaserStatus(false);
        }

        if (PointerOn)
            UpdateLine();
    }

    private void ChangeLaserStatus(bool status)
    {
        
        PointerOn = status;
        m_LineRenderer.enabled = status;
        m_Dot.SetActive(status);
    }

    private void UpdateLine()
    {
        Quaternion rot  = Controller.transform.rotation;
        Vector3 pos =  Controller.transform.position;

        transform.SetPositionAndRotation(pos, rot);


        PointerEventData data = m_InputModule.GetData();  
        
        float targetLength = data.pointerCurrentRaycast.distance == 0 ? m_DefaultLength : data.pointerCurrentRaycast.distance;

        RaycastHit hit = CreateRaycast(targetLength);
      
        // default endposition in case nothing is hit
        Vector3 endPosition = transform.position + (transform.forward * targetLength);

        if (hit.collider != null)
            endPosition = hit.point;

        //beam's sphere position
        m_Dot.transform.position = endPosition;

        //set linerenderer
        m_LineRenderer.SetPosition(0, transform.position);
        m_LineRenderer.SetPosition(1, endPosition);
        
    }

    /// <summary>
    /// Cast a ray with length "length" and returns 
    /// the objetc with the information about it
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    private RaycastHit CreateRaycast(float length)
    {
        RaycastHit hit;
       
        Ray ray = new Ray(transform.position, transform.forward);
        Physics.Raycast(ray, out hit, length);

        return hit;
    }
}
