using UnityEngine;
using System.Collections;

public class OSCFrameWait : MonoBehaviour {
    
   	public OSC osc;


	// Use this for initialization
	void Start () {
	    osc.SetAddressHandler("/start_query", SendStart);
    }
	
	// Update is called once per frame
	void Update () {
        

    }

    void SendStart(OscMessage message)
    {
        new WaitForEndOfFrame();
        OscMessage reply;
        reply = new OscMessage();
        reply.address = "/start_ok";
        reply.values.Add('1');
        osc.Send(reply);

    }
}
