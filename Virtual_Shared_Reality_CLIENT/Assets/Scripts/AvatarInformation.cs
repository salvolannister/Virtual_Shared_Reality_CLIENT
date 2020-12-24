using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarInformation : MonoBehaviour
{
    public  enum  Device { desktop, android, hmd };


    public float scale;
    public Color color;
    public Device device;
    public float OFFSET_Y;
    public float OFFSET_X;

    public string GetDeviceType()
    {
        return device.ToString();
    }
    public void SetDevice(string deviceName)
    {
        Enum.TryParse(deviceName, out device);
    }


}
