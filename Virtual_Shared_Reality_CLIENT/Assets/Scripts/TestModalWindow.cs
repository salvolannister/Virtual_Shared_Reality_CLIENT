using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*This class has the purpose to advise the android user
 * that ´the connection is not working*/

public class TestModalWindow : MonoBehaviour
{
    private ModalPanel modalPanel;
    private OpenConnection openConn;

    private UnityAction myTryAction;
    //private UnityAction myNoAction;
    private UnityAction myCancelAction;

    void Awake()
    {
        modalPanel = ModalPanel.Instance();

        openConn = GameObject.Find("BootStrap").GetComponent<OpenConnection>();
        myTryAction = new UnityAction(TestTryFunction);
       
        myCancelAction = new UnityAction(TestCancelFunction);
    }

    //    Send to the Modal Panel to set up the Buttons and Functions to call
    public void TestYNC(string error)
    {
        modalPanel.Choice(error, TestTryFunction, TestCancelFunction);
        //        modalPanel.Choice ("Would you like a poke in the eye?\nHow about with a sharp stick?", myYesAction, myNoAction, myCancelAction);
    }

    //    These are wrapped into UnityActions
    void TestTryFunction()
    {
        TryConnectionAgain();


    }

    void TestCancelFunction()
    {
            Debug.Log(" I give up ");
    }


    private void TryConnectionAgain()
    {
        openConn.StartConnection();

    }

}
