using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSoundCtrl : MonoBehaviour
{
    public OSC osc;

    private MakeLabel objLabel;
    private int objID;
    private AudioSource cue_audio;
    private string cuefname;

    void Start()
    {
        osc.SetAddressHandler("/Setup_firstplay", OnReceiveFirstPlay);
        cue_audio = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        cue_audio.playOnAwake = false;
        cue_audio.loop = false;
        cue_audio.priority = 128;
        cue_audio.volume = 1;
        cue_audio.pitch = 1;
        cue_audio.spatialBlend = 1; // fully 3D
    }

    void OnReceiveFirstPlay(OscMessage message)
    {
        Debug.Log("Making audio...");
        objLabel = GetComponent<MakeLabel>();
        objID = int.Parse(objLabel.filename_label);
        if (objID == 0)
        {
            cuefname = "cue_323";
        }
        else if (objID == 1)
        {
            cuefname = "cue_391";
        }
        else if (objID == 2)
        {
            cuefname = "cue_440";
        }
        AudioClip newclip = Resources.Load<AudioClip>(cuefname);
        cue_audio.clip = newclip;
    }
}