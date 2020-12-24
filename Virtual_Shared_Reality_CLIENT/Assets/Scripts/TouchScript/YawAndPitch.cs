using Lean.Touch;
using System.Collections;
using UnityEngine;

/// <summary>
/// Used to make the object rotate around X axis and Y axis relative to the camera forward vector
/// </summary>
public class YawAndPitch : MonoBehaviour
{
    public LeanDragTranslate scriptForTranslate;
    public enum AngleType
    {
        Degrees,
        Radians
    }

    public enum MultiplierType
    {
        None,
        DeltaTime
    }

    [Tooltip("Angle type")]
    public AngleType Angle;

    [Tooltip("Angle multiplier")]
    public MultiplierType Multiplier;

    public Vector3 center;
    [Tooltip("The rotation space")]
    public Space Space = Space.World;

    [Tooltip("The first axis of rotation")]
    public Vector3 AxisA = Vector3.down;

    [Tooltip("The second axis of rotation")]
    public Vector3 AxisB = Vector3.right;

    [Tooltip("Fixed multiplier of each rotation")]
    public float AngleMultiplier = 0.45f;
   
    [Tooltip("How quickly the rotation goes to the target value")]
    public float Dampening = 10.0f;

    [System.NonSerialized]
    private Quaternion remainingDelta = Quaternion.identity;

    public Quaternion actualRotation;

    public UpdatePositionHub UpPos;

 
    public Camera Camera;
    private bool Rotating = false;
   
    

    private void OnEnable()
    {
        StartCoroutine(SubscribeToEvents());
    }

    public void Start()
    {
        UpPos = (UpdatePositionHub)UpdatePositionHub.Instance;
        Camera = LeanTouch.GetCamera(Camera.main, gameObject);
        //  center = this.gameObject.transform.GetChild(0).transform.position;
    }
    private IEnumerator SubscribeToEvents()
    {
        yield return null;

        scriptForTranslate = this.gameObject.GetComponent<LeanDragTranslate>();
        if (scriptForTranslate)
            scriptForTranslate.OnDragParallel.AddListener(Rotate);
        else
            Debug.Log("lms.OnSwipeParallel is null");
    }

    private void OnDisable()
    {
        if (scriptForTranslate != null)
            scriptForTranslate.OnDragParallel.RemoveListener(Rotate);
        else
            Debug.Log("lms.OnSwipeParallel is null");
    }
    /// <summary>
    /// This function gets called when to finger are really close and the angle between them is less than 20°
    /// </summary>
    /// <param name="delta">direction of the finger swipe</param>
    public  void Rotate(Vector2 delta)
    {
       
        Rotating = true;
        if (Angle == AngleType.Radians)
        {
            delta *= Mathf.Rad2Deg;
        }
        Debug.Log("<color=red> YAW&PITCH</color> delta.x " + delta.x + "delta.y " + delta.y);
        Quaternion newRot = Quaternion.identity;

        float deltaX = Mathf.Abs(delta.x);
        float deltaY = Mathf.Abs(delta.y);
        if (deltaX > deltaY + deltaX/8 ) //much bigger
        {
            newRot = Quaternion.AngleAxis(delta.x * AngleMultiplier, -Camera.transform.up);
            
        }
        else 
        if( deltaY > deltaX +deltaY/8) // much bigger
        {
          
             newRot = Quaternion.AngleAxis(delta.y * AngleMultiplier, Camera.transform.right);
          
        }
        transform.rotation = newRot * transform.rotation;
     
    }
  

    void Update()
    {
        

        /*makes the object move around even if the rotation is concluded
         checking the collisions*/
        if (Rotating)
        {
            Rotating = false;
            UpPos.CheckCollision(this.gameObject);
        }

        
    }

  
}
