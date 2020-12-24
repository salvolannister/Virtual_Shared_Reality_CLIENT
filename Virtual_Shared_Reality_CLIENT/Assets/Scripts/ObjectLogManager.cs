using Microsoft.AspNetCore.SignalR.Client;
using SharedClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
using Valve.VR;
/// <summary>
/// 
/// </summary>
public class ObjectLogManager : MonoSingleton<ObjectLogManager>
{
    // Start is called before the first frame update
    public ObjLog obj;
    private int translation;
    private int rotation;
    private int scale;
    public static long sessionId;
    public static int id;
    public string m_levelName;
    public static GameObject objectToRecord;
    private static Stopwatch stopwatch = new Stopwatch();
    private Vector3 LossyScale;
    float  m_time;

    private void Start()
    {
        objectToRecord = new GameObject("Log");
        UpdatePositionHub.ObjectSelectedEvent += StartRecording;
        UpdatePositionHub.ObjectDeselectedEvent += CreateLog;
        ClientManager.levelUploaded += RecordLevelName;
    }

    private void RecordLevelName(string levelName)
    {
        m_levelName = levelName;
    }

    private void OnDisable()
    {
       
            UpdatePositionHub.ObjectSelectedEvent -= StartRecording;
            UpdatePositionHub.ObjectDeselectedEvent -= CreateLog;
        ClientManager.levelUploaded -= RecordLevelName;
      
    }


    /// <summary>
    /// Initialize the gameObject copy with the start position of the objectS
    /// </summary>
    /// <param name="m_movingGameObject"></param>
    public  void StartRecording(Transform m_movingGameObject)
    {
        
        stopwatch.Restart();
        objectToRecord.transform.localPosition =m_movingGameObject.localPosition;
        objectToRecord.transform.localRotation = m_movingGameObject.localRotation;
        LossyScale = m_movingGameObject.lossyScale;

       
    }
    /// <summary>
    /// understand what changed in the transform and send the log to the server
    /// </summary>
    /// <param name="end_transform">transform position were we stoped</param>
    /// <param name="m_name">Name of the object that we were moving</param>
    public void CreateLog(Transform end_transform)
    {
        string m_name = end_transform.name;
        stopwatch.Stop();
        m_time = (int)stopwatch.Elapsed.TotalMilliseconds; // second is counting like 0
        Transform start_transform = objectToRecord.transform;

        if (start_transform.localPosition.Equals(end_transform.localPosition))
            translation = 0;
        else translation = 1;

        if (start_transform.localRotation.Equals(end_transform.localRotation))
            rotation = 0;
        else rotation = 1;

        if (LossyScale.Equals(end_transform.localScale))
            scale = 0;
        else scale = 1;

       
        obj = new ObjLog(m_name, translation, rotation, scale, m_time, sessionId, id, m_levelName);
       
        SendLog();
    }

    internal void SendLog()
    {
        string fileName = "objects";
        OpenConnection.Instance.HubConnection.InvokeAsync("WriteLog",obj, fileName); 
    }

    
}
