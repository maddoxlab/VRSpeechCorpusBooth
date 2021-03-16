using UnityEngine;
using System.Collections;

public class ReceivePlay : MonoBehaviour {
    
   	public OSC osc;

    // Use this for initialization
    void Start () {
	    osc.SetAddressHandler("/Play" , OnReceivePlay);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnReceivePlay(OscMessage message){
        AudioSource audioData;
        audioData = GetComponent<AudioSource>();
        audioData.Play(0);
    }     
}
