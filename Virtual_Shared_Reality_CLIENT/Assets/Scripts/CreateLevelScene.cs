using Microsoft.AspNetCore.SignalR.Client;
using SignalR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Text;
using System.Globalization;

public class CreateLevelScene : MonoSingleton<CreateLevelScene>
{
    public GameObject root;
    private List<Primitive> primitives;
    public string Name;
    public string sufix = "Config";
    private int count= 0;
    [Tooltip("elements you want to write the config file for")]
    public int nElelments;
    public bool scale = false;
    public float scaleValue;
    public bool StartWriting;
    private readonly object m_lock = new object();

    public override void Init()
    {
        base.Init();
    }
    /* In order to make the script work, the models must have same name of the shape*/
 


    public void AddCount()
    {
        lock (m_lock)
        {
            count++;
        }
    }
    private void Update()
    {
        if (StartWriting)
        {
            WriteLevelFile();
            StartWriting = false;
        }


        if (count >= nElelments)
        {
            WriteConfigurationFile();
        }

        if (scale)
        {
            // scale everything for the desidered value
            foreach (Transform child in root.transform)
            {
            
                child.localScale = new Vector3(child.localScale.x, child.localScale.y, child.localScale.z )*scaleValue;
               
            }
            scale = false;
        }
      
    }

    private void WriteLevelFile()
    {
        primitives = new List<Primitive>();

        foreach (Transform child in root.transform)
        {
            int id = -1;
            if (child.gameObject.CompareTag("Static"))
                id = -2;

            Primitive p = new Primitive
            {
                Name = child.name,
                Shape = child.name,
                X = child.position.x,
                Y = child.position.y,
                Z = child.position.z,
                Id = id,
                RotX = child.rotation.x,
                RotY = child.rotation.y,
                RotZ = child.rotation.z,
                W = child.rotation.w,
                //Sx = child.lossyScale.x * 0.8f,
                //Sy = child.lossyScale.y * 0.8f,
                //Sz = child.lossyScale.z * 0.8f,

                Sx = child.localScale.x,
                Sy = child.localScale.y,
                Sz = child.localScale.z,
                Scale = true,
                Avatar = false
            };
            primitives.Add(p);



        }
        string jsonString = JsonConvert.SerializeObject(primitives);
        string path = Application.dataPath;
        File.WriteAllText(path + "\\" + Name + ".txt", jsonString);
        Debug.Log("<color=green> Level File written </color>");
    }

    private void WriteConfigurationFile()
    {
        Debug.Log("Writing Config file");
        /*write file with the information about config */
        foreach (Transform child in root.transform) // for every son, in the file
        {
            PropManager pM = child.gameObject.GetComponent<PropManager>();


            if (pM != null)
            {
                Transform gChild = child.GetChild(0);
                Vector3 centroid = gChild.localPosition;
                //centroid /= gChild.localScale.x;
                string content = child.gameObject.name + " " + pM.radius.ToString(CultureInfo.InvariantCulture) + " "
                    + centroid.x.ToString(CultureInfo.InvariantCulture)
                    + " " + centroid.y.ToString(CultureInfo.InvariantCulture)
                    + " " + centroid.z.ToString(CultureInfo.InvariantCulture) + "\n";
                Debug.Log(content);


                using (FileStream fs = new FileStream(Application.dataPath + "\\" + Name + sufix + ".txt", FileMode.Append, FileAccess.Write))
                {
                    Byte[] info =
                        new UTF8Encoding(true).GetBytes(content);

                    //Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }
            }
            /* write in the config file*/
        }
        count = 0;
    }
}

