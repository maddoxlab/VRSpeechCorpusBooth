using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleBreakText : MonoBehaviour
{
    public OSC osc;
    public Vector3 text_scale;
    private SendInput input_sender;

    private string OSC_address = "/Show_instructions";
    private string OSC_address2 = "/Handle_end";
    private SendInput.Hand hand;
    private OVRInput.Axis1D proceed_input;
    private bool correct_phase = false;
    private string blk_msg;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(0, 0, 0); // invisible until the break
        osc.SetAddressHandler(OSC_address, OnReceiveStartText);
        osc.SetAddressHandler(OSC_address2, OnReceiveEndExp);
        input_sender = FindObjectOfType<SendInput>();
        hand = input_sender.hand;
        if (hand.Equals(SendInput.Hand.Right))
        {
            proceed_input = OVRInput.Axis1D.SecondaryIndexTrigger;
        }
        else
        {
            proceed_input = OVRInput.Axis1D.PrimaryIndexTrigger;
        }
    }

    void Update()
    {
        float resp_trig = OVRInput.Get(proceed_input);
        if (correct_phase == true) 
        {
            if (resp_trig > 0.8)
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
    }

    void OnReceiveStartText(OscMessage message)
    {
        blk_msg = message.ToString();
        Debug.Log("received_instructions");
        Debug.Log(GetComponent<TextMesh>());
        GetComponent<TextMesh>().text = "During each trial you can press buttons but I don't remember which ones. \nPull the index trigger to continue.";
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
