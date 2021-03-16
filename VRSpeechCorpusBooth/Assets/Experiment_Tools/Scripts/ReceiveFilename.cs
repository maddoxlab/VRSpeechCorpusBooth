using UnityEngine;
using System.Collections;

public class ReceiveFilename : MonoBehaviour {
    
   	public OSC osc;
    private MakeLabel label;
    private string OSC_address;

    // Use this for initialization
    void Start () {
        label = GetComponent<MakeLabel>();
        OSC_address = "/fname" + label.filename_label;
        osc.SetAddressHandler(OSC_address , OnReceivefname);
        osc.SetAddressHandler("/Clear_audio", OnReceiveClear);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnReceivefname(OscMessage message)
    {
        string fname = message.ToString();
        fname = fname.Substring(OSC_address.Length + 1);

        // cut off the extension, required by Unity
        int ext_pos = fname.LastIndexOf(".");

        if (ext_pos >= 0)
        {
            fname = fname.Substring(0, ext_pos);
        }

        AudioSource audio = GetComponent<AudioSource>();
        //var www = new WWW("C:\\Users\\Public\\Code\\unitycontroller\\" + fname);
        //AudioClip myAudioClip = www.audioClip;
        //while (!myAudioClip.isReadyToPlay)
        //    yield return www;
        AudioClip newclip = Resources.Load<AudioClip>(fname);
        Debug.Log("start of clip");
        Debug.Log(newclip);
        audio.clip = newclip;
        OscMessage reply;
        reply = new OscMessage();
        reply.address = "/loaded";
        reply.values.Add('1');
        osc.Send(reply);
    }

    void OnReceiveClear(OscMessage message)
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = null;

    }
}
