using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

/*try too read the informations from a text file, 
 * if it fails it calculates them.
 * Now It's used to take the information about the radius in global scale to calculate collisions
     */
public class PropManagerPro : PropManager
{

    public  string LevelName ="";
    private string ResourcePath = "Configs/";

  




    public override void Init()
    {
        base.Init();
        StartConfig(GetTotalScoreGeneral.Instance.GetLevelName());
    }


    /*to use the functionality of reading the features from file...*/
    private void StartConfig(string name)
    {
        LevelName = name+ "Config";
        TextAsset txt = (TextAsset)Resources.Load(ResourcePath + LevelName, typeof(TextAsset));
        if (txt != null)
        {
            /*find your line */
            GetInformations(txt);
            /*setup radius and the rest*/



        }
        else
        {


            CreateCollisionProperties();
        }
       
    }

 
    private void GetInformations(TextAsset Text)
    {

        //StartCoroutine(ReadObjectConfigFile(Text));

        try
        {
            UnityMainThreadDispatcher.Instance().Enqueue(ReadObjectConfigFile(Text));

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

    }

    private IEnumerator ReadObjectConfigFile(TextAsset txt)
    {

        string content = txt.text;
        using (var sr = new StringReader(content))
        {
            var line = string.Empty;

            while ((line = sr.ReadLine()) != null)
            {
                var values = line.Split(' ');

                if (values[0].CompareTo(gameObject.name) == 0)
                {
                    /* in some computer there could  be prob reading a .txt file*/
                    try
                    {
                        
                        radius = float.Parse(values[1], CultureInfo.InvariantCulture);
                        //Debug.Log(" ragius read ");
                        if (UseCentroid)
                        {
                            centroid.x = float.Parse(values[2], CultureInfo.InvariantCulture);
                            centroid.y = float.Parse(values[3], CultureInfo.InvariantCulture);
                            centroid.z = float.Parse(values[4], CultureInfo.InvariantCulture);
                        }
                        break;
                    }
                    catch(FormatException fe)
                    {
                        Debug.LogError(fe.ToString());
                    }
                  
                }

            }
            
            
        }

        if (UseCentroid)
        {
            GameObject center = new GameObject
            {
                name = "centroid"
            };
            center.transform.parent = gameObject.transform;
            center.transform.localPosition = centroid;
        }
        yield return null;
    }


    


}
