using System.Collections;
using UnityEngine;
using Valve.VR;

/// <summary>
///  Calls GetTotalScoreGeneral and it makes the button red/green
/// </summary>
public class ButtonManagerHmd : MonoBehaviour
{

    public SteamVR_Input_Sources handType;    
    public static int pressed = 0;
    public GameObject PR_Pointer;   
    public  GameObject Button;
    public AudioSource m_audio;
    public Animation m_animation;
    private void Start()
    {
        Button = this.gameObject;
        //audio = GetComponent<AudioSource>();
        //animation = GetComponentInParent<Animation>();
    }

    private void OnTriggerEnter(Collider other)
    {
        /*check if the collider is of the handType*/
        if (pressed == 0 && other.gameObject.CompareTag("Player"))
        {
            pressed = 1;
            PressButton();
        }
    }

    /// <summary>
    /// this function works in pair with ButtonTurnOff, if u call one you need to call the other
    /// </summary>
    public void PressButton()
    {
        //No canvas should be enabled at this moment
        if (!GetTotalScoreGeneral.Instance.ScoreCanvas.isActiveAndEnabled && !GetTotalScoreGeneral.Instance.LevelThreeCanvas.isActiveAndEnabled)
        {
            //Debug.Log(" BUTTON SHOULD BE <color=green> GREEN</color> NOW");
            GetTotalScoreGeneral.Instance.CallInvoke();
            
            m_animation.Play("ButtonPressure");
            m_audio.Play();
          
        }
    }

    public void ButtonTurnOff()
    {
        if(pressed == 1)
        {
            //Debug.Log("Button <color=red>red</color> now");
            m_animation.Stop();
            m_animation.Play("ButtonReset");
            pressed = 0;
        }
    
    }


    
}
