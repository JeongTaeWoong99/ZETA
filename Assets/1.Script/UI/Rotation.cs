using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public float speed;
    public bool  plusOrMinus;
    
    private void Update()
    {
        if (plusOrMinus)
            transform.Rotate(0f, 0f, speed * Time.unscaledDeltaTime);
        else
            transform.Rotate(0f, 0f, -speed * Time.unscaledDeltaTime);
    }
}