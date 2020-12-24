
using Assets;
using Lean.Touch;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class TutorialManager : MonoSingleton<TutorialManager>
{

    public  GameObject canvas;
  
    public GameObject Model;
    /* Group fo transparent models */
    public GameObject TY;
    public GameObject TX; 
    public GameObject R; 
    public GameObject S;

    private GameObject info;
    private Vector3 StartLocalTransform;
    private Quaternion StartLocalRotation;
    private Vector3 OriginalScale;
    private List<GameObject> tut_models;
    TextMeshProUGUI text;
    Button buttonNext;
    private Outline ModelOutline;
#if UNITY_ANDROID
    private Animation Animation;
    private LeanDragTranslate leanDragTranslate;
    private LeanPinchScale leanPinchScale;
    private PinchTwistToggle pinchTwist;
    ButtonManagerAndroid buttonManager;
    private float originalPinchTreshold;
#else
    ButtonManagerHmd buttonManager;
    public GameObject ControllerInput;

#endif
    enum Phase
    {
        Start = 0,
        First = 1,
        Second =2,
        Third = 3
    }

    private Phase phase;

    public enum Tutorials
    {
        translation = 1,
        rotation = 2,
        scale = 3,
        end = 4
    }
    public Tutorials tutorial;
    private float THRESHOLD = 0.8f;
    private void OnEnable()
    {
        SetInfoCanvas();
        // Save original position
        StartLocalRotation = Model.transform.localRotation;
        StartLocalTransform = Model.transform.localPosition;
        OriginalScale = Model.transform.localScale;
        tutorial = Tutorials.translation;
        ModelOutline = Model.GetComponent<Outline>();
#if UNITY_ANDROID
        leanDragTranslate = Model.GetComponent<LeanDragTranslate>();
        leanPinchScale = Model.GetComponent<LeanPinchScale>();
        pinchTwist = Model.GetComponent<PinchTwistToggle>();
      
        originalPinchTreshold = pinchTwist.PinchThreshold;
        pinchTwist.PinchThreshold = 10; // disable scale function
       
        
        buttonManager = FindObjectOfType<ButtonManagerAndroid>();
#else
        buttonManager = FindObjectOfType<ButtonManagerHmd>();

#endif
    }
 

    public void  SetInfoCanvas()
    {
       
        info = GetComponentInChildren<GraphicRaycaster>().gameObject; // get canvas element
      
        text = info.GetComponentInChildren<TextMeshProUGUI>();
        buttonNext = info.GetComponentInChildren<Button>();
#if UNITY_ANDROID
        Animation = info.GetComponentInChildren<Animation>();
        Animation.gameObject.SetActive(false);
#else
        Canvas popUPCanvas = info.GetComponent<Canvas>();
        if (popUPCanvas)
        {
            GameObject pointerCamera = GameObject.FindGameObjectWithTag("PointerCamera");
            popUPCanvas.worldCamera =pointerCamera.GetComponent<Camera>();
        }        
        else
            Debug.Log("POP UP Canvas not found");
#endif

    }

   

    /// <summary>
    /// Check if the tutorial phase was OK, 
    /// then set the configuration to the next part calling OpenTutorialInfo or makes repeat the previous part
    /// </summary>
    /// <param name="tot_point">Point got in Calculate Difference</param>
    public void ContinueTutorial(float tot_point)
    {   
             
        info.SetActive(true);

        //tot_point = 1;
        //Debug.Log("TOT_POINT " + tot_point);
        if (tot_point < THRESHOLD)
            text.text = TutorialMessages.TryAgainText(tot_point, THRESHOLD);
        else
        {
            //SUCCESS
            switch (tutorial)
            {
                case TutorialManager.Tutorials.translation:
                    {

                        switch (phase)
                        {
                            case Phase.Start:



#if UNITY_ANDROID
                                phase = Phase.First;
#else
                                phase = Phase.Start;
                                tutorial = Tutorials.scale;
#endif
                                OpenTutorialInfo();


                                break;
                            case Phase.First:



                                TX.SetActive(false);
                                // Enable first element for rotation

                                phase = Phase.Start;
                                tutorial = Tutorials.rotation;

                                OpenTutorialInfo();

                                break;

                        }


                        break;
                    }
                case TutorialManager.Tutorials.rotation:
                    {
                        // there could be opne model and position read by file

                        switch (phase)
                        {
                            case Phase.Start:

                                phase = Phase.First;
                                OpenTutorialInfo();


                                break;

                            case Phase.Third:

                                phase = Phase.Start;
                                tutorial = Tutorials.scale;
                                OpenTutorialInfo();
                                //ResetTransform(Model);

                                break;

                        }
                        buttonNext.onClick.AddListener(Close);

                        break;
                    }
                case TutorialManager.Tutorials.scale:
                    {

                        tutorial = Tutorials.end;
                        OpenTutorialInfo();

                        break;
                    }



            }
        }      
    }

    /// <summary>
    /// Opens the Canvas with just the information about the specific part of the tutorial
    /// </summary>
    
    public void OpenTutorialInfo()
    {

        info.SetActive(true);
        buttonNext.onClick.AddListener(Close);
        Model.SetActive(false);

        switch (tutorial)
        {
            case TutorialManager.Tutorials.translation:
                {
                 
                    switch (phase)
                    {
                        case Phase.Start:

                            text.text = TutorialMessages.intro;
                            break;
                        case Phase.First:
#if UNITY_ANDROID
                            TY.SetActive(false);

                            ResetTransform(Model);
                            text.text = TutorialMessages.translationXText;
#endif
                            break;
                     
                    }

                    break;
                }
            case TutorialManager.Tutorials.rotation:
                {
#if UNITY_ANDROID
                    Animation.gameObject.SetActive(true);
                    Animation.gameObject.transform.rotation = Quaternion.identity;
#endif
                    switch (phase)
                    {
                        case Phase.Start:

#if UNITY_ANDROID
                            text.text = TutorialMessages.twistText;
                            Animation.Play("TWIST");
                            
#endif
                            break;
                        case Phase.First:
                            // ROTATION AROUND Y
#if UNITY_ANDROID

                            text.text = TutorialMessages.rotationYText;
                            Animation.Play("RotationY");
#endif
                            break;
                        case Phase.Second:
                            // ROTATION AROUND X
#if UNITY_ANDROID
                            text.text = TutorialMessages.rotationZText;
                            Animation.Play("RotationX");
#endif
                            break;
                        case Phase.Third:
#if UNITY_ANDROID
 
                            Animation.gameObject.SetActive(false);
                            text.text = TutorialMessages.rotationTaskText;
#endif
                            ResetTransform(Model);
                            break;
                    }
                    break;
                }
            case TutorialManager.Tutorials.scale:
                {

                    text.text = TutorialMessages.scaleText;
#if UNITY_ANDROID
                    R.SetActive(false);
                    Animation.gameObject.SetActive(true);
                    Animation.Play("Zoom");
#else
                    TY.SetActive(false);
#endif
                    break;
                }
            case TutorialManager.Tutorials.end:
                {
#if UNITY_ANDROID
                    Animation.gameObject.SetActive(false);

#else
                    ControllerInput.SetActive(false);
#endif
                    S.SetActive(false);
                    Model.SetActive(false);
                    buttonNext.onClick.AddListener(EndTutorial);
                    text.text = TutorialMessages.tutorialEnd;
                    
                    break;
                }


        }
    }

#if UNITY_ANDROID
    private void CheckAxisRotation(UnityEngine.Vector2 delta)
    {
        float deltaX = Mathf.Abs(delta.x);
        float deltaY = Mathf.Abs(delta.y);
      if (deltaX > deltaY + deltaX/6 )
        {
            // ROTATION AROUND Y
            if(phase == Phase.First && tutorial == Tutorials.rotation)
            {
               leanDragTranslate.OnDragParallel.RemoveListener(CheckAxisRotation); 
                phase = Phase.Second;
                
                OpenTutorialInfo();
            }
       
        }
        else if(deltaY > deltaX + deltaY / 6)
        {
            //ROTATION AROUND X
            if (phase == Phase.Second && tutorial == Tutorials.rotation )
            {
                leanDragTranslate.OnDragParallel.RemoveListener(CheckAxisRotation);
                phase = Phase.Third;
                OpenTutorialInfo();
            }
        }
    }
#endif

    private void ResetTransform(GameObject gameObject)
    {
        gameObject.transform.localPosition = StartLocalTransform;
        gameObject.transform.localRotation = StartLocalRotation;
    }
    private void Close()
    {

        //buttonNext.onClick.RemoveAllListeners();
#if UNITY_ANDROID
        if (Animation.isPlaying)
             Animation.Stop();

         if (tutorial == Tutorials.rotation  && ( phase == Phase.First || phase == Phase.Second))
        {
            leanDragTranslate.OnDragParallel.AddListener(CheckAxisRotation);
        }
#endif
        info.SetActive(false);
        buttonManager.ButtonTurnOff();

        // Alla chiusura di una fase apri il modello opportuno

        ActivateLastThings();
    }

    /// <summary>
    /// Activate models and listeners after click on Continue
    /// </summary>
    private void ActivateLastThings()
    {

        Model.SetActive(true);
        ModelOutline.enabled = false;
        switch (tutorial)
        {
            case TutorialManager.Tutorials.translation:
                {

                    switch (phase)
                    {
                        case Phase.Start:

                            TY.SetActive(true);
                            break;
                        case Phase.First:
#if UNITY_ANDROID
                            TX.SetActive(true);
                            
#endif
                            break;

                    }

                    break;
                }
            case TutorialManager.Tutorials.rotation:
                {
                   
                    switch (phase)
                    {
                        case Phase.Start:

#if UNITY_ANDROID
                            
                            Model.GetComponent<LeanTwistRotate>().TwistPerformed += OpenTutorialInfo;
                            phase = Phase.First;
#endif
                            break;
#if UNITY_ANDROID
                        case Phase.First :
                            leanDragTranslate.OnDragParallel.AddListener(CheckAxisRotation);
                            break;

                        case Phase.Second:
                            leanDragTranslate.OnDragParallel.AddListener(CheckAxisRotation);
                            break;
#endif
                        case Phase.Third:
#if UNITY_ANDROID
                            R.SetActive(true);
                          
#endif
                            ResetTransform(Model);
                            break;
                    }
                    break;
                }
            case TutorialManager.Tutorials.scale:
                {
                    ResetTransform(Model);

                    S.SetActive(true);
#if UNITY_ANDROID
                    pinchTwist.PinchThreshold = originalPinchTreshold; // enables also scale
#endif

                    break;
                }

        }
    }

    public void EndTutorial()
    {
        canvas.SetActive(true);
        ClientManager.Instance.TUTORIAL = false;
        Destroy(this.gameObject);
      
    }

   
}
