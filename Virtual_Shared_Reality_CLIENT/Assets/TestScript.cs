using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    public UpdatePositionHub UpPos;
    public float radius = 0f;
    public PropManager pm;
    public float isTime = 0.033f;
    private float time = 0;
    private Vector3 remainingTranslation;
    private Quaternion remainingRotation;
    public float factor = 10;
    private Quaternion finalRotation;
    private float dampeningf;
    public bool one = true;
    public float valuee = 10;
    void Start()
    {
        pm = gameObject.GetComponent<PropManager>();
         finalRotation = Quaternion.AngleAxis(40, Vector3.forward);
    }

    // Update is called once per frame
    void Update()
    {

        var oldPosition = transform.localPosition;
        var oldRotation = transform.localRotation;

        if (transform.localRotation != finalRotation)
        {
            transform.localRotation *= Quaternion.AngleAxis(valuee, Vector3.forward);
            
                // Increment
            remainingTranslation += transform.localPosition - oldPosition;
            remainingRotation *= Quaternion.Inverse(oldRotation) * transform.localRotation;


            dampeningf = LeanTouch.GetDampenFactor(factor, Time.deltaTime);
            // Dampen remainingDelta
            /*lerp returns a value between (b-a)* factor, f=1 -> b*/
            //var newRemainingTranslation = Vector3.Lerp(remainingTranslation, Vector3.zero, 1);
            //var newRemainingRotation = Quaternion.Slerp(remainingRotation, Quaternion.identity, 1);

            //// Shift this transform by the change in delta
            //transform.localPosition = oldPosition + remainingTranslation - newRemainingTranslation;
            ////transform.localPosition = oldPosition + remainingTranslation;
            ///*moltiplicare per Q.I(rem..) rallenta di un po' la rotazione*/
            //transform.localRotation = oldRotation * Quaternion.Inverse(newRemainingRotation) * remainingRotation;

            if (time >= isTime)
            {
                time = 0;

                if (radius == 0f) radius =(float) pm.radius;
                if (UpPos.CheckCollision( this.gameObject))
                {
                    Debug.Log("correct");
                    //transform.rotation *= Quaternion.Inverse(transform.rotation);
                }
                else
                {


                }
            }
            else
            {
                time += Time.deltaTime;
            }
        }
        else
        {
            
        }
    }
}
