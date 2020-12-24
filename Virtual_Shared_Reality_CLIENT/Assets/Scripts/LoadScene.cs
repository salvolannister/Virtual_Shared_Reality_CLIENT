using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
public class LoadScene : MonoBehaviour
{
    private string levelToLoad;
    // Start is called before the first frame update
    void Start()
    {
    #if UNITY_ANDROID
            levelToLoad ="TelephoneScene";
    #endif

    #if UNITY_STANDALONE
            levelToLoad ="HMDScene";
    #endif

        SceneManager.LoadScene(levelToLoad, LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
