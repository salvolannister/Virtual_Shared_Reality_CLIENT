using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script has to be attached to the cam in order to retrive
/// the information and send them to the other users
/// </summary>
public class SyncPosition : MonoBehaviour
{
    const float MIN_CHANGE = 0.1f;
   
    public UpdatePositionHub UpPos;
    public Transform origin;
    public GameObject avatar;
    public float scale;
    public Color color;


    private float lastTime = 0;
    private const float FORCE_UPDATE = 0.033f; //30 fps
    private Quaternion lastRotation;
    Vector3 lastPosition;

#if UNITY_ANDROID
    private float OFFSET_Y = 0.1f;
    private float OFFSET_X = 0.4f;
#endif

    // android needs some offset in order to have the model on the real tablet
    public void InitUpPos()
    {

        UpPos =(UpdatePositionHub) UpdatePositionHub.Instance;
       
    }


    protected virtual void Awake()
    {
        
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        avatar = new GameObject("Avatar");

    }

    private void Start()
    {
        InitUpPos();
    }

    void Update()
    {
        if (Time.time - lastTime > FORCE_UPDATE)
        {
            if (!transform.position.Equals(lastPosition))
            {
                if (ChangeOnPosition())
                {
                    ChangeAvatarTransform();
                    UpPos.ReactToUpdate(avatar);
                    lastPosition = transform.position;
                    lastRotation = transform.rotation;
                }
                else if (ChangeOnRotation())
                {

                    ChangeAvatarTransform();
                    UpPos.ReactToUpdate(avatar);
                    lastRotation = transform.rotation;
                    lastPosition = transform.position;
                }
            }
            else if (!transform.rotation.Equals(lastRotation))
            {

                if (ChangeOnRotation())
                {
                    ChangeAvatarTransform();
                    UpPos.ReactToUpdate(avatar);
                    lastRotation = transform.rotation;
                    lastPosition = transform.position;
                }

            }
        }
       
    }

    public void ChangeAvatarTransform()
    {
        avatar.transform.parent = null;
        avatar.transform.position = gameObject.transform.position;
        avatar.transform.rotation = gameObject.transform.rotation;
#if UNITY_ANDROID
        Vector3 tabletPosition = avatar.transform.position;
        tabletPosition.y -= OFFSET_Y;
        tabletPosition.x += OFFSET_X;
        avatar.transform.position = tabletPosition;

#endif

        /* maybe there will be problem with the scale */
        avatar.transform.parent = origin;

    }

    bool ChangeOnPosition()
    {
        Vector3 offset = lastPosition - transform.position;
        float sqrLen = offset.sqrMagnitude;
        if (sqrLen > MIN_CHANGE)
        {
            
            return true;
        }
        return false;
    }
    bool ChangeOnRotation()
    {
        if (Quaternion.Angle(transform.rotation, lastRotation) >= 2)
            return true;
        else return false;
    }
}
