using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    interface ITutorial
    {

        void SetInfoCanvas();

         void ContinueTutorial(float tot_point);

         void OpenTutorialInfo();

        void ResetTransform(UnityEngine.GameObject gameObject);

        void Close();
    }
}
