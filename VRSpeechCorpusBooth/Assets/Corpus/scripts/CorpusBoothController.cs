using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System;

public class CorpusBoothController : MonoBehaviour
{
    // Start is called before the first frame update
    public OSC osc;
    public GameObject videoPlayerObject;
    public GameObject[] SkyboxCorpus;
    private List<string> video_paths = new List<string>();
    private VideoPlayer[] videoPlayers;

    void Start()
    {
        //var a = videoPlayerObject.gameObject.GetComponent<VideoPlayer>();
        //Debug.Log(a);
        var list = videoPlayerObject.GetComponents<VideoPlayer>();
        for (int i = 0; i < list.Length; i++)
        {
            Debug.Log(list[i].name);
            list[i].playOnAwake = false;
            //list[i].Pause();
            list[i].isLooping = false;
            list[i].prepareCompleted += sendPrepared;
        }
        videoPlayers = list;
        osc.SetAddressHandler("/PrepareVideo", onPrepareVideo);
        osc.SetAddressHandler("/PlayVideo", onPlayVideo);
        osc.SetAddressHandler("/StopVideo", onStopVideo);
        osc.SetAddressHandler("/SetTransform", onSetTransform);
    }

    void sendPrepared(UnityEngine.Video.VideoPlayer vPlayer)
    {
        vPlayer.Play();
        vPlayer.Pause();
        Debug.Log("Video Prepared");
        OscMessage reply = new OscMessage();
        reply.address = "/prepared";
        reply.values.Add("[0]");
        osc.Send(reply);
        
    }

    void onPrepareVideo(OscMessage message)
    {
        for (int i = 0; i < videoPlayers.Length; i++)
        {
            videoPlayers[i].Stop();
            videoPlayers[i].Prepare();
            Debug.Log("Preparing video");
        }
    }

    void onPlayVideo(OscMessage message)
    {
        for (int i = 0; i < videoPlayers.Length; i++)
        {
            videoPlayers[i].Play();
            Debug.Log("Play video: "+DateTime.Now);
        }
        sendOSC("/start_now", "[1]");
    }

    void onStopVideo(OscMessage message)
    {
        for (int i = 0; i < videoPlayers.Length; i++)
        {
            videoPlayers[i].Stop();
            Debug.Log("Stop video: " + DateTime.Now);
        }
    }

    void onSetTransform(OscMessage message)
    {
        for (int i = 0; i < SkyboxCorpus.Length; i++)
        {
            float rotY = message.GetFloat(i);
            var rotation = SkyboxCorpus[i].transform.rotation.eulerAngles;
            rotation.y = rotY;
            SkyboxCorpus[i].transform.rotation = Quaternion.Euler(rotation);
        }
        sendOSC("/loaded", "[1]");
    }

    void sendOSC(string address,string msg)
    {
        OscMessage reply;
        reply = new OscMessage();
        reply.address = address;
        reply.values.Add(msg);
        osc.Send(reply);
    }

}
