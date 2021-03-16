using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ExperimentControl : MonoBehaviour {

    public string subID = "test";
    public OSC osc;
    // general
    //private string resource_path = "E:/Unity/VR/unitycontroller/Spatial_Temporal_Experiment/Sandbox2/Assets/Resources/";
    private string resource_path = "C:/Users/Public/Code/VR_Experiments/Basic_booth/Assets/Resources/";
    private string trl_OSC_address;
    public float[] timebase;
    public float maxTime;
    public bool python_Ready;
    private float smallSize = 0.1f; // < 1 constant (e.g. static size)
    // individual trial condition info, for simplicity in AnimControl

    // all trial conditions for the whole experiment

    private string traj_path;
    private List<float[]> traj_x;
    private List<float[]> traj_y;
    private List<float[]> traj_z;
    private bool isTgt;

    // animation curves -- referenced by AnimControl
    public AnimationCurve emptyAnim; // all zeros
    public AnimationCurve noGainAnim; // all ones
    public AnimationCurve smallConstAnim; // constant but smaller than one
    public List<AnimationCurve> Curves_Traj_x;
    public List<AnimationCurve> Curves_Traj_y;
    public List<AnimationCurve> Curves_Traj_z;

    void Start()
    {
        // Associate OSC messages with corresponding functions
        osc.SetAddressHandler("/Load_exp_data", OnReceiveLoadExpInfo);
        trl_OSC_address = "/Load_trial_data";
        osc.SetAddressHandler(trl_OSC_address, OnReceiveLoadTrial);
    }


    // Load condition parameters
    void OnReceiveLoadExpInfo(OscMessage message)
    {
        python_Ready = true;

        // Python can proceed once experiment info is loaded
        OSC_reply("/loaded");
    }


    // Set up visual stimuli for an individual trial
    void OnReceiveLoadTrial(OscMessage message)
    {
        // Load trial timebase (for now, all trials equal length, so no dynamic file name)
        string time_path = resource_path + "visual_stim_timing.json";
        string time_fromJSON = File.ReadAllText(time_path);
        Timebase time_info = JsonUtility.FromJson<Timebase>(time_fromJSON);
        timebase = time_info.time;
        maxTime = timebase[timebase.Length - 1];


        // ----- LOAD CONGRUENT TRAJECTORIES -----
        traj_x = new List<float[]>(); // Reset trajectories
        traj_y = new List<float[]>();
        traj_z = new List<float[]>();
        Curves_Traj_x = new List<AnimationCurve>();
        Curves_Traj_y = new List<AnimationCurve>();
        Curves_Traj_z = new List<AnimationCurve>();
        traj_path = resource_path + "visual_stim_trajectory.json";
        string traj_fromJSON = File.ReadAllText(traj_path);
        SpatTrajectories traj_info = JsonUtility.FromJson<SpatTrajectories>(traj_fromJSON);

        // ----- CONSTRUCT ANIMATION CURVES -----

        int cue = 0;

        // visual trajectories

        AnimationCurve x_Curve_obj = Make_Anim_Curve(timebase, traj_info.traj_x, cue);
        Curves_Traj_x.Add(x_Curve_obj);
        AnimationCurve y_Curve_obj = Make_Anim_Curve(timebase, traj_info.traj_y, cue);
        Curves_Traj_y.Add(y_Curve_obj);
        AnimationCurve z_Curve_obj = Make_Anim_Curve(timebase, traj_info.traj_z, cue);
        Curves_Traj_z.Add(z_Curve_obj);

        // ----- ALL CURVES LOADED: Python may proceed -----
        OSC_reply("/loaded");
    }


    // HELPER FUNCTION: Set up an animation curve
    public static AnimationCurve Make_Anim_Curve(float[] kfTimes, float[] kfVals, int cue_samples)
    {
        // kfTimes: time values for keyframes (includes cue)
        // kfVales: values for anim curves (DOES NOT include cue)
        // cue_samples: first [cue_samples] frames correspond to the cue
        // isT: determines whether the cue should be visible or set to nothing
        AnimationCurve currentCurve = new AnimationCurve();
        int trl_samp = 0;

        float init_val = kfVals[0];
        for (int kf = 0; kf < kfTimes.Length; kf++)
        {
            if (kf < cue_samples)
            {
                currentCurve.AddKey(kfTimes[kf], init_val);
            }
            else // start drawing actual animation curve
            {
                currentCurve.AddKey(kfTimes[kf], kfVals[trl_samp]);
                trl_samp += 1;
            }
        }

        return currentCurve;
    }


    // HELPER FUNCTION: Fisher Yates shuffling
    public static List<int> Fisher_Yates_Shuffle(List<int> aList)
    {
        System.Random _random = new System.Random();
        int myGO;
        int n = aList.Count;
        for (int i = 0; i < n; i++)
        {
            // NextDouble returns a random number between 0 and 1.
            // ... It is equivalent to Math.random() in Java.
            int r = i + (int)(_random.NextDouble() * (n - i));
            myGO = aList[r];
            aList[r] = aList[i];
            aList[i] = myGO;
        }
        return aList;
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