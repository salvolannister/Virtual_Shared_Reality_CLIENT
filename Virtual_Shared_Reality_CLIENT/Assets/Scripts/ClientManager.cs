using Microsoft.AspNetCore.SignalR.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Extensions.DependencyInjection;
using SignalR;
using System;
using Lean.Touch;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Analytics;
using SharedClass;
using Vuforia;
using UnityEngine.Events;

/* this class start the connection to the HUB, signs to different calls in order 
 * to send and receive information from other users
 * and create the different objects */

public class ClientManager : MonoSingleton<ClientManager>
{
    public long SessionID ;
    [Tooltip("WebApplication Server Url")]
    public string signalRUrl = "http://192.168.1.189:51798/ChatHub";
    public bool _CONNECTED = false;
    [Space,Tooltip("Put 1 for HMD and 0 for Desktop")]
    public int debug;
    public bool TUTORIAL = false;
    private readonly string slamClip ="Audio/slam_sound" ;
    private HubConnection _hubConnection;
    private enum View { desktop, android, hmd };
    private Primitive primitive;
    public PrimitiveHolder primitives = new PrimitiveHolder();
    public GameObject origin;
    private string SceneToLoad;
    private bool init = true;
    private View device;
    private int userId  = -1; //ownership ID
    private UpdatePositionHub UpPos;
    public GameObject Avatar;
    public Dictionary<string, GameObject > objects = new Dictionary<string, GameObject>();
    public static Action<string> levelUploaded;
    SyncPosition AvatarUpdater;
    //public delegate void ClientManagerEvent(int value);
    //public static event ClientManagerEvent GetTeamId;

#if UNITY_ANDROID
    private bool master = true;
    public TestModalWindow TMW;
    /*scale factor dependent on the size of the image target*/
    public  float _scaleFactor = 3.367003f; // A4

    //public float _scaleFactor = 2.3809f; // A3 1/paper_height

#endif



    /*makes start a coroutine*/
    IEnumerator Start()
    {
        /*Loads the android scene or the HMD scene*/
        #region LoadScene    

#if UNITY_ANDROID
                SceneToLoad ="TelephoneScene";
                device = View.android;
        
#else
        if (debug == 1)
            device = View.hmd;
        else
            device = View.desktop;

        if (device == View.desktop)
        {
            SceneToLoad = "DesktopScene";
        }
        else
        {
            SceneToLoad = "HMDScene";
        }

#endif
        SceneManager.LoadScene(SceneToLoad, LoadSceneMode.Additive);
        yield return null;

        #endregion

        
        #region Debug

        //Primitive pr = new Primitive
        //{
        //    Name = "try",
        //    Shape = "try",
        //    X = 0.12f,
        //    Y = 0.2f,
        //    Z = 0.04f,
        //    Scale = true,
        //    Sx = 0.15f,
        //    Sy = 0.15f,
        //    Sz = 0.15f
        //};

        //Primitive ar = new Primitive
        //{
        //    Name = "due",
        //    Shape = "cube",
        //    X = 0,
        //    Y = 0.5f,
        //    Z = 0,
        //    Id = -1,
        //    Scale = true,
        //    Sx = 0.15f,
        //    Sy = 0.15f,
        //    Sz = 0.25f
        //};
        //primitives.Add("hello", pr);
        //primitives.Add("due", ar);
        #endregion
    }



    public void StartConnection()
    {
        origin = GameObject.FindWithTag("Origin");
        if (origin == null)
        {
            Debug.LogError("Origin not found ");
        }



#if UNITY_ANDROID
       
                GameObject Manager = GameObject.Find("Manager");
                TMW = Manager.GetComponent<TestModalWindow>();
#endif

        RegisterToServerCalls();

        UpPos = (UpdatePositionHub)UpdatePositionHub.Instance;

        #region setAvatar
        AvatarUpdater = (SyncPosition)GameObject.FindObjectOfType(typeof(SyncPosition));
        AvatarUpdater.enabled = false;
        Avatar = AvatarUpdater.gameObject;
        AvatarUpdater.origin = origin.transform;
        AvatarUpdater.ChangeAvatarTransform();
        Primitive avatarInfo = UpPos.GetPrimitive(AvatarUpdater.avatar);
        avatarInfo.Shape = device.ToString();

        RegisterToServer(avatarInfo);
        AvatarUpdater.enabled = true;
        #endregion

        _CONNECTED = true;
    }

    public void CallServerForPrimitives()
    {
        _hubConnection.InvokeAsync("GetPrimitives");
    }

    /// <summary>
    /// Gets the identifier for this session, retrieve the avatar Id from the server and
    /// sets it up in the OwnerManager to handle the transforming ownership
    /// </summary>
    /// <param name="avatarInfo"></param>
    public async void RegisterToServer(Primitive avatarInfo)
    {
        try
        {
            /*retrive unity session id*/
            SessionID = AnalyticsSessionInfo.sessionId;

            /*get string with sessionId + userId*/
            string setup = await _hubConnection.InvokeAsync<string>("GetInfoDirectlyAsync", avatarInfo, SessionID);
            /*p name is then changed to the ID number in webApplicatin*/
            
            string[] content = setup.Split(',');
            userId = int.Parse(content[0]);
            UpPos.ownershipID = userId;
            OwnerManager.myID = userId;
            string id = content[0];
            
            //Debug.Log("osID is " + id);
          
            SessionID = long.Parse(content[1]);

            /*set information in the log manager*/
            ObjectLogManager.sessionId = SessionID;
            ObjectLogManager.id = userId;
             
        }
        catch (Exception e)
        {
            Debug.Log("error " + e.ToString());
        }
       
    }




    private void ChangePosition(Primitive obj)
    {
        primitive = obj;
           
    }


    void UpdatePrimitive(Primitive p)
    {
        primitive = p;
    }
    /// <summary>
    /// Remove the avatar from the object list, clear the list and a new one from the server then starts the level
    /// </summary>
    /// <param name="pH"></param>
    /// <param name="levelName"></param>
    void GetPrimitives(PrimitiveHolder pH, string levelName)
    {
        if(primitives.Count() != 0)
        {
           
            /* delete old primitives and gameObjects
             * avoiding to delete camera (osID)*/
            objects.Remove(userId.ToString());
            List<Primitive> list = primitives.getList();
            foreach(Primitive prim in list)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(DestroyObjMainThread(prim.Name));
            }
            
            
            /* should I add myseld OSID to objects ? */
        }
      
        primitives = pH;
        init = true;
        levelUploaded?.Invoke(levelName);
        GetTotalScoreGeneral.Instance.StartLevel(levelName);
        AvatarUpdater.enabled = true;
    }

    /// <summary>
    /// Register the app to the calls of the server
    /// </summary>
    private void RegisterToServerCalls()
    {

        /*after loading the scene*/

        if (_hubConnection == null)
        {   
            _hubConnection = OpenConnection.Instance.StartSignalR(signalRUrl);
            if (_hubConnection == null)
            {
                #if UNITY_ANDROID
                            TMW.TestYNC("hub connection is null ");                               
                #endif
            }

            _hubConnection.On<Primitive>("OnAddPrimitive", UpdatePrimitive);
            _hubConnection.On<Primitive>("OnChangePosition", ChangePosition);
            _hubConnection.On<PrimitiveHolder, string>("RecivePrimitives", GetPrimitives);
            _hubConnection.On<Primitive>("OnOwnerChange", ChangeOwner);
            _hubConnection.On<string>("OnGoodByeAsync", DestroyGameObject);
            _hubConnection.On<RecordInfo>("OnSendScore", ShowScore);
            _hubConnection.On<Primitive>("OnCollision", PlaySound);
            OpenConnection.Instance.StartConnection();
            
        }
        else
        {
            Debug.Log("hub is already connected");
        }
        
  
    }

    private void PlaySound(Primitive primitive)
    {
        if (objects.TryGetValue(primitive.Name, out GameObject CollidedObj))
        {
            try
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => CollidedObj.GetComponent<AudioSource>().Play());

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

        }
        else
        {
            Debug.Log("[PlaySound]: no obj with this name " + primitive.Name);
        }
    }

    /// <summary>
    /// Stops count and show score
    /// </summary>
    /// <param name="record">Information about the score </param>
    public void ShowScore(RecordInfo record)
    {
        
        GetTotalScoreGeneral.Instance.StopCount();
        UnityMainThreadDispatcher.Instance().Enqueue(
            () => GetTotalScoreGeneral.Instance.OpenScoreCanvas(record));
    }

    public IEnumerator ChangeOwnerMainThread(GameObject currentObj, int newID)
    {
       
        if (currentObj != null)
        {
            /* never gets back from here */
            OwnerManager ownerManager = currentObj.GetComponent<OwnerManager>();

            if (newID == userId) Debug.Log("I'm the owner, this should not happen");
            else if (newID == -1)
            {
                ownerManager.FreeObject();
            }
            ownerManager.ChangeOwnerId(newID);

        }
        else
        {
            Debug.Log("The element is  null (!?)");
        }
        yield return null;
    }


    private void ChangeOwner(Primitive p)
    {
       // Debug.Log("name " + p.Name + "is changing owner"); 
        if (!objects.TryGetValue(p.Name, out GameObject currentO))
            Debug.Log("currentO is null!! in change owner");
        UnityMainThreadDispatcher.Instance().Enqueue(ChangeOwnerMainThread(currentO,p.Id));
    }

 

  
    void Update()
    {
        
        if (primitive != null)
        { 
            objects.TryGetValue(primitive.Name, out GameObject gameObject);
           
            if (gameObject == null) /* create the correspondent obj */
            {
                /* it means it doesn't exist so I have to create it */
                Debug.Log("The primitive shape chosen is " + primitive.Shape + 
                    " and is position is X: " + primitive.X + " Y:" + primitive.Y + " Z" + primitive.Z);
                CreatePrimitive(primitive);
            }
            else
            {
                
                Vector3 position = new Vector3(primitive.X, primitive.Y, primitive.Z);
                Quaternion rot = new Quaternion(primitive.RotX, primitive.RotY, primitive.RotZ, primitive.W);
              
                //if ( (!primitive.Avatar) )
                //{

                #if UNITY_ANDROID
                                position = position *_scaleFactor;
                
                #endif
                gameObject.transform.localPosition = position;
                gameObject.transform.localRotation = rot;

                /*scale changed */
                    if (primitive.Scale)
                    {

                    float x = primitive.Sx;
                    float y = primitive.Sy;
                    float z = primitive.Sz;
                    Vector3 scale = new Vector3(primitive.Sx, primitive.Sy, primitive.Sz);
                #if UNITY_ANDROID
                    scale *= _scaleFactor;
                #endif
                    gameObject.transform.localScale = scale;
                    
                    }

                //}

            }  

            primitive = null;
        }

        /*to create all the primitives  already existing
         * at the beginning of the application*/
        if (init && primitives.Count() != 0)
        {
            init = false;
            List<Primitive> list = primitives.getList();
            foreach (var currentPrimitive in list)
            {
                CreatePrimitive(currentPrimitive);   
            }
        }


      

    }

    /// <summary>
    /// Instantiate the prefab and adds the script needed to perform the tasks
    /// </summary>
    /// <param name="primitive">Object information coming from the server</param>
     private void CreatePrimitive(Primitive primitive)
    {
        
        

        /* if the object is not my avatar */
        if (primitive.Name != userId.ToString())
        {


            if (!objects.ContainsKey(primitive.Name)  && primitive.Shape!= null)
            {

                GameObject loadedObject = Instantiate(Resources.Load(primitive.Shape)) as GameObject;
                /* name of avatar are numbers, set in WebApplication */


                if (primitive.Avatar || loadedObject.CompareTag("Static"))
                {
                    AddDetails(loadedObject, primitive, false);
                } 
                else
                    AddDetails(loadedObject, primitive, true);

                objects.Add(loadedObject.name, loadedObject);
            }
            else
            {
                Debug.LogError("Put a shape in the primitive, cause is missing ");
            }

        }
        
       
      
      

    }

    /*sF is a value taken from the editor, it is used because
     * everything is scaled in proportion of the target dimension for what concern
     * the ANDROID part*/
    private void  AddDetails(GameObject gameObject, Primitive primitive, bool canMove)
    {
        
        gameObject.transform.parent = origin.transform;
        if(origin == null)
        {
            Debug.Log("Cannot relate to origin");
        }
        gameObject.name = primitive.Name;
        if (canMove)
        {

            gameObject.AddComponent<PropManagerPro>();
            OwnerManager manager = gameObject.AddComponent<OwnerManager>();

            manager.setID(primitive.Id);
          
        
#if UNITY_ANDROID
            AddTouchFunctions(gameObject);
            /* to be removed after the app is  completed*/
#elif !UNITY_ANDROID && UNITY_STANDALONE
            if (device == View.desktop)
            { 
                AddTouchFunctions(gameObject);
            }
#endif

        }
        else
        {

            /* the information about the scale is in the Prefab*/
            if (primitive.Avatar)
            {
                primitive.Scale = true;
                AvatarInformation avatarInformation = gameObject.GetComponent<AvatarInformation>();
                primitive.Sx = primitive.Sy = primitive.Sz = avatarInformation.scale;
                
                /*manage color*/
                GetColor(primitive, gameObject);
            }
        }

        Vector3 position = new Vector3(primitive.X, primitive.Y, primitive.Z);
        Vector3 scale;
        if (primitive.Scale == false)
        {
            float x = 0.2f;
            float y = 0.2f;
            float z = 0.2f;
            scale = new Vector3(x, y, z);
            primitive.Scale = true;
        }
        else
        {
            float x = primitive.Sx;
            float y = primitive.Sy;
            float z = primitive.Sz;
            scale = new Vector3(x, y, z);

        }
        Quaternion quaternion = new Quaternion(primitive.RotX, primitive.RotY, primitive.RotZ, primitive.W);
#if UNITY_ANDROID
        position *= _scaleFactor;
        scale *= _scaleFactor;
       
#endif
        gameObject.transform.localPosition = position;
        gameObject.transform.localRotation = quaternion;
        gameObject.transform.localScale = scale;
       
        return;
    }

    private void AddTouchFunctions(GameObject gameObject)
    {
        gameObject.AddComponent<LeanSelectable>();
        LeanSelectable lS = gameObject.GetComponent<LeanSelectable>();
        //lS.DeselectOnUp = true;
        LeanTwistRotate ltr = gameObject.AddComponent<LeanTwistRotate>();
        ltr.Use.RequiredSelectable = lS;
        LeanDragTranslate ldt = gameObject.AddComponent<LeanDragTranslate>();
        ldt.Use.RequiredSelectable = lS;
        LeanPinchScale lps = gameObject.AddComponent<LeanPinchScale>();
        lps.Use.RequiredSelectable = lS;
        PinchTwistToggle ptt = gameObject.AddComponent<PinchTwistToggle>();
        ptt.PinchComponent = lps;
        ptt.TwistComponent = ltr;
        ptt.RequiredSelectable = lS;
        ldt.pinchTwistToggle = ptt;
        gameObject.AddComponent<LeanSelectableSendChange>().enabled = true;
        gameObject.AddComponent<ChangeModality>();
        gameObject.AddComponent<YawAndPitch>();
      
    }

    /* to improve using enum parsing D: */
    private void GetColor(Primitive p, GameObject gameObject)
    {
        string partner =  p.Shape;
        Color color;
        
        switch (p.Color)  // Needs refactoring with Enum
        {
            case "black" :
                color = Color.black;
                break;
            case "white":
                color = Color.white;
                break;
            case "blue":
                color = Color.blue;
                break;
            case "cyan":
                color = Color.cyan;
                break;
            case "grey":
                color = Color.grey;
                break;
            case "magenta":
                color = Color.magenta;
                break;
            case "yellow":
                color = Color.yellow;
                break;
             case "red":
                color = Color.red;
                break;
             case "green":
                color = Color.green;
                break;
             default:
                Debug.Log("Color is default: Yellow");
                color = Color.yellow;
                break;

        }
        /* for now desktop and HMD are equals*/
        switch (partner)
        {
            case "android":
               GameObject child = gameObject.GetComponent<Transform>().GetChild(0).gameObject;
               child.GetComponent<MeshRenderer>().material.color = color;
                break;
            case "desktop":
                gameObject.GetComponent<MeshRenderer>().material.color = color;
                break;
            case "HMD":
                gameObject.GetComponent<MeshRenderer>().material.color = color;
                break;
        }
    }

    public void OnApplicationQuit()
    {
        /*call HubConnection and signal your departure*/
        string idString = userId.ToString();
      if(ClientManager.Instance._CONNECTED)
        _hubConnection.InvokeAsync("GoodByeAsync", idString);
    }

    void DestroyGameObject(string name)
    {

        UnityMainThreadDispatcher.Instance().Enqueue(DestroyObjMainThread(name));
        primitives.Remove(name);
       
      
    }

    public IEnumerator DestroyObjMainThread(string name)
    {

        objects.TryGetValue(name, out GameObject o);
        objects.Remove(name);
        Destroy(o);
        yield return null;
    }

    


}
