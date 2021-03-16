using UnityEngine;
using System.Collections;

public class ReceiveProperties : MonoBehaviour {
    
   	public OSC osc;


	// Use this for initialization
	void Start () {
        MakeLabel label = GetComponent<MakeLabel>();
	    osc.SetAddressHandler("/UnityShape" + label.shape_properties_label, OnReceiveParams);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnReceiveParams(OscMessage message){

		float pos_x = message.GetFloat(0);
        float pos_y = message.GetFloat(1);
		float pos_z = message.GetFloat(2);

		transform.position = new Vector3(pos_x, pos_y, pos_z);

        float rot_x = message.GetFloat(3);
        float rot_y = message.GetFloat(4);
        float rot_z = message.GetFloat(5);
        
        transform.eulerAngles = new Vector3(rot_x, rot_y, rot_z);

        float sca_x = message.GetFloat(6);
        float sca_y = message.GetFloat(7);
        float sca_z = message.GetFloat(8);

        transform.localScale = new Vector3(sca_x, sca_y, sca_z);

        if (this.GetComponent<Renderer>())
        {
            float r = message.GetFloat(9);
            float g = message.GetFloat(10);
            float b = message.GetFloat(11);
            float a = message.GetFloat(12);
            Debug.Log(r);
            this.GetComponent<Renderer>().material.SetColor("_Color", new Vector4(r, g, b, a));

            int int_v = message.GetInt(13);
            if (int_v == 1)
            {
                this.GetComponent<Renderer>().enabled = true;
            }
            else
            {
                this.GetComponent<Renderer>().enabled = false;
            }
        }
    }

}
