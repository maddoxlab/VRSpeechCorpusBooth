using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimControl : MonoBehaviour
{

    public OSC osc;

    private ExperimentControl expControllerRef;
    private string curr_trl;
    private List<int> trial_objects;
    private MakeLabel objLabel;
    private int objType;
    private string objIDstr;
    private int objID;
    private int curr_spat;
    private int curr_temp;
    private float maxTime;
    private string fname;
    // Animation Curves for this object
    private AnimationCurve my_visEnv;
    private AnimationCurve my_visOrthog;
    private AnimationCurve my_traj_x;
    private AnimationCurve my_traj_y;
    private AnimationCurve my_traj_z;


    // Start is called before the first frame update
    void Start()
    {
        osc.SetAddressHandler("/Set_up_trial", OnReceiveReadyTrial);
        osc.SetAddressHandler("/Play_trial", OnReceivePlayTrial);
        expControllerRef = FindObjectOfType<ExperimentControl>();
    }

    void OnReceiveReadyTrial(OscMessage message)
    {
        // Gather trial parameters
        maxTime = expControllerRef.maxTime;
        objLabel = GetComponent<MakeLabel>();
        objType = objLabel.shape_properties_label;
        objIDstr = objLabel.filename_label;
        objID = int.Parse(objIDstr);

        // * * * * *
        // SET VISUAL TRIAL PARAMETERS
        // * * * * *

        my_traj_x = expControllerRef.Curves_Traj_x[objID];
        my_traj_y = expControllerRef.Curves_Traj_y[objID];
        my_traj_z = expControllerRef.Curves_Traj_z[objID];


        // Loaded. Only one of the objects that loaded sound sends OSC reply
        if (objID == 0)
        {
            OscMessage reply;
            reply = new OscMessage();
            reply.address = "/loaded";
            reply.values.Add('1');
            osc.Send(reply);
        }
    }

    void OnReceivePlayTrial(OscMessage message)
    {
        // Start the co-routine for visual stimuli
        StartCoroutine(applyCurves(my_visEnv, my_visOrthog, my_traj_x, my_traj_y, 
            my_traj_z, maxTime));
    }

    IEnumerator applyCurves(AnimationCurve ve, AnimationCurve or, AnimationCurve x, 
        AnimationCurve y, AnimationCurve z, float overSeconds)
    {
        float startTime = Time.time;
        float endTime = startTime + overSeconds;

        while (Time.time < endTime)
        {
            float elapsedTime = Time.time - startTime;
            float xVal = x.Evaluate(elapsedTime);
            float yVal = y.Evaluate(elapsedTime);
            float zVal = z.Evaluate(elapsedTime);
  
            transform.localPosition = new Vector3(xVal, yVal, zVal);

            yield return null;
        }

        //// target object only: send trial finished message
        //if (objType == "Target" && expControllerRef.curr_tgt == objID)
        //{
        //    OscMessage reply;
        //    reply = new OscMessage();
        //    reply.address = "/playback_done";
        //    reply.values.Add('1');
        //    osc.Send(reply);
        //}
    }
}
