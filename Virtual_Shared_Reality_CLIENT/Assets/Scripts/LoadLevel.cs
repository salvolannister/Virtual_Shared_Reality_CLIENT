using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

/// <summary>
/// Load the level when the option is chosen in the canvases
/// </summary>
public partial class LoadLevel : MonoBehaviour
{
    public HubConnection hubConnection;
    public Button levelButton;
    public Image imgModality;
    public enum Levels { FirstLevel = 1, SecondLevel = 2, ThirdLevel = 3 , Tutorial = 4};
    public Levels current;
    public GameObject canvas;
    public GameObject InfoCanvas;
    public TextMeshProUGUI[] TextMesh;
    public string levelName;
   
    [Tooltip("Enable only if u want to join a session already started")]
    public bool EXTERNAL_CALL;
#if UNITY_ANDROID && !UNITY_STANDALONE
    public Lean.Touch.ButtonManagerAndroid ButtonScript;
#else
    public ButtonManagerHmd ButtonScript;
  
#endif
    public readonly string sufix=".txt";
    //private long SessionId;

    public void Start()
    {
        Init();
    }
    public virtual void OnEnable()
    {
       
       //GetTotalScoreGeneral.ScoreManReady += InitScoreMan;
        levelButton = this.GetComponent<Button>();
        //levelButton.onClick.AddListener(ConnectToServer);
        #if UNITY_ANDROID && !UNITY_STANDALONE
                imgModality.enabled = false;
        #endif
        if (canvas == null)
            canvas = GameObject.FindWithTag("Menu");

        if (EXTERNAL_CALL)
        {
            ClientManager.levelUploaded += AddInstruction;
        }
     

    }

    public void Init()
    {
       
        GameObject Press = GameObject.FindWithTag("Points");
        #if UNITY_ANDROID && !UNITY_STANDALONE
                 ButtonScript = Press.GetComponent<Lean.Touch.ButtonManagerAndroid>();
        #else 
                ButtonScript = Press.GetComponent<ButtonManagerHmd>();
                
        #endif

        ButtonScript.ButtonTurnOff();
    }

    //public void InitScoreMan()
    //{
    //    /*init score Man is working but after I get NULL when trying to call*/
    //    ScoreGeneralScript = GetTotalScoreGeneral.ScoreMan;

    //}


    public void OnDisable()
    {
       
      
        levelButton.onClick.RemoveListener(SetupScene);
        

        if (EXTERNAL_CALL)
        {
            ClientManager.levelUploaded -= AddInstruction;
        }
    }

    public virtual void SetupScene()
    {

        levelName = null;
        foreach (Levels level in Enum.GetValues(typeof(Levels)))
        {
            levelName = level.ToString();
            if (gameObject.CompareTag(levelName))
            {
                current = level;
                break;
            }
        }

        if(current != Levels.Tutorial)
            ConnectToServer();
         /*Check the tags of the object where the script
          * is attached and load the correspondent level*/

      

        /*load*/
        if (levelName != null)
        {
            
            ButtonScript.ButtonTurnOff();

            if (!CreateScene(levelName))
                Debug.Log("erro in loading the scene " + levelName);
            AddInstruction(levelName);

        }
        else Debug.Log("[Load level] name is null</color>");
        /*close canvas*/
        CloseCanvas();
     
        
    }

    public  void AddInstruction(string levelName)
    {

        Enum.TryParse(levelName, out current);

        /* enables the information canvas*/
        if (current == Levels.ThirdLevel || current == Levels.FirstLevel)
            UnityMainThreadDispatcher.Instance().Enqueue( () => OpenInfoCanvas(current));

    }

    public void OpenInfoCanvas(Levels current)
    {
        InfoCanvas.SetActive(true);

      

        TextMesh = InfoCanvas.transform.GetComponentsInChildren<TextMeshProUGUI>();
        if (current == Levels.ThirdLevel)
        {
            
          
            TextMesh[0].enabled = false;
            TextMesh[1].enabled = true;
        }
        else if (current == Levels.FirstLevel)
        {
            
            TextMesh[0].enabled = true;
            TextMesh[1].enabled = false;
            
        }
    }


    //private void SendSessionID()
    //{
    //    if (hubConnection == null) hubConnection = OpenConnection_2.OpenConnect.HubConnection;
    //    hubConnection.InvokeAsync("SetSessionId", SessionId);
    //}

    public void CloseCanvas()
    {
        canvas.SetActive( false);

        #if UNITY_ANDROID && !UNITY_STANDALONE
                if(imgModality!= null) imgModality.enabled = true;
        #else
       
        #endif
    }

   

    /// <summary>
    /// Ask the server to load a specific scene 
    /// </summary>
    /// <param name="fileName">Name of the object loaded</param>
    /// <returns></returns>
    public bool CreateScene(string fileName)
    {
        try
        {
            fileName = $"{fileName}{sufix}";
            if(current!= Levels.Tutorial)
                OpenConnection.Instance.HubConnection.InvokeAsync<string>("LoadScene", fileName);
            else
            {
                LoadTutorial();
            }
        }
        catch (Exception err)
        {

           Debug.Log(err.ToString());
            return false;
        }

        return true;
    }

    private void LoadTutorial()
    {
        ClientManager.Instance.TUTORIAL = true;

#if UNITY_ANDROID
        GameObject tutorial = Instantiate(Resources.Load("Tutorial")) as GameObject;
        Vector3 position = new Vector3(tutorial.transform.localPosition.x, tutorial.transform.localPosition.y, tutorial.transform.localPosition.z);
        Vector3 scale = new Vector3(tutorial.transform.localScale.x, tutorial.transform.localScale.y, tutorial.transform.localScale.z) * ClientManager.Instance._scaleFactor;
        tutorial.transform.parent = GameObject.FindWithTag("Origin").transform;

        position *= ClientManager.Instance._scaleFactor;


        tutorial.transform.localPosition = position;
        tutorial.transform.localScale = scale;
      
#else
        GameObject origin = GameObject.FindWithTag("Origin");
        GameObject tutorial = Instantiate(Resources.Load("TutorialVR"),origin.transform) as GameObject;

       
#endif

        TutorialManager tutorialManager = tutorial.GetComponent<TutorialManager>();
        tutorialManager.canvas = canvas;
        tutorialManager.OpenTutorialInfo();
    }

    public void JoinTheSession()
    {
        ClientManager.levelUploaded += AddInstruction;
        ConnectToServer();        
        ClientManager.Instance.CallServerForPrimitives();
        ButtonScript.ButtonTurnOff();        
        CloseCanvas();   

    }

    private static void ConnectToServer()
    {
        if (!ClientManager.Instance._CONNECTED)
            ClientManager.Instance.StartConnection();
    }
}
