using UnityEngine;

namespace Lean.Touch
{
	
//This component will enable/disable the target pinch and twist components based on total pinch and twist gestures, like mobile map applications
	public class PinchTwistToggle : MonoBehaviour
	{
		public enum StateType
		{
			None,
			Scale,
			Rotate,
			ScaleRotate
		}

		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreStartedOverGui = true;

		[Tooltip("Ignore fingers with IsOverGui?")]
		public bool IgnoreIsOverGui;

		[Tooltip("Ignore if there was no change?")]
		public bool IgnoreIfStatic;

		[Tooltip("Ignore fingers if the finger count doesn't match? (0 = any)")]
		public int RequiredFingerCount;

		[Tooltip("If RequiredSelectable.IsSelected is false, ignore?")]
		public LeanSelectable RequiredSelectable;

		[Tooltip("The component that will be enabled/disabled when scaling")]
		public MonoBehaviour PinchComponent;

		[Tooltip("The component that will be enabled/disabled when rotating")]
		public MonoBehaviour TwistComponent;

		[Tooltip("The amount of pinch required to enable twisting in scale (e.g. 0.1 = 0.9 to 1.1).")]
		public float PinchThreshold = 0.1f;

		[Tooltip("The state we enter when you pinch past the threshold.")]
		public StateType PinchMode = StateType.Scale;

		[Tooltip("The amount of twist required to enable twisting in degrees.")]
		public float TwistThreshold = 5.5f;

		[Tooltip("The state we enter when you pinch past the threshold.")]
		public StateType TwistMode = StateType.Rotate;

		[System.NonSerialized]
		private StateType state;

		[System.NonSerialized]
		private float scale = 1.0f;

		[System.NonSerialized]
		private float twist;

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
		}

		protected virtual void Update()
		{
			// Get fingers
			var fingers = LeanSelectable.GetFingers(IgnoreStartedOverGui, IgnoreIsOverGui, RequiredFingerCount, RequiredSelectable);

			if (fingers.Count > 0)
			{
				scale *= LeanGesture.GetPinchRatio(fingers);
				twist += LeanGesture.GetTwistDegrees(fingers);

				if (state == StateType.None)
				{
					float Pscale = Mathf.Abs(scale - 1.0f);
					float Ptwist = Mathf.Abs(twist);
				
					if (Pscale >= PinchThreshold)
					{
						state = PinchMode;
					}
					else if (Ptwist >= TwistThreshold)
					{
						state = TwistMode;
					}
				}
			}
			else
			{
				state = StateType.None;
				scale = 1.0f;
				twist = 0.0f;
			}

			switch (state)
			{
				case StateType.None:
				{
					PinchComponent.enabled = false;
					TwistComponent.enabled = false;
				}
				break;

				case StateType.Scale:
				{
					PinchComponent.enabled = true;
					TwistComponent.enabled = false;
				}
				break;

				case StateType.Rotate:
				{
					PinchComponent.enabled = false;
					TwistComponent.enabled = true;
				}
				break;

				//case StateType.ScaleRotate:
				//{
				//	PinchComponent.enabled = true;
				//	TwistComponent.enabled = true;
				//}
				//break;
			}
		}
	}
}