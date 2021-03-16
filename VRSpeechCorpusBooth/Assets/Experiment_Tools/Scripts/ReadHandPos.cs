using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadHandPos : MonoBehaviour
{

    public Ray sel;
    void Start()
    {
        Vector3 pos = transform.position;
        Vector3 fwd = transform.forward;
        //Debug.Log(fwd);
        sel = new Ray(pos, fwd);
        
    }

    void Update()
    {
        Vector3 pos = transform.position;
        Vector3 fwd = transform.forward;
        //Debug.Log(fwd);
        sel.origin = pos;
        sel.direction = fwd;
    }
}
