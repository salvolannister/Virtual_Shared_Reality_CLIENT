using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

using UnityEngine;


/// <summary>
/// Tutorial to tell players what to do in the second level, givin warning if selectin the wrong element
/// </summary>
public class LevelTwoTutorial : MonoBehaviour
{
    // List of transparent models to me matched
    public GameObject[] Floors;
    public GameObject[] Columns;
    public GameObject[] Walls;
    public GameObject[] Railings;
    public GameObject Roof;


    public GameObject messageCanvas;
    private TextMeshProUGUI text;
    private readonly float Z_OFFSET = 0.8f;
    private bool correctObject;
    private bool[] correctPosition;
    private bool firstTime = false;
    private const float MAX_DISTANCE = 0.003f;
    private static int inCorrectPosition = 0; // static so there is a lock on it
    private float LastUpdate = 0f;
    private const float TimeToUpdate = 5f;
    private Camera mainCamera;
    private enum Construction
    {
        Floors = 0,
        Railings = 1,
        Column = 2,
        Walls =3,
        Roof = 4
    }

    Construction construction;

 

    void Start()
    {
        
        construction = Construction.Floors;
        //phase = Phase.Start;
        //text = messageCanvas.GetComponent<TextMeshProUGUI>();
        firstTime = true;
        mainCamera = Camera.main;
        
    }

    private void OnEnable()
    {
        UpdatePositionHub.ObjectSelectedEvent += CheckCorrectObject;
        UpdatePositionHub.ObjectDeselectedEvent += CheckCorrectPosition;
        ClientManager.levelUploaded += DisableScript;
    }

    private void DisableScript(string levelName)
    {
        string comparing= LoadLevel.Levels.ThirdLevel.ToString();
        if (levelName.CompareTo(comparing) == 0)
        {
            this.enabled = false;
        }
    }

    private void OnDisable()
    {
        ClientManager.levelUploaded -= DisableScript;
        UpdatePositionHub.ObjectSelectedEvent -= CheckCorrectObject;
        UpdatePositionHub.ObjectDeselectedEvent -= CheckCorrectPosition;
    }


    /// <summary>
    /// Check correct position if the object is deselected
    /// </summary>
    /// <param name="currentSelected">Ttransform of the object currently selected</param>
    private void CheckCorrectPosition(Transform currentSelected)
    {
        if (correctObject)
        {
            UpdateConstructionPhase(currentSelected);
            correctObject = false;
            
        }
        else
        {
            messageCanvas.gameObject.SetActive(false);
        }
    }

    private void UpdateConstructionPhase(Transform currentSelected)
    {
        switch (construction)
        {
            case Construction.Floors:
              
                CheckPosition(currentSelected, ref Floors, Construction.Railings);
                break;
            case Construction.Railings:
                CheckPosition(currentSelected, ref Railings,Construction.Column);

                break;
            case Construction.Column:
                CheckPosition(currentSelected,  ref Columns, Construction.Walls);
                break;
            case Construction.Walls:
                CheckPosition(currentSelected, ref Walls, Construction.Roof);

                break;
            case Construction.Roof:
              //nothing to do here
                break;


        }
    }

    private void CheckPosition(Transform currentSelected, ref GameObject[] gameObjects, Construction nextConstruction)
    {
        //INIT VECTOR TO KEEP TRACK OF THE STATE
        int length = gameObjects.Count();
        //int count = 0;
        //bool currentStatus;
        float distance;
        if (firstTime)
        {
            correctPosition = new bool[length];
            // check if the selected is in the correct position and set the right bool value, otherwise set all to false
            for (int i = 0; i < length; i++)
            {
                if (gameObjects[i].CompareTag(currentSelected.tag))
                {
                    // check position
                    distance = Vector3.SqrMagnitude(gameObjects[i].transform.position - currentSelected.transform.position);
                    if (distance < MAX_DISTANCE)
                    {
                        correctPosition[i] = true;
                        inCorrectPosition++;
                        break;
                    }
                    else
                    {
                        correctPosition[i] = false;
                    }
                }
                else
                {
                    correctPosition[i] = false;

                }

            }
            firstTime = false;

        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                if (correctPosition[i])
                {
                    // already in correct position
                    continue;
                }
                else
                {
                    if (gameObjects[i].CompareTag(currentSelected.tag))
                    {
                        // check position
                        distance = Vector3.SqrMagnitude(gameObjects[i].transform.position - currentSelected.transform.position);
                        if (distance < MAX_DISTANCE)
                        {
                            correctPosition[i] = true;
                            inCorrectPosition++;
                            
                            if (inCorrectPosition == length)
                            {
                                construction = nextConstruction;
                                firstTime = true;
                                inCorrectPosition = 0;
                            }
                            break;
                        }
                        else
                        {
                            correctPosition[i] = false;
                        }
                    }
                    else
                    {
                        correctPosition[i] = false;

                    }
                }


            }
        }
    }

    private void CheckCorrectObject(Transform currentSelected)
    {
         correctObject = false;

        switch (construction)
        {
            case Construction.Floors:

                correctObject = CheckTag(currentSelected, correctObject,Floors);

                // if it s not the correct object selected make something appear

                if (!correctObject)
                {
                    PopUpCanvas(currentSelected);
                    // open global canvas saying ... 
                    // deselect the object ? 
                }
               
                break;
            case Construction.Railings:
                correctObject = CheckTag(currentSelected, correctObject, Railings[0]);

                if (!correctObject)
                {
                    PopUpCanvas(currentSelected);
                    // open global canvas saying ... 
                    // deselect the object ? 
                }

                break;
            case Construction.Column:
                correctObject = CheckTag(currentSelected, correctObject, Columns[0]);

                if (!correctObject)
                {
                    PopUpCanvas(currentSelected);
                    // open global canvas saying ... 
                    // deselect the object ? 
                }
                break;
            case Construction.Walls:

                correctObject = CheckTag(currentSelected, correctObject, Walls[0]);

                if (!correctObject)
                {
                    PopUpCanvas(currentSelected);
                    // open global canvas saying ... 
                    // deselect the object ? 
                }

                break;
            case Construction.Roof:
                this.enabled = false;

                break;

        }
    }

    private void PopUpCanvas(Transform currentSelected)
    {
        messageCanvas.SetActive(true);
     
        float distFromSelected = 0.1f;
        Vector3 dirToCamera = currentSelected.position - mainCamera.transform.position;
        messageCanvas.transform.position = currentSelected.position - (dirToCamera.normalized * distFromSelected);        
        messageCanvas.transform.LookAt(mainCamera.transform);
        messageCanvas.transform.forward = -1 * messageCanvas.transform.forward.normalized;
        //messageCanvas.transform.Rotate(messageCanvas.transform.up, 180);
        
        //messageCanvas.SetActive(true);
        //messageCanvas.transform.position = currentSelected.position;
        //messageCanvas.transform.parent = currentSelected;
        //messageCanvas.transform.rotation = Quaternion.identity;
        //messageCanvas.transform.LookAt(mainCamera.transform.forward,Vector3.up);

    }

    private bool CheckTag(Transform obj, bool correctObject, GameObject m_gameObject)
    {
        if (m_gameObject.CompareTag(obj.tag))
        {
            return true;
           
        }
        return false;
    }

    private bool CheckTag(Transform obj, bool correctObject, GameObject[] objects)
    {
        foreach (var m_gameObject in objects)
        {
            if (m_gameObject.CompareTag(obj.tag))
            {
                correctObject = true;
                break;
            }

        }

        return correctObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - LastUpdate > TimeToUpdate)
        {
            LastUpdate = Time.time;
            switch (construction)
            {
                case Construction.Floors:
                    // check if the next phase can be triggered
                    CheckForUpdates(ref Floors, Construction.Railings);
                    break;
                case Construction.Railings:
                    CheckForUpdates(ref Railings, Construction.Column);
                    break;
                case Construction.Column:
                    CheckForUpdates(ref Columns, Construction.Walls);
                    break;
                case Construction.Walls:
                    CheckForUpdates(ref Walls, Construction.Roof);

                    break;
                case Construction.Roof:

                    this.enabled = false;
                    break;


            }
        }
        //messageCanvas.transform.LookAt(mainCamera.transform.forward);

    }

    
    /// <summary>
    /// Function Called by Update to move the tutorial in the correct
    /// Construction phase in case someone else positioned the element in the correct order
    /// </summary>
    /// <param name="currentObjects">Model of the objects to dock with the transparent ones</param>
    /// <param name="nextPhase">Phase you would like to move next </param>
    private void CheckForUpdates(ref GameObject[] currentObjects, Construction nextPhase)
    {
        if (inCorrectPosition == currentObjects.Count())
        {
            // go to the next phase
            construction = nextPhase;
        }
        else
        {

            foreach (var transparentModel in currentObjects)
            {
                if (ClientManager.Instance.objects.TryGetValue(transparentModel.name, out GameObject currentSelected))
                {
                    UpdateConstructionPhase(currentSelected.transform);
                }
                else
                {
                    Debug.LogError("This shouldn't happen!The value " + transparentModel.name + " wasnt present in the dictionary");
                }
            }

        }
    }
}
