using UnityEngine;
using System.Collections;

public class ShapeInput: MonoBehaviour {

    // refer to point
    public Laser laser;
    public OSC osc;
    
	// Use this for initialization
	void Start () {
 
	}
	
	void LateUpdate () {
        Vector3 error = laser.hitpoint - transform.position;

        if (laser.hitting && error.sqrMagnitude < transform.localScale.sqrMagnitude)
        {
            this.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            bool click = OVRInput.Get(OVRInput.Button.One);
            if (click)
            {
                OscMessage reply;
                reply = new OscMessage();
                reply.address = "/button_press";
                reply.values.Add(laser.hitpoint);
                osc.Send(reply);
                this.GetComponent<Renderer>().enabled = false;
            }
        }
        else
        {
            this.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        }


    
    }
}
