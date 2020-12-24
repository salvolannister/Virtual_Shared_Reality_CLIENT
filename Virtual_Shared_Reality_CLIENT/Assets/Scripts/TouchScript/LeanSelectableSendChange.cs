using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Lean.Touch
{
    /// <summary>This component allows you to change the color of the Renderer (e.g. MeshRenderer) attached to the current GameObject when selected.</summary>
    [RequireComponent(typeof(GameObject))]
   
    public class LeanSelectableSendChange : LeanSelectableBehaviour
    {
        private GameObject movingObj;
        public GameObject scheleton;
        public UpdatePositionHub UpPos;
        private bool moving = false;
        private bool owner;
        public LeanFingerFilter Use = new LeanFingerFilter(true);


        protected virtual void Awake()
		{

            UpPos = (UpdatePositionHub)UpdatePositionHub.Instance;
                if (UpPos == null)
                {
                    Debug.Log("Can't find the singleton UpdatePositionHub" );
                   
                }   
		}

		protected override void OnSelect(LeanFinger finger)
		{
           
            owner = UpPos.TryMove( gameObject);
            if (owner)
            { 
                moving = true;
            }

            
		}

		protected override void OnDeselect()
		{
            moving = false;
            if (owner)
            {
                if (!UpPos.FreeObject(gameObject)) 
                    Debug.Log("cant stop this!!");
                owner = false;
            }

        }

     

        private void Update()
        {
            
            if (moving)
            {
                var fingers = Use.GetFingers();
                var screenDelta = LeanGesture.GetScreenDelta(fingers);
                if (screenDelta != Vector2.zero)
                    SendMovement();

            }
               
        }

        private void SendMovement()
        {
            if (movingObj == null)
            {
                movingObj = this.gameObject;
           
            }
            else
            {
              if(!ClientManager.Instance.TUTORIAL)
                UpPos.ReactToUpdate(movingObj);
            }     
          
        }
	}
}