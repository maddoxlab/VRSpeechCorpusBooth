using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Valve.VR;
using HTC.UnityPlugin.Vive;

public class ExperimentControl_TMS : MonoBehaviour {

    public string subID = "test_001";
    public OSC osc;
    // general
    public float[] timebase;
    public float trial_dur = 17; // 15 sec + 2 sec cue
    public int n_objects = 5;
    public List<float[]> object_colors;
    public float maxTime;
    // all trial conditions for the whole experiment
    public int[] cond_IDs;
    // individual trial condition info
    public int curr_trl;
    public int[] objIDs_trl;
    // Trackers
    public CalibrateRoom calibrator;
    public GameObject[] pucks;
    private List<float> puck_Xpositions;
    private List<float> puck_Zpositions;
    // private general vars
    private string resource_path = "C:/Users/Public/Code/Spatial_Temporal_Experiment/Sandbox2/Assets/Resources/";
    //private string resource_path = "E:/Unity/VR/unitycontroller/Spatial_Temporal_Experiment/Sandbox2/Assets/Resources/";
    private int fps = 90;
    private int cue_dur = 2; // seconds
    private int cue_frames;
    private string set_color_OSC_address;
    private string load_trl_OSC_address;
    private float obj_size = 0.3f; // < 1 constant (e.g. static size)
    // private trial specific vars
    private string objID_msg;
    private string traj_msg;
    private List<float[]> traj_x;
    private List<float[]> traj_y;
    private List<float[]> traj_z;
    private Vector3 cumulative_offset;
    private Vector3 cumulative_rotation;
    // animation curves -- referenced by AnimControl
    public AnimationCurve objSize_tgt;
    public AnimationCurve objSize_nontgt;
    public AnimationCurve objSize_aonly;
    public List<AnimationCurve> Curves_Traj_x;
    public List<AnimationCurve> Curves_Traj_y;
    public List<AnimationCurve> Curves_Traj_z;
    public List<AnimationCurve> Curves_VisEnv;

    void Start()
    {
        // Associate OSC messages with corresponding functions
        calibrator = FindObjectOfType<CalibrateRoom>();
        osc.SetAddressHandler("/Load_exp_data", OnReceiveLoadExpInfo);
        osc.SetAddressHandler("/Set object_colors", OnReceiveSetColors);
        set_color_OSC_address = "/Set_object_IDs"; // used later
        osc.SetAddressHandler(set_color_OSC_address, OnReceiveSetColors);
        load_trl_OSC_address = "/Load_trial_data";
        osc.SetAddressHandler(load_trl_OSC_address, OnReceiveLoadTrial);
        // Set some predictable parameters
        cue_frames = cue_dur * fps; // (constant across trials)
        object_colors = new List<float[]>();
        object_colors.Add(new float[] { 27f, 158f, 119f });
        object_colors.Add(new float[] { 217f, 95f, 2f });
        object_colors.Add(new float[] { 117f, 112f, 179f });
        object_colors.Add(new float[] { 231f, 41f, 138f });
        object_colors.Add(new float[] { 230f, 171f, 2f });
        curr_trl = -1;
        pucks = GameObject.FindGameObjectsWithTag("puck");
    }


    // Load condition parameters (all trials)
    void OnReceiveLoadExpInfo(OscMessage message)
    {
        // Trial timebase (for now, all trials equal length, so no dynamic file name)
        string time_path = resource_path + "VisData/Timebase.json";
        string time_fromJSON = File.ReadAllText(time_path);
        Timebase time_info = JsonUtility.FromJson<Timebase>(time_fromJSON);
        timebase = time_info.time;
        maxTime = timebase[timebase.Length - 1];
        // Conditions(0: AV aligned  --  1: AV misaligned  --  2: A-only)
        string path_expinfo = resource_path + "exp_info_" + subID + ".json";
        string expinfo_fromJSON = File.ReadAllText(path_expinfo);
        ExpParams_TMS experiment_info = JsonUtility.FromJson<ExpParams_TMS>(expinfo_fromJSON);
        cond_IDs = experiment_info.cond_IDs;

        // Assign tracker indices to tracked objects
        int tracker_ct = -1;
        var error = ETrackedPropertyError.TrackedProp_Success;
        for (uint i = 0; i < 16; i++)
        {
            var result = new System.Text.StringBuilder((int)64);
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);
            if (result.ToString().Contains("tracker"))
            {
                tracker_ct += 1;
                Debug.Log(i);
                pucks[tracker_ct].GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)i;
                //puck_Xpositions.Add(pucks[tracker_ct].transform.localPosition[0]);
                //puck_Zpositions.Add(pucks[tracker_ct].transform.localPosition[2]);
            }
        }

        // Identify pucks based on position
        //int most_left = puck_Xpositions.IndexOf(puck_Xpositions.Max());
        //int most_right = puck_Xpositions.IndexOf(puck_Xpositions.Min());
        //int most_forward = puck_Zpositions.IndexOf(puck_Zpositions.Max());
        //Debug.Log(most_left);
        //Debug.Log(most_right);
        //Debug.Log(most_forward);
        //pucks[most_left].GetComponent<IDpuck>().is_left = true;
        //pucks[most_right].GetComponent<IDpuck>().is_right = true;
        //pucks[most_forward].GetComponent<IDpuck>().is_forward = true;

        // Python can proceed once experiment info is loaded
        OSC_reply("/loaded");
    }

    // Assign object colors for this trial
    void OnReceiveSetColors(OscMessage message)
    {
        // Get the path for this trial's object order from the OSC message
        objID_msg = message.ToString();
        objID_msg = objID_msg.Substring(set_color_OSC_address.Length + 1);
        string objID_path = resource_path + "VisData/" + objID_msg;
        string objID_fromJSON = File.ReadAllText(objID_path);
        TrialObjID_TMS experiment_objID = JsonUtility.FromJson<TrialObjID_TMS>(objID_fromJSON);
        objIDs_trl = experiment_objID.objID;
        // Python can proceed once experiment info is loaded
        OSC_reply("/loaded");
    }

    // Set up visual stimuli for an individual trial
    void OnReceiveLoadTrial(OscMessage message)
    {
        curr_trl = curr_trl + 1;
        // Get the cumulative offset and rotation adjustments
        cumulative_offset = calibrator.cumulative_offset;
        cumulative_rotation = calibrator.cumulative_rotation;
        // Create constant size envelopes for cued (tgt) and uncued objects
        // FUTURE DEV: Move this into experiment setup: no dynamic envelopes
        int envSamples = timebase.Length;
        float[] tgt_env = new float[envSamples];
        float[] nontgt_env = new float[envSamples];
        float[] a_only_env = new float[envSamples];
        for (int i = 0; i < envSamples; i++)
        {
            if (i < cue_frames)
            {
                tgt_env[i] = 0.3f;
                nontgt_env[i] = 0f;
                a_only_env[i] = 0f;
            }
            else
            {
                tgt_env[i] = 0.3f;
                nontgt_env[i] = 0.3f;
                a_only_env[i] = 0f;
            }
        }
        objSize_tgt = Make_Anim_Curve(timebase, tgt_env, 0, 0);
        objSize_nontgt = Make_Anim_Curve(timebase, nontgt_env, 0, 0);
        objSize_aonly = Make_Anim_Curve(timebase, a_only_env, 0, 0);

        // Get spatial trajectory file from the OSC message
        traj_msg = message.ToString();
        traj_msg = traj_msg.Substring(load_trl_OSC_address.Length + 1);
        string traj_path = resource_path + "VisData/" + traj_msg;

        // ----- LOAD VISUAL OBJECT TRAJECTORIES -----
        string traj_fromJSON = File.ReadAllText(traj_path);
        TrialTraj_TMS traj_info = JsonUtility.FromJson<TrialTraj_TMS>(traj_fromJSON);
        // Reset trajectories from previous trial
        traj_x = new List<float[]>();
        traj_y = new List<float[]>();
        traj_z = new List<float[]>();
        Curves_Traj_x = new List<AnimationCurve>();
        Curves_Traj_y = new List<AnimationCurve>();
        Curves_Traj_z = new List<AnimationCurve>();
        Curves_VisEnv = new List<AnimationCurve>();
        // Add all object trajectories
        traj_x.Add(traj_info.obj0_x);
        traj_x.Add(traj_info.obj1_x);
        traj_x.Add(traj_info.obj2_x);
        traj_x.Add(traj_info.obj3_x);
        traj_x.Add(traj_info.obj4_x);
        traj_y.Add(traj_info.obj0_y);
        traj_y.Add(traj_info.obj1_y);
        traj_y.Add(traj_info.obj2_y);
        traj_y.Add(traj_info.obj3_y);
        traj_y.Add(traj_info.obj4_y);
        traj_z.Add(traj_info.obj0_z);
        traj_z.Add(traj_info.obj1_z);
        traj_z.Add(traj_info.obj2_z);
        traj_z.Add(traj_info.obj3_z);
        traj_z.Add(traj_info.obj4_z);

        // ----- CONSTRUCT ANIMATION CURVES -----
        for (int obj = 0; obj < n_objects; obj++)
        {
            // Visual envelopes (including cue if target)
            if (cond_IDs[curr_trl] == 2) // audio-only condition
            {
                Curves_VisEnv.Add(objSize_aonly);
            }
            else
            {
                if (obj == 0) // target
                {
                    Curves_VisEnv.Add(objSize_tgt);
                }
                else
                {
                    Curves_VisEnv.Add(objSize_nontgt);
                }
            }
            // Object trajectories
            AnimationCurve x_Curve_obj = Make_Anim_Curve(timebase, traj_x[obj], cumulative_offset[0], cumulative_rotation[0]);
            Curves_Traj_x.Add(x_Curve_obj);
            AnimationCurve y_Curve_obj = Make_Anim_Curve(timebase, traj_y[obj], cumulative_offset[1], cumulative_rotation[1]);
            Curves_Traj_y.Add(y_Curve_obj);
            AnimationCurve z_Curve_obj = Make_Anim_Curve(timebase, traj_z[obj], cumulative_offset[2], cumulative_rotation[2]);
            Curves_Traj_z.Add(z_Curve_obj);
        }

        // ----- ALL CURVES LOADED: Python may proceed -----
        OSC_reply("/loaded");
    }


    // HELPER FUNCTION: Set up an animation curve
    public static AnimationCurve Make_Anim_Curve(float[] kfTimes, float[] kfVals, float offset, float rotation)
    {
        // kfTimes: time values for keyframes (includes cue)
        // kfVales: values for anim curves (DOES NOT include cue)
        AnimationCurve currentCurve = new AnimationCurve();
        float currval;
        for (int kf = 0; kf < kfTimes.Length; kf++)
        {
            currval = kfVals[kf] - offset;
            currentCurve.AddKey(kfTimes[kf], currval);
        }
        return currentCurve;
    }


    // HELPER FUNCTION: Send a reply message to Python over OSC
    void OSC_reply(string address)
    {
        OscMessage reply;
        reply = new OscMessage();
        reply.address = address;
        reply.values.Add('1');
        osc.Send(reply);
    }
}