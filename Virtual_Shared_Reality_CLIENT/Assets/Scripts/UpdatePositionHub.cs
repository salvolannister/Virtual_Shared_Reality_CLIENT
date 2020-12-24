using Microsoft.AspNetCore.SignalR.Client;
using SignalR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using UnityEngine;

/* this function will send the updated 
 * position of the element that is moving,
 * that will be communicated by another script.
 * So I will have just an instance of this script handling movements*/

//public class UpdatePositionHub: MonoSingleton<UpdatePositionHub>
public class UpdatePositionHub : MonoSingleton<UpdatePositionHub>
{
   
    
    public TestModalWindow TMW;
    public OpenConnection openConnection;
   
   
    
    private GameObject movingObj;
    public int maxNeighbours = 16;
#if UNITY_ANDROID
    private float scaleFactor = 3.367003f;
#endif 
    private float timeToSend = 0;
    private static float TIME_TO_CHECK = 0.033f; // 30 FPS
    public int ownershipID;
    public int free = -1;
    float volume;
    Collider[] neighbours;
    Collider[] myColliders;
    private float last_checked_time = 0;
    private bool collision = false;
    // private Collider[] colliders = new Collider[5];
    public bool play = true;
    /*used to ask for the Instance in the correct moment*/
    public delegate void UpdatePositionEvent();
    public static event UpdatePositionEvent UpPosReady;
    public static bool fired = false;
    public static Action<Transform> ObjectSelectedEvent;
    public static Action<Transform> ObjectDeselectedEvent;

    public override void Init()
    {
        base.Init();
     
        if (UpPosReady != null)
        {
            UpPosReady.Invoke();
            fired = true;
        }
      
    }
  

   
    void Start()
    {
        neighbours = new Collider[maxNeighbours];
#if UNITY_ANDROID
        scaleFactor = ClientManager.Instance._scaleFactor;
#endif
    }

    //private void GetTeamId(int value)
    //{
    //    m_team = value;
    //}


    #region Collision
    //Draw a circle around the object in order to debug  the CheckCollision function
    public void DrawDebugCircle(Vector3 center, float radius, int subdivision = 16)
    {
        float angle = 0;
        Vector3 firstPoint = center + radius * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), center.z);
        Vector3 precPoint = firstPoint;
        for (int i = 0; i < subdivision; i++)
        {
            angle = i * Mathf.PI * 2 / subdivision;
            Vector3 newPoint = center + radius * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), center.z);
            Debug.DrawLine(precPoint, newPoint, Color.red, .1f);
            precPoint = newPoint;
        }
        Debug.DrawLine(precPoint, firstPoint, Color.red, .1f);
    }

    /// <summary>
    /// CheckCollision for Android version
    /// </summary>
    /// <param name="MovingObject"></param>
    /// <returns></returns>
    public bool CheckCollision(GameObject MovingObject)
    {


        //  Debug.Log("in check collision");
        myColliders = MovingObject.GetComponents<Collider>(); /*in case of compound objects*/
        Collider thisCollider;
        collision = false;
        Vector3 newposition;



        for (int i = 0; i < myColliders.Length; i++)
        {
            thisCollider = myColliders[i];
            if (!thisCollider)
                return false; // nothing to do without a Collider attached
          
            float radius = getRadius(MovingObject);


            //DrawDebugCircle(newposition, radius, 32);
          
          
            newposition = MovingObject.transform.position;
            //DrawDebugCircle(newposition, radius, 32);
            int count = Physics.OverlapSphereNonAlloc(newposition, radius, neighbours);
            //  Debug.Log(count + " " + tC.name);

            for (int j = 0; j < count; ++j)
            {
                var collider = neighbours[j];

                if (myColliders.Contains(collider))
                {

                    continue; // skip ourself

                }
                //Debug.Log("collider " + collider.gameObject.name);



                Vector3 otherPosition = collider.gameObject.transform.position;
                Quaternion otherRotation = collider.gameObject.transform.rotation;

                Vector3 direction;
                float distance;

                bool overlapped = Physics.ComputePenetration(
                        thisCollider, MovingObject.transform.position, MovingObject.transform.rotation,
                        collider, otherPosition, otherRotation,
                        out direction, out distance
                    );




                // draw a line showing the depenetration direction if overlapped
                if (overlapped)
                {
                    collision = true;
                    /*move the object*/
                    // Debug.Log("<color=red>overlapped play:</color>" + play);

                    if (play)
                    {
                        MovingObject.GetComponent<AudioSource>().Play();
                        if (!ClientManager.Instance.TUTORIAL)
                            SendCollisionSound(MovingObject);
                        play = false;
                    }

                    Vector3 possiblePosition = newposition + (direction * distance);
                    MovingObject.transform.position = possiblePosition;

                    //newposition = Child.transform.position;

                }

            }


        }


        if (collision == false)
        {
         
            play = true;
        }

        return collision;

    }

    /// <summary>
    /// Reads the radius from the script PropManagerPro if present, if not set a default value
    /// </summary>
    /// <param name="MovingObject">Object in movement</param>
    /// <returns></returns>
    private static float getRadius(GameObject MovingObject)
    {
        PropManagerPro properties = MovingObject.GetComponent<PropManagerPro>();
        float radius = 0.5f;

        if (properties)
            radius = properties.GetScaledRadius();
        else
            radius = /*0.8f * MovingObject.transform.lossyScale.x;*/ 1;

        return radius;
    }


    /// <summary>
    /// Check Collision version for the head mounted display
    /// </summary>
    /// <param name="MovingObject">GameObject of which we want to check the collision</param>
    /// <param name="controlllerCollider">The collider attached to the controller</param>
    /// <returns></returns>

    public bool CheckCollision(GameObject MovingObject, Collider controlllerCollider)
    {

        
        myColliders = MovingObject.GetComponents<Collider>();
        /*in case of compound objects*/

       
        Collider thisCollider;
        collision = false;

        for (int i = 0; i < myColliders.Length; i++)
        {
            thisCollider = myColliders[i];
            float radius = getRadius(MovingObject);

            if (!thisCollider)
                return false; // nothing to do without a Collider attached
            int count = 0;
            Vector3 newposition = MovingObject.transform.position;
            //DrawDebugCircle(newposition, radius, 32);
            count = Physics.OverlapSphereNonAlloc(newposition, radius, neighbours);



            for (int j = 0; j < count; ++j)
            {
                var collider = neighbours[j];

                //if (myColliders.Contains(collider) || collider == exception)
                //collider.gameObject.CompareTag("Player")
                if (myColliders.Contains(collider) || collider == controlllerCollider)
                    continue; // skip ourself
                //Debug.Log("collider " + collider.gameObject.name);


                Vector3 otherPosition = collider.gameObject.transform.position;
                Quaternion otherRotation = collider.gameObject.transform.rotation;

                Vector3 direction;
                float distance;

                //Debug.Log("<color=red>[HUB]</color> " + "(" + tC.transform.rotation.w + ")" + tC.transform.rotation.x + ")" +
                //    "(" + tC.transform.rotation.y + ")"
                //   + "(" + tC.transform.rotation.z + ")");

                bool overlapped = Physics.ComputePenetration(
                    thisCollider, MovingObject.transform.position, MovingObject.transform.rotation,
                    collider, otherPosition, otherRotation,
                    out direction, out distance
                );




                // draw a line showing the depenetration direction if overlapped
                if (overlapped)
                {

                    //Debug.Log("overlapped ");
                    if (collider != controlllerCollider)
                    {
                        collision = true;
                        if (play)
                        {

                            MovingObject.GetComponent<AudioSource>().Play();
                            if(!ClientManager.Instance.TUTORIAL)
                                 SendCollisionSound(MovingObject);
                            play = false;
                        }
                    }


                    Vector3 possiblePosition = newposition + (direction * distance);

                    MovingObject.transform.position = possiblePosition;
                    newposition = MovingObject.transform.position;

                }

            }


        }


        if (collision == false)
        { /*enable sound again*/
            // Debug.Log("<color=green> no collision </color>");
            play = true;
        }
        return collision;

    }


    /// <summary>
    /// Send a collision sound when the object hits something and was not hitting before
    /// </summary>
    /// <param name="MovingObject">The object that we were moving </param>
    private void SendCollisionSound(GameObject MovingObject)
    {
        Primitive p = GetPrimitive(MovingObject);
        
        OpenConnection.Instance.HubConnection.InvokeAsync("Collision", p);
    }

    #endregion

    #region GetPossession
    /*checks if the object is free and then gather possess */
    public bool TryMove(GameObject ObjectSelected)
    {
        OwnerManager ownerManager = ObjectSelected.GetComponent<OwnerManager>();
        if (ownerManager != null )
        {
            if (!ClientManager.Instance.TUTORIAL)
            {
                int ID = ownerManager.getID();
             
                // subscribe to an event to get this information only once
                if (ID == free)
                {
                    /* try to get possess */
                    _ = StartChangeOwner(ObjectSelected.name, ownershipID);

                    ownerManager.ChangeOwnerId(ownershipID);
                    // Ho preso l'oggetto aveva transform tot
                    ObjectSelectedEvent?.Invoke(ObjectSelected.transform);
                    return true;

                }
                else if (ID == ownershipID) return true;
                else return false;
            }
            else
            {
                ownerManager.SetToGreen();
                return true;
            }
          
        }
        return false;

    }

    /// <summary>
    /// Return false if the object wasnt an object that could have been selected
    /// </summary>
    /// <param name="ObjectSelected"></param>
    /// <returns></returns>
    internal bool FreeObject(GameObject ObjectSelected)
    {
        OwnerManager ownerManager = ObjectSelected.GetComponent<OwnerManager>();

        if (ownerManager != null)
        {
            if (!ClientManager.Instance.TUTORIAL)
            {
                _ = StartChangeOwner(ObjectSelected.name, free);
                ObjectDeselectedEvent?.Invoke(ObjectSelected.transform);
            }
             
            ownerManager.FreeObject();
         
        

            return true;
        }

        return false;
    }

    public async Task StartChangeOwner(string name, int id)
    {

        try
        {
            await OpenConnection.Instance.HubConnection.InvokeAsync("ChangeOwnerDirectly", name, id);

        }
        catch (Exception e)
        {
            Debug.LogError("<color=red>Error  </color>" + e.ToString());
        }
    }

    #endregion
   

  



    public void ReactToUpdate(GameObject objectToUpdate)
    {
        movingObj = objectToUpdate;
    }

    void Update()
    {
        if (movingObj != null)
        {
            if (timeToSend >= TIME_TO_CHECK )
            {
                timeToSend = 0;
                Primitive p = GetPrimitive(movingObj);
                movingObj = null;
                SharePosition(p);

            }
            else
            {
                timeToSend += Time.deltaTime;

            }
            last_checked_time += Time.deltaTime;
        }

    }



    public Primitive GetPrimitive(GameObject gameObject)
    {
        string name;
        int ID;
        float sx;
        float sy;
        float sz;
        bool scale;
        bool isUserMark = gameObject.name.Equals("Avatar") ? true : false;

        if (isUserMark)
        {
            /*both cameras have  scale not affected by vuforia */
            name = ownershipID.ToString();
            ID = ownershipID;
            scale = false;
            sx = sy = sz = 0; /*just to keep quiet the compiler*/
        }
        else
        {
            scale = true;
            ID = gameObject.GetComponent<OwnerManager>().getID();
            name = gameObject.name;
           
            sx = gameObject.transform.localScale.x;
            sy = gameObject.transform.localScale.y;
            sz = gameObject.transform.localScale.z;
        }

        float x = gameObject.transform.localPosition.x;
        float y = gameObject.transform.localPosition.y;
        float z = gameObject.transform.localPosition.z;

        /* the position is rescaled, because only the android version has 
         * the value sF and the others don't know about the existence of this 
         * value */
#if UNITY_ANDROID

        if (x != 0)
            x = x / scaleFactor;
        if (y != 0)
            y = y / scaleFactor;
        if (z != 0)
            z = z / scaleFactor;
        if (scale)
        {
            if (sx != 0)
                sx = sx / scaleFactor;
            if (sy != 0)
                sy = sy / scaleFactor;
            if (sz != 0)
                sz = sz / scaleFactor;
        }
#endif

        Primitive p = new Primitive
        {
            Name = name,
            X = x,
            Y = y,
            Z = z,
            Id = ID,
            RotX = gameObject.transform.localRotation.x,
            RotY = gameObject.transform.localRotation.y,
            RotZ = gameObject.transform.localRotation.z,
            W = gameObject.transform.localRotation.w,
            Scale = scale,
            Avatar = isUserMark
            
        };

        if (scale)
        {
            p.Sx = sx;
            p.Sy = sy;
            p.Sz = sz;
        }


        return p;
    }





    private async void SharePosition(Primitive p)
    {
        await OpenConnection.Instance.HubConnection.InvokeAsync("ChangePosition", p);
    }







}
