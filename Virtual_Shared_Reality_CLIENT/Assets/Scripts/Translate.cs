﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translate : MonoBehaviour
{

    Vector3 dist;
    float posX;
    float posY;

    void OnMouseDown()
    {
        dist = Camera.main.WorldToScreenPoint(transform.position);
        posX = Input.mousePosition.x - dist.x;
        posY = Input.mousePosition.y - dist.y;

    }

    void OnMouseDrag()
    {
        Vector3 curPos =
         new Vector3(Input.mousePosition.x - posX,
                     Input.mousePosition.y - posY, dist.z);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(curPos);
        transform.position = worldPos;
    }

}
