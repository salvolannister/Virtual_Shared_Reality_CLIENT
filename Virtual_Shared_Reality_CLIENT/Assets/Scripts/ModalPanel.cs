using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


//    This script will be updated in Part 2 of this 2 part series.
public class ModalPanel : MonoBehaviour
{

    public Text question;
    public Image iconImage;
    public Button tryButton;
    public Button cancelButton;
    public GameObject modalPanelObject;

    private static ModalPanel modalPanel;

    public static ModalPanel Instance()
    {
        if (!modalPanel)
        {
            modalPanel = FindObjectOfType(typeof(ModalPanel)) as ModalPanel;
            if (!modalPanel)
                Debug.LogError("There needs to be one active ModalPanel script on a GameObject in your scene.");
        }

        return modalPanel;
    }

    // Yes/No/Cancel: A string, a Yes event, a No event and Cancel event
    public void Choice(string question, UnityAction yesEvent, UnityAction cancelEvent)
    {
        modalPanelObject.SetActive(true);

        tryButton.onClick.RemoveAllListeners();
        tryButton.onClick.AddListener(yesEvent);
        tryButton.onClick.AddListener(ClosePanel);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(cancelEvent);
        cancelButton.onClick.AddListener(ClosePanel);

        this.question.text = question;

        this.iconImage.gameObject.SetActive(false);
        tryButton.gameObject.SetActive(true);
        
        cancelButton.gameObject.SetActive(true);
    }

    void ClosePanel()
    {
        modalPanelObject.SetActive(false);
    }
}
