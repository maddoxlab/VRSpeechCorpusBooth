using UnityEngine;
using System.Collections;
using System.IO;


public class SetupTrial : MonoBehaviour
{

    [System.Serializable]
    public class ExperimentInfo
    {
        public float[] alldata;
    }

    public OSC osc;


    private string expInfoDir = "C:/Users/Public/Code/unitycontroller/exp_info.json";
    private string stimInfoDir = "C:/Users/Public/Code/unitycontroller/stim_data.json";

    void Start()
    {
        osc.SetAddressHandler("/Load_all_data", LoadData);
    }


    void LoadData(OscMessage message)
    {
        //Read in the experiment info
        string exp_info = File.ReadAllText(expInfoDir);
        ExperimentInfo exp = JsonUtility.FromJson<ExperimentInfo>(exp_info);

        Debug.Log("look here");
        Debug.Log(exp.alldata);
    }

}

