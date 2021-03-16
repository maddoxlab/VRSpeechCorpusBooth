using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class CalibrateRoom : MonoBehaviour
{
    public OSC osc;
    public Vector3 cumulative_offset = new Vector3(0, 0, 0);
    public Vector3 cumulative_rotation = new Vector3(0, 0, 0);

    // game objects to manipulate or track
    public GameObject virtual_puck0;
    public GameObject virtual_puck1;
    public GameObject virtual_puck2;
    public GameObject room;
    public GameObject tracked_puck0;
    public GameObject tracked_puck1;
    public GameObject tracked_puck2;
    // virtual pucks
    private Vector3 virtual_puck0_loc;
    private Vector3 virtual_puck1_loc;
    private Vector3 virtual_puck2_loc;
    private Vector3 a;
    private Vector3 b;
    private Vector3 norm;
    // real pucks
    private Vector3 tracked_puck0_loc;
    private Vector3 tracked_puck1_loc;
    private Vector3 tracked_puck2_loc;
    private Vector3 a_tracked;
    private Vector3 b_tracked;
    private Vector3 norm_tracked;
    // calculation variables
    private Vector3 offset;
    private List<Vector3> all_offsets;
    private Vector3 norm_diff;

    void Start()
    {
        osc.SetAddressHandler("/Calibrate", OnReceiveCal);
        // GameObject references
        //virtual_puck0 = GameObject.Find("virtual_puck0");
        //virtual_puck1 = GameObject.Find("virtual_puck1");
        //virtual_puck2 = GameObject.Find("virtual_puck2");
        //room = GameObject.Find("Floor");
        //tracked_puck0 = GameObject.Find("Puck0");
        //tracked_puck1 = GameObject.Find("Puck1");
        //tracked_puck2 = GameObject.Find("Puck2");
        uint index = 0;
        var error = ETrackedPropertyError.TrackedProp_Success;
        for (uint i = 0; i < 16; i++)
        {
            var result = new System.Text.StringBuilder((int)64);
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);
            //Debug.Log("device: " + result);
            if (result.ToString().Contains("tracker"))
            {
                index = i;
                Debug.Log(index);
            }
        }
    }

    void OnReceiveCal(OscMessage message)
    {
        // Get the current position and rotation of the room
        
        // Get virtual and physical puck locations
        virtual_puck0_loc = virtual_puck0.transform.position;
        virtual_puck1_loc = virtual_puck1.transform.position;
        virtual_puck2_loc = virtual_puck2.transform.position;
        tracked_puck0_loc = tracked_puck0.transform.position;
        tracked_puck1_loc = tracked_puck1.transform.position;
        tracked_puck2_loc = tracked_puck2.transform.position;
        // Compute average offset between virtual and tracked pucks
        all_offsets = new List<Vector3>();
        all_offsets.Add(virtual_puck0_loc - tracked_puck0_loc);
        all_offsets.Add(virtual_puck1_loc - tracked_puck1_loc);
        all_offsets.Add(virtual_puck2_loc - tracked_puck2_loc);
        offset = GetMeanVector(all_offsets);
        cumulative_offset += offset;
        // Compute normal vectors of planes defined by virtual and tracked pucks
        a = virtual_puck1_loc - virtual_puck0_loc;
        b = virtual_puck2_loc - virtual_puck0_loc;
        norm = Vector3.Cross(a, b).normalized;
        a_tracked = tracked_puck1_loc - tracked_puck0_loc;
        b_tracked = tracked_puck2_loc - tracked_puck0_loc;
        norm_tracked = Vector3.Cross(a_tracked, b_tracked).normalized;
        // Difference vector (amount of rotation required)
        norm_diff = norm - norm_tracked;
        // Adjust the room
        //Debug.Log("on receiving calibration");
        //Debug.Log(virtual_puck0.transform.position);
        //Debug.Log(virtual_puck1.transform.position);
        //Debug.Log(virtual_puck2.transform.position);
        room.transform.localPosition = room.transform.localPosition - offset;
        // Go-ahead reply
        OscMessage reply;
        reply = new OscMessage();
        reply.address = "/calibrated";
        reply.values.Add('1');
        osc.Send(reply);
        //Debug.Log(virtual_puck0.transform.position);
        //Debug.Log(virtual_puck1.transform.position);
        //Debug.Log(virtual_puck2.transform.position);

    }

    private Vector3 GetMeanVector(List<Vector3> positions)
    {
        if (positions.Count == 0)
            return Vector3.zero;

        float x = 0f;
        float y = 0f;
        float z = 0f;

        foreach (Vector3 pos in positions)
        {
            x += pos.x;
            y += pos.y;
            z += pos.z;
        }
        return new Vector3(x / positions.Count, y / positions.Count, z / positions.Count);
    }
}