using Lean.Touch;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*script that allows to activate/deactivate onDouble tap events
 It switches between the two modalities: onPlane, moves only xz;
 and UpDown, moves Up/down*/
public class ChangeModality : MonoBehaviour
{
	public Canvas EmptyZPlane;
	TextMeshFade[] TextEffects;
	private TextMeshProUGUI[] TextMesh;
	private bool already = false;
	public int RequiredTapInterval = 2;
	public Image[] img;
	public GameObject imageField;
	
	void Start()
    {
		EmptyZPlane = GameObject.FindWithTag("Zplane").GetComponent<Canvas>();
		if (EmptyZPlane != null)
		{

			TextMesh = EmptyZPlane.transform.GetComponentsInChildren<TextMeshProUGUI>();
			TextEffects = EmptyZPlane.transform.GetComponentsInChildren<TextMeshFade>();
			imageField = EmptyZPlane.transform.GetChild(2).gameObject;
			img = imageField.GetComponentsInChildren<Image>();
		}
		else {
			Debug.Log("The canvas for zplane mode is null");
		}
		
	}

	public void ChangeTransformDirection(LeanFinger finger)
	{

		if (RequiredTapInterval > 0 && (finger.TapCount % RequiredTapInterval) == 0)
		{
			if (!already)
			{
				/*ZX  */
				if (TextMesh[0].enabled != true || TextMesh[1] != false)
				{
					TextEffects[1].ResetFade();
					TextMesh[1].enabled = false;
					TextMesh[0].enabled = true;
					img[1].enabled = false;
					img[0].enabled = true;
					TextEffects[0].BeginFade();
				}


				this.gameObject.GetComponent<LeanDragTranslate>().onPlane = true;
				already = true;
			}
			else
			{

				if (TextMesh[0].enabled != false || TextMesh[1] != true)
				{
					img[1].enabled = true;
					TextMesh[1].enabled = true;
					TextEffects[1].BeginFade();
					TextMesh[0].enabled = false;
					img[0].enabled = false;
					TextEffects[0].ResetFade();
				}

				this.gameObject.GetComponent<LeanDragTranslate>().onPlane = false;
				already = false;
			}
		}


	}


	protected virtual void OnEnable()
	{
	
		LeanTouch.OnFingerTap += ChangeTransformDirection;
	}

	protected virtual void OnDisable()
	{
	
		LeanTouch.OnFingerTap -= ChangeTransformDirection;
	}
	
}
