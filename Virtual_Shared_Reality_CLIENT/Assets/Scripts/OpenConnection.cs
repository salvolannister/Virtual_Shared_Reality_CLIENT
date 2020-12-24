using System.Collections.Generic;
using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;


/* create an instance of an 
 * HUB and create the conncetion*/
public class OpenConnection : MonoSingleton<OpenConnection>
{
    public bool useSignalR = true;
#if UNITY_ANDROID
            public TestModalWindow TMW;
#endif

    public override void Init()
    {
        base.Init();
    }


    public HubConnection HubConnection { get; private set; }

    public HubConnection StartSignalR(string signalRUrl)
    {
               
            var hubConnectionBuilder = new HubConnectionBuilder()
                            .WithUrl(signalRUrl);
         
            /* we can use two protocol 
             * but we choose to use the json one*/
                hubConnectionBuilder = hubConnectionBuilder
                    .AddJsonProtocol(options =>
                    {
                        options.PayloadSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects;
                    });
            
            HubConnection = hubConnectionBuilder
                .Build();

        


        return HubConnection;
        
    }

    /*tells to the other script in the scene that need the Hub that now they
     * can get his reference through the method GetHubConnection, cause the connection is open */
    //private void HelloConnection()
    //{
    //    // get root objects in scene
    //    Scene secondScene = SceneManager.GetSceneAt(1);

    //    List<GameObject> rootObjects = new List<GameObject>(secondScene.rootCount + 1);
    //    secondScene.GetRootGameObjects(rootObjects);

    //    // iterate root objects and do something
    //    for (int i = 0; i < rootObjects.Count; ++i)
    //    {
    //        GameObject gameObject = rootObjects[i];
    //        if (gameObject != null)
    //        {
    //            gameObject.BroadcastMessage(broadcastMethod, SendMessageOptions.DontRequireReceiver);
    //        }

    //    }
    //}

    public void StartConnection()
    {
        //Debug.Log("connecting...");

    
        Task t = new Task(async () => await Connect());
        t.RunSynchronously();
        t.Wait();
       
        //Debug.Log("done");


        HubConnection.Closed += async (exception) =>
        {
            if (exception != null)
            {
                await Task.Delay(2000);
                await HubConnection.StartAsync();
            }
        };

    }

    public bool Connected { get; private set; }

    public async Task Connect()
    {
        try
        {
            await HubConnection.StartAsync();
            Connected = true;
        }
        catch (Exception ex)
        {
#if UNITY_ANDROID 
            TMW = GameObject.Find("Manager").GetComponent<TestModalWindow>();
            TMW.TestYNC(ex.ToString());
#endif
            throw ex;
        }
    }

 


    
}
