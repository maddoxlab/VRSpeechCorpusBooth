using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendInput : MonoBehaviour
{

    public OSC osc;
    public enum Hand { Right, Left };
    public Hand hand;

    private float currTime;
    private float trlStartTime;
    private OVRInput.Button button_vis;
    private OVRInput.Button button_aud;
    private List<float> responses;
    private string xshort;


    void Start()
    {
        // set up button mappings depending on which hand was chosen for responses
        if (hand.Equals(Hand.Right))
        {
            button_vis = OVRInput.Button.One;
            button_aud = OVRInput.Button.Two;
        }
        else
        {
            button_vis = OVRInput.Button.Three;
            button_aud = OVRInput.Button.Four;
        }
        osc.SetAddressHandler("/Set_up_trial", OnReceiveSetupTrial);
        osc.SetAddressHandler("/Play_trial", OnReceiveStartTrial);
        osc.SetAddressHandler("/Retrieve_responses", OnReceiveSendResponses);
    }


    void Update()
    {
        bool vis_pressed = OVRInput.GetDown(button_vis);
        bool aud_pressed = OVRInput.GetDown(button_aud);

        if (vis_pressed)
        {
            currTime = Time.time - trlStartTime;
            Debug.Log("you pressed the visual button");
            responses.Add(currTime);
            responses.Add(1); // 1 if by vision
        }
        if (aud_pressed)
        {
            currTime = Time.time - trlStartTime;
            Debug.Log("you pressed the auditory button");
            responses.Add(currTime);
            responses.Add(2); // 2 if by audition
        }
    }

    void OnReceiveSetupTrial(OscMessage message)
    {
        // before trial playback, clear out the old responses
        responses = new List<float>();
    }

    void OnReceiveStartTrial(OscMessage message)
    {
        // store the start time
        trlStartTime = Time.time;
    }

    void OnReceiveSendResponses(OscMessage message)
    {
        // send back response data
        OscMessage reply;
        reply = new OscMessage();
        reply.address = "/responses";
        for (int i = 0; i < responses.Count; i++)
        {
            // truncate long values to save space in OSC message
            string x = responses[i].ToString();
            int decidx = x.IndexOf('.');
            if (decidx == -1) // no decimal, no rounding
            {
                xshort = x;
            }
            else if (x.Substring(decidx + 1).Length > 4) // long enough to round
            {
                xshort = x.Substring(0, decidx + 5);
            }
            else // decimal but too short to round
            {
                xshort = x;
            }
            reply.values.Add(xshort);
        }
        osc.Send(reply);
    }
}
