using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextMeshFade : MonoBehaviour
{
    
    private float starting_count;
    public TextMeshProUGUI m_textMeshPro;

    void OnEnable()
    {
        m_textMeshPro = this.gameObject.GetComponent<TextMeshProUGUI>();
    }

   public  void BeginFade()
    {
        StartCoroutine(DisplayTextMeshFadingText());
    }

    private IEnumerator DisplayTextMeshFadingText()
    {
        float CountDuration = 2.0f; // How long is the countdown alive.
        float alpha = 255;       
        float starting_Count;
        float current_Count = starting_Count = 2f;
        //float fadeDuration = 3 /  CountDuration * starting_Count;

        while (current_Count > 0 && alpha!= 0)
        {
            current_Count -= (Time.deltaTime / CountDuration) * starting_Count;
            alpha = Mathf.Clamp(alpha - (Time.deltaTime) * 255, 0, 255);
 
            m_textMeshPro.alpha = (byte)alpha;
            yield return new WaitForEndOfFrame();
        }
    }

    public void ResetFade()
    {
        m_textMeshPro.alpha = 255;
    }
}
