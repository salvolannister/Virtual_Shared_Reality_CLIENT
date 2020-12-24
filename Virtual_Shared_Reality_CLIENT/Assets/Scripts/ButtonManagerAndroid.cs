
using UnityEngine;

namespace Lean.Touch
{
    /// <summary>
    /// Stops count down when the button get pressed
    /// </summary>
    public class ButtonManagerAndroid : LeanSelectableBehaviour
    {

        public static int pressed = 0;
        private readonly float REFRESH_TIME = 2;
        private float last_time;
        public GameObject Button;
        public AudioSource m_audio;
        public Animation m_animation;
       
        protected override void OnSelect(LeanFinger finger)
        {
            if (pressed == 0 && (Time.time -last_time) > REFRESH_TIME)
            {
                pressed = 1;
                ButtonTurnOn();
                last_time = Time.time;
            }
        }

        /// <summary>
        /// Animate the button and enables it only if some specified canvas are disabled
        /// </summary>
        public void ButtonTurnOn()
        {
            if (!GetTotalScoreGeneral.Instance.ScoreCanvas.isActiveAndEnabled && !GetTotalScoreGeneral.Instance.LevelThreeCanvas.isActiveAndEnabled)
            {
                Debug.Log("<color=green> Button Pressed</color>");
                m_animation.Play("ButtonPressure");
                m_audio.Play();
                GetTotalScoreGeneral.Instance.CallInvoke();
            }
        }
 
        public void ButtonTurnOff()
        {
            if (pressed == 1)
            {
                pressed = 0;
                Debug.Log("<color=red> Button Reset</color>");
                m_animation.Stop();
                m_animation.Play("ButtonReset");
            }   
        }
         
        
    }
}
