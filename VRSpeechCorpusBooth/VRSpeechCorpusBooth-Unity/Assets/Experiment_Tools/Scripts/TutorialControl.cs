using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TutorialControl : MonoBehaviour
{
    public OSC osc;
    public enum Hand { Right, Left };
    public Hand hand;
    // visible targets
    public GameObject o1;
    public GameObject o2;
    public GameObject o3;
    // invisible trajectories, for spatially incong sound
    public GameObject it1;
    public GameObject it2;
    public GameObject it3;
    public GameObject contText;
    public Vector3 text_scale;

    private string resource_path = "E:/Unity/VR/unitycontroller/Spatial_Temporal_Experiment/" +
        "Sandbox2/Assets/Resources/";
    private string OSC_address1 = "/Setup_firstplay";
    private OVRInput.Axis1D proceed_input;
    private OVRInput.Button aud_button;
    private OVRInput.Button vis_button;
    private float proc_trig;
    private bool a_but;
    private bool v_but;
    private IEnumerator coroutine;


    void Start()
    {
        // link OSC addresses to functions
        osc.SetAddressHandler(OSC_address1, OnReceiveFirstPlay);
        // set up buttons according to selected hand
        if (hand.Equals(Hand.Right))
        {
            proceed_input = OVRInput.Axis1D.SecondaryIndexTrigger;
            aud_button = OVRInput.Button.One;
            vis_button = OVRInput.Button.Two;
        }
        else
        {
            proceed_input = OVRInput.Axis1D.PrimaryIndexTrigger;
            aud_button = OVRInput.Button.Three;
            vis_button = OVRInput.Button.Four;
        }
        // initialize all objects to size zero
        o1.transform.localScale = new Vector3(0, 0, 0);
        o2.transform.localScale = new Vector3(0, 0, 0);
        o3.transform.localScale = new Vector3(0, 0, 0);
        it1.transform.localScale = new Vector3(0, 0, 0);
        it2.transform.localScale = new Vector3(0, 0, 0);
        it3.transform.localScale = new Vector3(0, 0, 0);
    }

    void OnReceiveFirstPlay(OscMessage message)
    {
        coroutine = RunTutorialS1();
        StartCoroutine(coroutine);
    }

    IEnumerator RunTutorialS1()
    {
        // initialize the text
        string t1 = "In this task, you'll be tracking a target \nsphere through" +
            " the virtual room.";
        GetComponent<TextMesh>().text = t1;
        transform.localScale = text_scale;
        yield return new WaitForSeconds(2.0f);
        // show the first (Target) object
        o1.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
        yield return new WaitForSeconds(2.0f);
        // show the "pull trigger to continue" text
        contText.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        string t2 = "Sometimes there will also be distractor spheres," +
            "\nwhich you must ignore.";
        while (OVRInput.Get(proceed_input) < 0.8)
        {
            yield return null;
        }
        contText.transform.localScale = new Vector3(0, 0, 0);
        GetComponent<TextMesh>().text = t2;
        yield return new WaitForSeconds(2.0f);
        o2.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
        o3.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
        yield return new WaitForSeconds(2.0f);
        contText.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        string t3 = "Each sphere will also have an accompanying tone." +
            "\nHave a listen.";
        while (OVRInput.Get(proceed_input) < 0.8)
        {
            yield return null;
        }
        contText.transform.localScale = new Vector3(0, 0, 0);
        GetComponent<TextMesh>().text = t3;
        yield return new WaitForSeconds(2.0f);
        // now time to sequentially play each object's audio...
        AudioSource o1audio = o1.GetComponent<AudioSource>();
        AudioSource o2audio = o2.GetComponent<AudioSource>();
        AudioSource o3audio = o3.GetComponent<AudioSource>();
        Debug.Log(o1audio);
        Debug.Log(o2audio);
        Debug.Log(o3audio);

        o1audio.Play(0);
        yield return new WaitForSeconds(2.0f);
        o2audio.Play(0);
        yield return new WaitForSeconds(2.0f);
        o3audio.Play(0);
        yield return new WaitForSeconds(2.0f);
        contText.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        while (OVRInput.Get(proceed_input) < 0.8)
        {
            yield return null;
        }

        // Tutorial segment finished
        yield return null;
        OscMessage reply;
        reply = new OscMessage();
        reply.address = "/loaded";
        reply.values.Add('1');
        osc.Send(reply);
    }


    //// Update is called once per frame
    //void Update()
    //{
    //    proc_trig = OVRInput.Get(proceed_input);
    //    a_but = OVRInput.Get(aud_button);
    //    v_but = OVRInput.Get(vis_button);
    //}
}
