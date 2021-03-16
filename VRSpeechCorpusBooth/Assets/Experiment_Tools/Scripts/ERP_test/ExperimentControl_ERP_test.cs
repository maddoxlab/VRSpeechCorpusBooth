using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ExperimentControl_ERP_test : MonoBehaviour {

    public string subID;
    public OSC osc;
    // general
    // Path for running in Boston
    // private string resource_path = "E:/Unity/VR/unitycontroller/Spatial_Temporal_Experiment/Sandbox2/Assets/Resources/";
    // Path for running at UR
    private string resource_path = "C:/Users/Public/Code/Spatial_Temporal_Experiment/Sandbox2/Assets/Resources/";

    private string trl_OSC_address;
    public float[] timebase;
    public float maxTime;
    public int max_N_objects = 2;
    public int max_total_obj; // will incl. spatially incong trajectories
    private float smallSize = 0.6f; // < 1 constant (e.g. static size)
    // individual trial condition info, for simplicity in AnimControl
    public int tNum;
    public string tStr;
    public int curr_spat;
    public int curr_temp;
    public int curr_Nobj;
    public int curr_tgt;
    public int curr_side;
    public List<int> trial_objects;
    // whole experiment trial conditions
    private int[] spatial_cong;
    private int[] temporal_coh;
    private int[] n_objects;
    private int[] tgt_object;
    private int[] tgt_side;
    // private trial specific vars
    private string visenv_path;
    private List<float[]> visEnvelopes;
    private string visorthog_path;
    private List<float[]> visOrthog;
    public float tgtStartScale;
    public Vector3 tgtStartPos;
    public Vector3 distStartPos;

    // animation curves -- referenced by AnimControl!
    public AnimationCurve emptyAnim; // all zeros
    public AnimationCurve noGainAnim; // all ones
    public AnimationCurve smallConstAnim; // constant but smaller than one
    public List<AnimationCurve> Curves_VisEnv;
    public List<AnimationCurve> Curves_VisOrthog;

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
        string path_expinfo = resource_path + "exp_info_" + subID + ".json";
        Debug.Log(path_expinfo);
        string expinfo_fromJSON = File.ReadAllText(path_expinfo);
        ExpParams_ERP_test experiment_info = JsonUtility.FromJson<ExpParams_ERP_test>(expinfo_fromJSON);
        spatial_cong = experiment_info.spat_cong;
        temporal_coh = experiment_info.temp_coh;
        n_objects = experiment_info.n_obj;
        tgt_object = experiment_info.tgt_obj;
        tgt_side = experiment_info.tgt_side;
        // Python can proceed once experiment info is loaded
        OSC_reply("/loaded");
    }


    // Set up visual stimuli for an individual trial
    void OnReceiveLoadTrial(OscMessage message)
    {
        max_total_obj = max_N_objects * 2;
        // Get trial number (tNum) from the OSC message
        tStr = message.ToString();
        tStr = tStr.Substring(trl_OSC_address.Length + 1);
        Debug.Log("Trial Number" + tStr);
        tNum = int.Parse(tStr);

        // Pull specific condition info for this trial
        curr_spat = spatial_cong[tNum];
        curr_temp = temporal_coh[tNum];
        curr_Nobj = n_objects[tNum];
        int curr_Ndist = curr_Nobj - 1;
        curr_tgt = tgt_object[tNum];
        curr_side = tgt_side[tNum];

        // Figure out what objects to set this trial
        trial_objects = new List<int>();
        List<int> poss_distractors = new List<int>();
        List<int> poss_dist_shuff = new List<int>();
        for (int i = 0; i < max_N_objects; i++)
        {
            if (i == curr_tgt)
            {
                trial_objects.Add(i); // target must be in list
            }
            else // it's a possible distractor
            {
                poss_distractors.Add(i);
            }
        }
        poss_dist_shuff = Fisher_Yates_Shuffle(poss_distractors);
        for (int i = 0; i < curr_Ndist; i++)
        {
            trial_objects.Add(poss_dist_shuff[i]);
        }

        // Load trial timebase (for now, all trials equal length, so no dynamic file name)
        string time_path = resource_path + "VisData/Timebase.json";
        string time_fromJSON = File.ReadAllText(time_path);
        Timebase time_info = JsonUtility.FromJson<Timebase>(time_fromJSON);
        timebase = time_info.time;
        maxTime = timebase[timebase.Length - 1];

        // Create fake envelopes of same length as timebase
        int envSamples = timebase.Length;
        float[] fakeEnv = new float[envSamples];
        float[] onesEnv = new float[envSamples];
        float[] smallEnv = new float[envSamples];
        for (int i = 0; i < envSamples; i++)
        {
            fakeEnv[i] = 0;
            onesEnv[i] = 1;
            smallEnv[i] = smallSize;
        }
        emptyAnim = Make_Anim_Curve(timebase, fakeEnv);
        noGainAnim = Make_Anim_Curve(timebase, onesEnv);
        smallConstAnim = Make_Anim_Curve(timebase, smallEnv);

        // ----- LOAD VISUAL ENVELOPES -----
        visEnvelopes =  new List<float[]>(); // Reset the envelopes
        Curves_VisEnv = new List<AnimationCurve>();
        // in this version, incoherent envelopes are FLAT
        if (curr_temp == 1) // temporally coherent
        {
            visenv_path = resource_path + "VisData/VisEnv_coh_trl_" + tStr + ".json";
            string visenv_fromJSON = File.ReadAllText(visenv_path);
            VisEnv visenv_info = JsonUtility.FromJson<VisEnv>(visenv_fromJSON);
            visEnvelopes.Add(visenv_info.env_obj0);
            visEnvelopes.Add(visenv_info.env_obj1);
            visEnvelopes.Add(visenv_info.env_obj2);
            // Save target initialization value
            tgtStartScale = visEnvelopes[curr_tgt][0];
        }
        else // temporally incoherent
        {
            tgtStartScale = smallSize; // target size is constant
        }
        
        // ----- LOAD VISUAL ORTHOGONAL FEATURE CURVES -----
        visOrthog = new List<float[]>(); // Reset orthog features
        Curves_VisOrthog = new List<AnimationCurve>();
        visorthog_path = resource_path + "VisData/Vorthog_trl_" + tStr + ".json";
        string visorthog_fromJSON = File.ReadAllText(visorthog_path);
        OrthogFeats visorthog_info = JsonUtility.FromJson<OrthogFeats>(visorthog_fromJSON);
        visOrthog.Add(visorthog_info.Vorthog_obj0);
        visOrthog.Add(visorthog_info.Vorthog_obj1);
        // visOrthog.Add(visorthog_info.Vorthog_obj2);

        // ----- SET TARGET START POSITION -----
        if (curr_side == 0) // target left
        {
            tgtStartPos = new Vector3(-5, 2, 8);
            distStartPos = new Vector3(5, 2, 8);
        }
        else if (curr_side == 1) // target right
        {
            tgtStartPos = new Vector3(5, 2, 8);
            distStartPos = new Vector3(-5, 2, 8);
        }

        // ----- CONSTRUCT ANIMATION CURVES -----
        for (int obj = 0; obj < max_N_objects; obj++)
        {
            if (trial_objects.Contains(obj)) // this is an object to be drawn
            {
                // visual envelopes (temporally coherent)
                if (curr_temp == 1)
                {
                    AnimationCurve envCurve_obj = Make_Anim_Curve(timebase, visEnvelopes[obj]);
                    Curves_VisEnv.Add(envCurve_obj);
                }
                else // temporally incoherent -- envelopes are flat
                {
                    Curves_VisEnv.Add(smallConstAnim);
                }
                // visual orthogonal features
                AnimationCurve orthogCurve_obj = Make_Anim_Curve(timebase, visOrthog[obj]);
                Curves_VisOrthog.Add(orthogCurve_obj);
            }
            else // put in a flat, all-zeros animation curve
            {
                Curves_VisEnv.Add(emptyAnim);
                Curves_VisOrthog.Add(noGainAnim);
            }  
        }

        // ----- ALL CURVES LOADED: Python may proceed -----
        OSC_reply("/loaded");
    }

    // HELPER FUNCTION: Set up an animation curve
    public static AnimationCurve Make_Anim_Curve(float[] kfTimes, float[] kfVals)
    {
        AnimationCurve currentCurve = new AnimationCurve();
        for (int kf = 0; kf < kfTimes.Length; kf++)
        {
            currentCurve.AddKey(kfTimes[kf], kfVals[kf]);
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