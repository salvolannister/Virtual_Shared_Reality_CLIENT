using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Closes the canvas and quit the game
/// </summary>
public class CloseCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject canvas;
    public Image imgModality;

#if UNITY_ANDROID && !UNITY_STANDALONE
    public Lean.Touch.ButtonManagerAndroid ButtonScript;
#else
    public ButtonManagerHmd ButtonScript;
#endif

   

    

    public void QuitGame()
    {
        ClientManager.Instance.OnApplicationQuit();
        Application.Quit();
    }
    /* disable the hmd pointer and close the canvas*/
    public void Continue()
    {
        GameObject Press = GameObject.FindWithTag("Points");
        canvas.SetActive(false);
#if UNITY_ANDROID && !UNITY_STANDALONE
        if(imgModality!= null) imgModality.enabled = true;
        Press.GetComponent<Lean.Touch.ButtonManagerAndroid>().ButtonTurnOff();
#else
        /* only if in debug mode = HMD*/
        if (ClientManager.Instance.debug == 1)
        {
            Press.GetComponent<ButtonManagerHmd>().ButtonTurnOff();
          
        }
        else
        {
            if (imgModality != null) imgModality.enabled = true;
        }
        
#endif

    }

  

}
