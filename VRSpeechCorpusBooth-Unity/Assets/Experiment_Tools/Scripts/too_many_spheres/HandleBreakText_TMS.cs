using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class HandleBreakText_TMS : MonoBehaviour
{
    public OSC osc;
    public Vector3 text_scale;
    public SteamVR_Input_Sources handType;
    public float thresh = 0.25f;
    public SteamVR_Action_Single squeezeAction;

    private string OSC_address = "/Handle_break";
    private string OSC_address2 = "/Handle_end";
    private bool correct_phase;
    private string blk_msg;

    // Start is called before the first frame update
    void Start()
    {
        correct_phase = false;
        transform.localScale = new Vector3(0, 0, 0); // invisible until the break
        osc.SetAddressHandler(OSC_address, OnReceiveStartBreak);
        osc.SetAddressHandler(OSC_address2, OnReceiveEndExp);
    }

    void Update()
    {
        // Check if trigger pulled during block
        if (correct_phase == true & squeezeAction.GetAxis(handType) > thresh) 
        {
            transform.localScale = new Vector3(0, 0, 0);
            correct_phase = false;
            OscMessage reply;
            reply = new OscMessage();
            reply.address = "/ready_to_go";
            reply.values.Add('1');
            osc.Send(reply);
        }
    }

    void OnReceiveStartBreak(OscMessage message)
    {
        blk_msg = message.ToString();
        blk_msg = blk_msg.Substring(OSC_address.Length + 1);
        int split_ind = blk_msg.IndexOf(" ");
        string curr_blk = blk_msg.Substring(0, split_ind);
        string num_blks = blk_msg.Substring(split_ind + 1);
        Debug.Log(GetComponent<TextMesh>());
        GetComponent<TextMesh>().text = "Start of block " + curr_blk + 
            " out of " + num_blks + "\nPull the index trigger to continue.";
        transform.localScale = text_scale;
        correct_phase = true;
    }

    void OnReceiveEndExp(OscMessage message)
    {
        GetComponent<TextMesh>().text = "Experiment Complete!" +
            "\nYou may now carefully remove the headset";
        transform.localScale = text_scale;
    }
}
