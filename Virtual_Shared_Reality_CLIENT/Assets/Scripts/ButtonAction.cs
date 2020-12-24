using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAction : MonoBehaviour
{
    OpenConnection openConn;
    // Start is called before the first frame update
    void Start()
    {
        openConn = GameObject.Find("BootStrap").GetComponent<OpenConnection>();
    }

    public void StartConnectionOnCLick()
    {
        openConn.StartConnection();
       
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
