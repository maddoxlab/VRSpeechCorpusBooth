using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour {

    // for changing the position with hand mvmt
    public GameObject LeftHand;
    public Vector3 handpos;
    public Vector3 handfwd;
    public bool hitting;
    public Vector3 hitpoint;

    private LineRenderer lr;
	// Use this for initialization
	void Start () {
        lr = GetComponent<LineRenderer>();
	}
	
	void Update () {

        handpos = LeftHand.transform.position;
        handfwd = LeftHand.transform.forward;

        // move the laser according to hand position
        transform.position = handpos;
        transform.forward = handfwd;

        lr.SetPosition(0, handpos);

        RaycastHit hit;
        if (Physics.Raycast(handpos, handfwd, out hit, Mathf.Infinity))
        {
            lr.SetPosition(1, hit.point);
            //Debug.Log("HITTING");
            //Debug.Log(hit.point);
            hitting = true;
            hitpoint = hit.point;
        }
        else
        {
            lr.SetPosition(1, handfwd * 5000);
            //Debug.Log("NOT HITTING");
            hitting = false;
            hitpoint = new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        }
    }
}
