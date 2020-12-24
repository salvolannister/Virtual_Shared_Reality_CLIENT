using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/* compare the actual element with the one with the same tag */
public class CalculateDifference : MonoBehaviour
{

   
  
      public  const int RIGTH = 90;
      public  const int FLAT = 180;
      public  const int FULL = 360;
    //  public enum Tutorials
    //    {
    //        translation =1,

    //        rotation =2,
    //        scale = 3,
    //        end = 4
    //    }
    //public Tutorials tutorial;


    public TutorialManager.Tutorials tutorial;
    private const float MinDist = 0.00022f;
    private const float MaxDist = 0.003f;
    private const float MIN_ANGLE = 5;
    private const float maxAngle = 20;
    private const float minScale = 0.005f;//0.05
    private const float maxScale = 0.03f; //1
    private readonly float MAX_POINT_DIST = 1;
    
    private  int maxPoints;
    private SphereCollider MyCollider;

    Collider[] m_neighbours = new Collider[20];
    public void Start()
    {
       if(!ClientManager.Instance.TUTORIAL)
             GetTotalScoreGeneral.OnClick += DockDifference;
        else
        {
            tutorial = TutorialManager.Instance.tutorial;
           
            GetTotalScoreGeneral.OnClick += DockDifferenceTutorial;
        }
    }




    private void OnDisable()
    {
        if (!ClientManager.Instance.TUTORIAL)
            GetTotalScoreGeneral.OnClick -= DockDifference;
        else
        {
            // DOESNT GO HERE
            GetTotalScoreGeneral.OnClick -= DockDifferenceTutorial;
        }
        
    }

    /// <summary>
    /// The level got for tutorial is wrong, but we needed to call the method
    /// </summary>
    /// <param name="level">Set this element in the switch</param>
    private void DockDifferenceTutorial(int level)
    {
       
        
        /*find object that are colliding*/
        int count = 0;
        GameObject compare = null;
        Vector3 fixedPosition;


        MyCollider = gameObject.AddComponent<SphereCollider>();
        //PropManagerPro pmp = transform.GetComponent<PropManagerPro>();
        //float radius = pmp.GetScaledRadius();
        float radius = 0.5f;

        //radius *= transform.lossyScale.x;
        float point_dist = 0;
        float point_angle = 0;
        float point_scale = 0;
        float tot_point = 0;


        fixedPosition = transform.position;
        count = Physics.OverlapSphereNonAlloc(fixedPosition, radius, m_neighbours);
        /*see if there is any with the same tag*/
        //Debug.Log("<color=red>"+ name +"</color> "+ " #count colliding is " + count);
        compare = FindDocking(count, MyCollider);

        maxPoints = level;

        if (compare == null)
        {
            /* put the points for dist, angle and scale to zero */

            // Debug.Log("<color=blue>[CalcDiff] </color> The object for comparing " + name + " is too distan or null");
            tot_point = 0;

          
        }
        else /*calculate Points for position, scale, rotation*/
        {
            
            switch (tutorial)
            {
                case TutorialManager.Tutorials.translation:
                {
                        point_dist = GetPositionPoints(compare, ref tot_point);
                        Debug.Log("Punti totali posizione " + point_dist);
                        break;
                }
                case TutorialManager.Tutorials.rotation:
                {
                        tot_point = GetRotationPoints(compare, point_dist, out point_angle, tot_point);
                      
                        break;
                }
                case TutorialManager.Tutorials.scale:
                {
                        tot_point = GetScalePoints(compare, out point_scale, tot_point);
                        Debug.Log(" li calcola in scale");
                        break;
                }
                case TutorialManager.Tutorials.end:
                {
                        Debug.LogError("It should never go here");
                        break;
                }
                

            }
           
        }

       
        TutorialManager.Instance.ContinueTutorial(tot_point);
        //Debug.Log("Tutorial value is " + tutorial.ToString());

        UnityMainThreadDispatcher.Instance().Enqueue(() => Destroy(MyCollider));
    }


    /*creates the collider and calculates the level points
     * cant't create the collider before cause Vuforia enables it even if disabled!*/
    public void DockDifference(int level)
    {
        /*find object that are colliding*/
        int count = 0;
        GameObject compare = null;
        Vector3 fixedPosition;
        
       
        MyCollider = gameObject.AddComponent<SphereCollider>();
        //PropManagerPro pmp = transform.GetComponent<PropManagerPro>();
        //float radius = pmp.GetScaledRadius();
        float radius = 0.05f;

     
        float point_dist = 0;
        float point_angle = 0;
        float point_scale = 0;
        float tot_point = 0;
        

        fixedPosition = transform.position;
        count = Physics.OverlapSphereNonAlloc(fixedPosition, radius/2, m_neighbours);
        /*see if there is any with the same tag*/
        //Debug.Log("<color=red>"+ name +"</color> "+ " #count colliding is " + count);
        compare = FindDocking(count, MyCollider);

        maxPoints = level;

        if (compare == null)
        {
            /* put the points for dist, angle and scale to zero */

           // Debug.Log("<color=blue>[CalcDiff] </color> The object for comparing " + name + " is too distan or null");
            tot_point = 0;

            if(level > 2)
            {
                //Debug.Log(this.gameObject.name + " was a " + this.gameObject.tag);
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                GetTotalScoreGeneral.Instance.AddFood(name, 0);
            }
        }
        else /*calculate Points for position, scale, rotation*/
        {
            point_dist = GetPositionPoints(compare, ref tot_point);
            if (point_dist > 0) // If distance is too big, no need to calculate the other points
            {




                if (level > 1)
                {



                    tot_point = GetRotationPoints(compare, point_dist, out point_angle, tot_point);
                    if (level > 2)
                    {



                        tot_point = GetScalePoints(compare, out point_scale, tot_point);
                        GetTotalScoreGeneral.Instance.AddFood(name, 1);


                    }

                }
                tot_point /= maxPoints;
            }
            else
            {
                tot_point = 0;
                if (level > 2)
                {
                    GetTotalScoreGeneral.Instance.AddFood(name, 0);
                    gameObject.GetComponent<Renderer>().material.color = Color.red;
                }
            }
        }


        //Debug.Log("<color=yellow> Total points "+ name+ " </color> "+ tot_point);
        
       GetTotalScoreGeneral.Instance.points.Add(tot_point);

        UnityMainThreadDispatcher.Instance().Enqueue(() => Destroy(MyCollider));
        

    }

    private float GetPositionPoints(GameObject compare, ref float tot_point)
    {
        float point_dist;
        //Debug.Log("[CalcDiff] obj " + gameObject.name + " docked " + compare.name);
        float dist = Vector3.SqrMagnitude(transform.position - compare.transform.position);
        //Debug.Log("distance is " + dist);
        point_dist = Normalize(dist, MaxDist, MinDist);
        tot_point += point_dist;
        //Debug.Log("[CalcDiff] Points for <color=blue>position</color> is: " + point_dist);
        return point_dist;
    }

    private float GetScalePoints(GameObject compare, out float point_scale, float tot_point)
    {
     float   ScaleDiff = transform.lossyScale.sqrMagnitude - compare.transform.lossyScale.sqrMagnitude;
        point_scale = Normalize(ScaleDiff, maxScale, minScale);
        //Debug.Log("<color=yellow>[CalcDiff] Points for scale is: </color>" + point_scale);
        tot_point += point_scale;
        return tot_point;
    }

    private float GetRotationPoints(GameObject compare, float point_dist, out float point_angle, float tot_point)
    {
        float AngleDiff;
        AngleDiff = Quaternion.Angle(transform.rotation, compare.transform.rotation);
        //Debug.Log(name + " Angle w q: " + AngleDiff);


        if (MAX_POINT_DIST - point_dist < 0.5f && AngleDiff > maxAngle)  // Check for equivalent rotation cases
        {
            if (FLAT - AngleDiff < maxAngle)
                AngleDiff = FLAT - AngleDiff;
            else if (RIGTH - AngleDiff < maxAngle)
            {

              
                AngleDiff = RIGTH - AngleDiff;
               
                  

            }else if(FULL - AngleDiff < maxAngle)
            {
                AngleDiff = FULL - AngleDiff;
            }
        }

        point_angle = Normalize(AngleDiff, maxAngle, MIN_ANGLE);
        //Debug.Log("[CalcDiff] Points <color=orange> rotation</color> is: " + point_angle);
        tot_point += point_angle;
        return tot_point;
    }

    /// <summary>
    ///  Finds the first object with the same tag that is colliding and has a distance less than 1mm
    /// </summary>
    /// <param name="count">Number of objects colliding in the neighborhood</param>
    /// <param name="m_neighbours">Colliders in the neighborhood</param>
    /// <param name="myCollider">Collider proprierty of the object where to check</param>
    /// <returns></returns>
    private GameObject FindDocking(int count, SphereCollider myCollider)
    {
        float min = 500f;
        GameObject m_compare = null;
        GameObject final_compare = null;
        for (int i = 0; i < count; i++)
        {

            if (myCollider == (m_neighbours[i]))
                continue;
            
                //Debug.Log("<color=green> "+name +" </color>"+ "colliding with " +neighbours[i].name);

            if (gameObject.CompareTag(m_neighbours[i].gameObject.tag)
                && m_neighbours[i].gameObject != gameObject)
            {
                m_compare = m_neighbours[i].gameObject;
               // Debug.Log(name + " colliding with " + m_compare.name);
                float distance = Vector3.Distance(m_compare.transform.position, this.transform.position);
                //Debug.Log(" distance " + distance);
                if (distance < 0.001f)
                {
                    final_compare = m_compare;
                    break;
                }
                else if( distance < min)
                {
                    final_compare = m_compare;
                    min = distance;
                }
            }

        }

        return final_compare;
    }

    
    private float Normalize(float diff, float max_t, float min_t)
    {
        if (diff <= max_t)
        {
            if (diff < min_t) diff = min_t;
            /*else stay in between*/
        }
        else diff = max_t;

        /* 0 point if diff = threshold max one point if in the thresholds */
        return (diff - max_t) / (min_t - max_t);
    }   



}
