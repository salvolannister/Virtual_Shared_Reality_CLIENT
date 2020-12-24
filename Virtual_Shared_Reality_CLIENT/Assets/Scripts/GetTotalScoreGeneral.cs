using Microsoft.AspNetCore.SignalR.Client;
using SharedClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


/*when everybody has calculated its dock difference, show the total points on a canvas set 
 * in the editor, at the beggining also notify to the objects they can read their info from file*/
public class GetTotalScoreGeneral : MonoSingleton<GetTotalScoreGeneral>
{
    // Start is called before the first frame update
    public delegate void buttonManager(int level);
    public static event buttonManager OnClick;
    public List<float> points;
    public Canvas ScoreCanvas;
    public Canvas LevelThreeCanvas;
    private int MaxElements = 10;
    float Total = 0, score = 0;
   


    
    
    private Stopwatch stopwatch = new Stopwatch();
    private int timeElapsed;

    private readonly string LevelConfig ="Configs/LevelConfig";
    private Dictionary<string, int> Food;

    public LoadLevel.Levels m_actualLevel;

    public override void Init()
    {
        base.Init();
        Food = new Dictionary<string, int>();
    }

    
    /// <summary>
    ///  start stopwatch and also signal other script that needs to know which level is
    /// </summary>

    public void StartLevel(string name)
    {
       if(Enum.TryParse(name, out m_actualLevel))
       {
            GetLevelMaxPoints();
            startCount();
       }
       
     
    }
   

    public void AddFood(string name , int value)
    {
        Food.Add(name, value);
    }

    public  string GetLevelName() {
       
            return m_actualLevel.ToString();
        
    }
    
    void Update()
    {
       
        if(points.Count == MaxElements)
        {
            //Debug.Log("<color=red>MaxELement reached </color> ");
            foreach (float p in points)
            {
                Total += p;
            }

            score = (Total / MaxElements) * 100;
            /* get tag information and retrieve datas about the level, read the file LevelConfig */

            /*open canvas and show score and time elapsed*/
            /* only in Android version open Canvas!!!*/
            OpenScoreCanvas();
            /* call the hub to send the informations*/
            /* 0 to occupy field that will be changed in the SERVER*/
            //string scoreToS = score.ToString().Replace(",", "."); /*Universal string standard*/
            //int firstDecimal = scoreToS.IndexOf(".");
            //scoreToS = scoreToS.Substring(0, firstDecimal + 2);
            string scoreToS = String.Format("{0:0.0}", score);
            RecordInfo recordInfo = new RecordInfo(0, scoreToS, timeElapsed, 0, m_actualLevel.ToString());
            OpenConnection.Instance.HubConnection.InvokeAsync("RecordDataAsync", recordInfo);
            points.Clear();
            Total = 0;
        }
    }

    /* Open the canvas and set the points after pressing the button*/
    private void OpenScoreCanvas()
    {
        int Level =(int) m_actualLevel;
        string scoreS = String.Format("{0:0.0}", score);
        if (ScoreCanvas != null && Level <3)
        {
         
            InsertValues(ScoreCanvas, scoreS);
        }
        else if (LevelThreeCanvas != null && Level > 2)
        {
           
            InsertValues(LevelThreeCanvas, scoreS);
            SetToggles();

        }
        else
        {
            UnityEngine.Debug.Log("[SCORE_GENERAL] OpenScoreCanvas error: " + Level);
        }
    }

    private void InsertValues(Canvas canvas, string scoreS)
    {
        canvas.gameObject.SetActive(true);
        TextMeshProUGUI[] TextMesh = canvas.transform.GetComponentsInChildren<TextMeshProUGUI>();

       
        TextMesh[0].text = scoreS + " %";
    }

   

    /* Open the canvas and set the points from clientManager,
     [ATTENTION]: works only if the texts for the point are the first two sons*/
    public void OpenScoreCanvas(RecordInfo recordInfo)
    {
        int Level = (int)m_actualLevel;
        if (ScoreCanvas != null && Level <3 )
        {
            Debug.Log("ScoreCanvas one");
            InsertValues(ScoreCanvas, recordInfo.Score );
            
        }
        else if (LevelThreeCanvas != null && Level >= 3)
        {
            InsertValues(LevelThreeCanvas, recordInfo.Score);
            SetToggles();


            /*sets informations about prots and carbs*/
        }
        else
        {
            UnityEngine.Debug.Log("[SCORE_GENERAL] OpenScoreCanvas error: " +Level);
        }
    }

    private void SetToggles()
    {
        Toggle[] Toggles = LevelThreeCanvas.transform.GetComponentsInChildren<Toggle>();
        foreach (Toggle toogle in Toggles)
        {
          
            if (Food.TryGetValue(toogle.gameObject.name, out int value))
            {
               // Debug.Log("[toogles] " + g.gameObject.name + " value " + value);
                if (value == 1)
                    toogle.isOn = true;

            }
        }
    }

    private void GetLevelMaxPoints()
    {  

        try
        {
            UnityMainThreadDispatcher.Instance().Enqueue(ReadLevelConfigFile());
           
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
    /*Reads the number of elements that are used to calculate the
     * total score from a .txt and sets the level variable*/
    private IEnumerator ReadLevelConfigFile()
    {
        TextAsset txt = (TextAsset)Resources.Load(LevelConfig, typeof(TextAsset));
        string content = txt.text;
        string actualLevel = m_actualLevel.ToString();
        using (var sr = new StringReader(content))
        {
            var line = string.Empty;
           
            while ((line = sr.ReadLine()) != null)
            {
                var dataPoints = line.Split(' ');
                if( actualLevel.CompareTo(dataPoints[0]) == 0)
                {
                    MaxElements = Int32.Parse(dataPoints[1]);
                    break;
                }
              
            }
          
        }
        yield return null;
    }

    /// <summary>
    /// Called when someone hit the button
    /// </summary>
    public void CallInvoke()
    {

            StopCount();
            OnClick?.Invoke((int) m_actualLevel);

    
    }

    public void  startCount()
    {
        stopwatch.Reset();
        stopwatch.Start();
        /*probably is a new Level so I get the informations*/
      
    }

    public void StopCount()
    {
        
        stopwatch.Stop();
        timeElapsed = (int)stopwatch.Elapsed.TotalMilliseconds;
        //Debug.Log("[GENERAL]:time (s)" + timeElapsed/1000);

    }


}
