using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class JoinCurrentSession : LoadLevel
{
    // Start is called before the first frame update
    public override void SetupScene()
    {
        Task getLevelName = AskForLevelName();
        getLevelName.Wait();
        /*set in General Score*/
        GetTotalScoreGeneral.Instance.StartLevel(levelName);
    
        /*closeCanvas*/
        CloseCanvas();
    }


    private async Task AskForLevelName()
    {
        try
        {
            if (hubConnection == null) hubConnection = OpenConnection.Instance.HubConnection;

            levelName = await hubConnection.InvokeAsync<string>("GetLNameDirectlyAsync");
            
        }
        catch (Exception e)
        {
            Debug.Log("error " + e.ToString());
        }
    }

}
