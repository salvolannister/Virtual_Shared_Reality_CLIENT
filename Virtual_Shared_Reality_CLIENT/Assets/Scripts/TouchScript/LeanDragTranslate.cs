using UnityEngine;

namespace Lean.Touch
{
	/// <summary>This component allows you to translate the current GameObject relative to the camera using the finger drag gesture.</summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanDragTranslate")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Drag Translate")]
	public class LeanDragTranslate : MonoBehaviour
	{
		[System.Serializable] public class Vector2Event : UnityEngine.Events.UnityEvent<Vector2> { };
		public bool onPlane = false;
		private Vector3 newposition;
		//private bool m_hit;
		public UpdatePositionHub UpPos;
	
		public Vector2Event OnDragParallel = new Vector2Event();
		/// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
		public LeanFingerFilter Use = new LeanFingerFilter(true);

		/// <summary>The camera the translation will be calculated using.\n\nNone = MainCamera.</summary>
		[Tooltip("The camera the translation will be calculated using.\n\nNone = MainCamera.")]
		public Camera Camera;

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = -1.0f;

		[HideInInspector]
		[SerializeField]
		private Vector3 remainingTranslation;
		private float ParallelAngleThreshold = 20.0f;
		public PinchTwistToggle pinchTwistToggle;
		private bool isParallel = false;
		private float lastTime;
		private float FPS = 0.016f;
		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
		public void AddFinger(LeanFinger finger)
		{
			Use.AddFinger(finger);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
		public void RemoveFinger(LeanFinger finger)
		{
			Use.RemoveFinger(finger);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove all fingers.</summary>
		public void RemoveAllFingers()
		{
			Use.RemoveAllFingers();
		}
#if UNITY_EDITOR
		protected virtual void Reset()
		{
			Use.UpdateRequiredSelectable(gameObject);
		}
#endif
		protected virtual void Awake()
		{
			Use.UpdateRequiredSelectable(gameObject);
			UpPos =(UpdatePositionHub) UpdatePositionHub.Instance;
			
		}

		private void OnEnable()
		{
			LeanTouch.OnFingerUp += FingerUp;
		}

		private void OnDisable()
		{
			LeanTouch.OnFingerUp -= FingerUp;
		}
		protected virtual void Update()
		{
			
				// Store
				var oldPosition = transform.localPosition;
				// Get the fingers we want to use
				var fingers = Use.GetFingers();
			
				// Calculate the screenDelta value based on these fingers
				/* difference between starting finger and end finger*/
		    var screenDelta = LeanGesture.GetScreenDelta(fingers);

			if (screenDelta != Vector2.zero && gameObject != null )
			{
	
				if (UpPos.TryMove(gameObject)) // without I can move the object 
				{
					if (fingers.Count == 2)
					{

						/* calculate the angle between two fingers*/
						
						float angle = Vector2.Angle(fingers[0].SwipeScaledDelta, fingers[1].SwipeScaledDelta);



						if (angle < ParallelAngleThreshold && angle != 0) // is a parallel swipe, remove != 0 to debug 
						//if (angle < ParallelAngleThreshold)
						{
							
							// Disabling Scale and Rotation aroung Z axis
							pinchTwistToggle.TwistComponent.enabled = false;
							pinchTwistToggle.PinchComponent.enabled = false;
							pinchTwistToggle.enabled = false;

							isParallel = true;
							OnDragParallel?.Invoke(screenDelta*LeanTouch.ScalingFactor);

						}
						else
						{
							
							pinchTwistToggle.enabled = true;
							isParallel = false;
						}

					}
					else
					{

						// Perform the translation
						if (transform is RectTransform)
						{
							TranslateUI(screenDelta);
						}
						else
						{
							Translate(screenDelta);
						}
					}
			}
		}

			

		}

		

		
		private void TranslateUI(Vector2 screenDelta)
		{
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			
			// Screen position of the transform
			var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, transform.position);

			// Add the deltaPosition
			screenPoint += screenDelta;

			// Convert back to world space
			var worldPoint = default(Vector3);

			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform, screenPoint, camera, out worldPoint) == true)
			{

				transform.position = worldPoint;
			}
		}

		private void Translate(Vector2 screenDelta)
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);
			Vector3 fixedPosition = transform.localPosition;
			/*really moves the object*/
			if (camera != null)
			{
				// Screen position of the transform
				
				
	
				var screenPoint = camera.WorldToScreenPoint(transform.position);

				
				// Add the deltaPosition
				screenPoint += (Vector3)screenDelta;
				
				// Convert back to world space
				newposition = camera.ScreenToWorldPoint(screenPoint);

				transform.position = newposition;
				if (onPlane)
				{
					/* moves the object only on the plane*/
					fixedPosition.x = transform.localPosition.x;
					fixedPosition.z = transform.localPosition.z;
					
				}
				else
				{
					/*moves the object up/down*/
					fixedPosition.y = transform.localPosition.y;
					transform.localPosition = fixedPosition;
				}

				transform.localPosition = fixedPosition;
				

				 UpPos.CheckCollision(gameObject);

				

			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your camera as MainCamera, or set one in this component.", this);
			}
		}

		private void FingerUp(LeanFinger upFinger)
		{
			
			// Go through all fingers and return if any are still touching the screen
			if (isParallel)
			{
				var fingers = LeanTouch.Fingers;

				for (var i = fingers.Count - 1; i >= 0; i--)
				{
					var finger = fingers[i];

					if (finger.Up == false)
					{
						return;
					}
				}

				pinchTwistToggle.enabled = true;
				isParallel = false;
			}
		}
	}
}