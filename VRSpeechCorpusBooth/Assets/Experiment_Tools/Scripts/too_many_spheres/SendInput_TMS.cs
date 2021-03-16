using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SendInput_TMS : MonoBehaviour
{

    public OSC osc;
    public SteamVR_Input_Sources handType;
    public float thresh = 0.25f;
    public SteamVR_Action_Single squeezeAction;

    private ExperimentControl_TMS expControllerRef;
    private int trial_num;
    private float currTime;
    private float trlStartTime;
    private List<float> responses;
    private string xshort;
    private bool listening;
    private bool in_response_win;


    void Start()
    {
        osc.SetAddressHandler("/Set_up_trial", OnReceiveSetupTrial);
        osc.SetAddressHandler("/Play_trial", OnReceiveStartTrial);
        osc.SetAddressHandler("/Retrieve_responses", OnReceiveSendResponses);
        //osc.SetAddressHandler("/Handle_end", OnReceiveSaveResponses);
        expControllerRef = FindObjectOfType<ExperimentControl_TMS>();
        listening = false;
        in_response_win = false;
    }


    void Update()
    {
        //Debug.Log(squeezeAction.GetAxis(handType));
        if (in_response_win & listening & squeezeAction.GetAxis(handType) > thresh)
        {
            currTime = Time.time - trlStartTime;
            responses.Add(trial_num);
            responses.Add(currTime);
            //Debug.Log("TRIGGER DOWN!!!!!!!!!!!!!!");
            listening = false;
        }
        // Start listening for another response as participant releases trigger
        else if (in_response_win & !listening & squeezeAction.GetAxis(handType) > 0 & squeezeAction.GetAxis(handType) < thresh)
        {
            listening = true;
        }
    }

    void OnReceiveSetupTrial(OscMessage message)
    {
        // before trial playback, clear out the old responses
        responses = new List<float>();
        trial_num = expControllerRef.curr_trl;
    }

    void OnReceiveStartTrial(OscMessage message)
    {
        // store the start time
        trlStartTime = Time.time;
        listening = true;
        in_response_win = true;
    }

    void OnReceiveSendResponses(OscMessage message)
    {
        listening = false;
        in_response_win = false;
        // send back response data
        OscMessage reply;
        reply = new OscMessage();
        reply.address = "/response_TMS";
        for (int i = 0; i < responses.Count; i++)
        {
            Debug.Log(responses[i]);
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

    // For saving all responses at the end of the experiment
    //void OnReceiveSaveResponses(OscMessage message)
    //{
    //    listening = false; // temp!
    //}
}
