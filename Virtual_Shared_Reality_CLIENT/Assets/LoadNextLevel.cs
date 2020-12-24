using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Read levelName field from GetTotalScoreGeneral and
 * makes start the next level if is not the last*/

public class LoadNextLevel : LoadLevel
{
    // Start is called before the first frame update
    public override void OnEnable()
    {
       
        //GetTotalScoreGeneral.ScoreManReady += InitScoreMan;
        levelButton = this.GetComponent<Button>();
        levelButton.onClick.AddListener(SetupScene);
        if (canvas == null) 
            Debug.Log("you forgot to put canvas in " + name);

       
       

    }



  

    public override void SetupScene()
    {
       
        levelName = GetTotalScoreGeneral.Instance.GetLevelName();
        if (levelName != null)
        {
           
            
            if (!Levels.TryParse(levelName, out Levels myLevel)) 
                Debug.Log("Parsing enum went wrong");
            else
            {
                int n = (int)myLevel;
                if(n != 3)
                {
                    n++;
                    Levels NewLevel = (Levels)n;
                    current = NewLevel;
                    levelName = NewLevel.ToString();
                    /* hub connection is null because
                     * the object is disabled and can't
                     * register to the call ready on time*/ 
                   if(current != GetTotalScoreGeneral.Instance.m_actualLevel) // prevent to people to click both NEXT LEVEL
                    {
                        if (!CreateScene(levelName))
                            Debug.Log("error in loading the scene " + levelName);
                        GetTotalScoreGeneral.Instance.StartLevel(levelName);
                        if (current == Levels.ThirdLevel || current == Levels.FirstLevel)
                            OpenInfoCanvas(current);
                    }
                  
                }
                else
                {
                    /* quit the game */
                    Debug.Log("There s no Next level");
                }

                CloseCanvas();
            }
            ButtonScript.ButtonTurnOff();
        }
        else
        {
            Debug.Log("[LLP] level name is null");
        }
    }
}
