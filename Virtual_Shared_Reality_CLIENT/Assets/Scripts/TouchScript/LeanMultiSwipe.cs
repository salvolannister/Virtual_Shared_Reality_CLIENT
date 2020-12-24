using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Lean.Touch
{
	// This script calculates the multi-swipe event
	// A multi-swipe is where you swipe multiple fingers at the same time, and OnSwipe gets called when the first finger is released from the screen
	public class LeanMultiSwipe : MonoBehaviour
	{
		// Event signatures
		[System.Serializable] public class FingerListEvent : UnityEvent<List<LeanFinger>> {}
		[System.Serializable] public class Vector2Event : UnityEvent<Vector2> {}
		
		[System.Serializable] public class FloatEvent : UnityEvent<float> {}

		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreStartedOverGui = true;

		[Tooltip("Ignore fingers if the finger count doesn't match? (0 = any)")]
		public int RequiredFingerCount;

		[Tooltip("If RequiredSelectable.IsSelected is false, ignore?")]
		public LeanSelectable RequiredSelectable;

		[Tooltip("Each finger touching the screen must have moved at least this distance for a multi swipe to be considered. This prevents the scenario where multiple fingers are touching, but only one swipes.")]
		public float ScaledDistanceThreshold = 50.0f;

		[Tooltip("This allows you to set the maximum angle between parallel swiping fingers for the OnSwipeParallel event to be fired.")]
		public float ParallelAngleThreshold = 20.0f;

		[Tooltip("This allows you to set the minimum pinch distance for the OnSwipeIn and OnSwipeOut events to be fired.")]
		public float PinchScaledDistanceThreshold = 100.0f;

		// Called when a multi-swipe occurs
		public FingerListEvent OnSwipe;

		// Called when a multi-swipe occurs where each finger moves paralell to each other (Vector2 = ScaledDirection)
		public Vector2Event OnSwipeParallel = new Vector2Event();

		// Called when a multi-swipe occurs where each finger pinches in (Float = ScaledDistance)
		public FloatEvent OnSwipeIn;

		// Called when a multi-swipe occurs where each finger pinches out (Float = ScaledDistance)
		public FloatEvent OnSwipeOut;

		// Set to prevent multiple invocation
		private bool swiped;

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			Start();
		}
#endif

		protected virtual void Start()
		{
			if (RequiredSelectable == null)
			{
				RequiredSelectable = GetComponent<LeanSelectable>();
			}

			LeanTouch.OnFingerSwipe += FingerSwipe;
			LeanTouch.OnFingerUp += FingerUp;
			//this.OnSwipeParallel.AddListener(prova);
		}

		protected virtual void OnEnable()
		{
			//LeanTouch.OnFingerSwipe += FingerSwipe;
			//LeanTouch.OnFingerUp    += FingerUp;
			//OnSwipeParallel.AddListener(prova);
			
		}

		protected virtual void OnDisable()
		{
			LeanTouch.OnFingerSwipe -= FingerSwipe;
			LeanTouch.OnFingerUp    -= FingerUp;
			
		}


		private void FingerSwipe(LeanFinger swipedFinger)
		{
			
			// Prevent multi invocation
			if (swiped == false)
			{
				// Get all valid fingers for swipe
				var fingers = LeanSelectable.GetFingers(IgnoreStartedOverGui, false, RequiredFingerCount, RequiredSelectable);

				// Make sure there are some fingers, and the current finger is a part of it
				if (fingers.Count > 0 && fingers.Contains(swipedFinger) == true)
				{
					swiped = true;
					Debug.Log("<color=red> oon swiped</color>");
					var scaledDelta = swipedFinger.SwipeScaledDelta;
					var isParallel  = true;

					// Go through all fingers
					for (var i = fingers.Count - 1; i >= 0; i--)
					{
						var finger = fingers[i];

						// If it's too old to swipe, skip
						if (finger.Age > LeanTouch.CurrentTapThreshold)
						{
							return;
						}

						// If it didn't move far enough to swipe, skip
						if (finger.SwipeScaledDelta.magnitude < ScaledDistanceThreshold)
						{
							return;
						}

						// If the finger didn't move parallel the others, make the OnSwipeParallel event inelligible
						if (finger != swipedFinger)
						{
							var angle = Vector2.Angle(scaledDelta, finger.SwipeScaledDelta);

							if (angle > ParallelAngleThreshold)
							{
								isParallel = false;
							}
						}
					}

					if (OnSwipe != null)
					{
						OnSwipe.Invoke(fingers);
					}

					if (fingers.Count > 1)
					{
						var centerA = LeanGesture.GetStartScreenCenter(fingers);
						var centerB = LeanGesture.GetScreenCenter(fingers);

						if (OnSwipeParallel != null && isParallel == true)
						{
							var delta = centerA - centerB;
							Debug.Log("<color=yellow> oon swipedparallel </color>");
							OnSwipeParallel.Invoke(delta * LeanTouch.ScalingFactor);
						}
						else
						{
							var pinch = LeanGesture.GetScaledDistance(fingers, centerB) - LeanGesture.GetStartScaledDistance(fingers, centerA);

							if (OnSwipeIn != null && pinch <= -PinchScaledDistanceThreshold)
							{
								OnSwipeIn.Invoke(-pinch);
							}

							if (OnSwipeOut != null && pinch >= PinchScaledDistanceThreshold)
							{
								OnSwipeOut.Invoke(pinch);
							}
						}
					}
				}
			}
		}

		private void FingerUp(LeanFinger upFinger)
		{
			// Go through all fingers and return if any are still touching the screen
			var fingers = LeanTouch.Fingers;

			for (var i = fingers.Count - 1; i >= 0; i--)
			{
				var finger = fingers[i];

				if (finger.Up == false)
				{
					return;
				}
			}

			// If not, reset swiped
			swiped = false;
		}
	}






}