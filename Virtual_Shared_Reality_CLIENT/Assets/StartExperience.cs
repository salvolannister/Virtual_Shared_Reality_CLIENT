using Assets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

/// <summary>
/// Script used in VR to make the users learn how to use the laser and set the table position
/// </summary>
public class StartExperience : MonoBehaviour
{
#if UNITY_STANDALONE
    public GameObject LaserCanvas;
    public GameObject LevelCanvas;
    //private float OFFSET = 0.5f;
    public TextMeshProUGUI text;
    private GetControllerPosition[] getControllerPositions;
    public  Button finishStart;
    enum Starting
    {
        SetTable =0,
        LaserActivation =1
    }

    Starting phase;
    public void Start()
    {
        text.text = TutorialMessages.moveTableText;
        getControllerPositions = FindObjectsOfType<GetControllerPosition>();
        phase = Starting.SetTable;
        finishStart.gameObject.SetActive(false);
    }

    public void Update()
    {
        if(getControllerPositions!= null  && phase == Starting.SetTable)
        {
            foreach (var getControllerPosition in getControllerPositions)
            {
                if (getControllerPosition.GetGrip())
                {
                    phase = Starting.LaserActivation;
                    text.text = TutorialMessages.laserManagementText;
                    finishStart.gameObject.SetActive(true);
                    
                }
            }
        }
    }
    //Listen for A pressure

    // Charge the new Canvas for the laser


    // Start is called before the first frame update
    //void Start()
    //{
    //    GameObject Camera= GameObject.FindGameObjectWithTag("MainCamera");
    //    if(Camera == null)
    //    {
    //        Debug.LogError("There is no Main Camera in the experience!");
    //    }
    //    else
    //    {
    //        //Vector3 newPosition = Camera.transform.position;
    //        //newPosition.z -= OFFSET;
    //        //newPosition.y -= OFFSET;
    //        LaserCanvas.transform.SetParent(Camera.transform, false);
    //        Vector3 newPosition = Camera.transform.position;
    //        newPosition.z -= OFFSET;
    //        newPosition.y -= OFFSET;
    //        LaserCanvas.transform.position = newPosition;
    //        //LaserCanvas.transform.forward = Camera.transform.forward ;
    //    }
    //}


    public void Close()
    {
        // Open the Main Canvas
        LevelCanvas.SetActive(true);
        // Destroy the GameObject
        Destroy(this.gameObject);
    }

#endif
}
