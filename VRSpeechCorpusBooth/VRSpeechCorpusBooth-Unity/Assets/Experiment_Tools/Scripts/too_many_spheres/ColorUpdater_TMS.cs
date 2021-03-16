using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorUpdater_TMS : MonoBehaviour
{
    public OSC osc;
    public Color objColor;

    private ExperimentControl_TMS expControllerRef;
    private MakeLabel objLabel;
    private string objIDstr;
    private int objID;
    private int colorID;
    private float[] colorVal;

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    void Start()
    {
        osc.SetAddressHandler("/Set_object_colors", OnReceiveSetup);
        expControllerRef = FindObjectOfType<ExperimentControl_TMS>();
        objLabel = GetComponent<MakeLabel>();
        objIDstr = objLabel.filename_label;
        objID = int.Parse(objIDstr);
    }

    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
    }

    void OnReceiveSetup(OscMessage message)
    {
        colorID = expControllerRef.objIDs_trl[objID]; // map this object to trial-specific color
        colorVal = expControllerRef.object_colors[colorID]; // get RGB
        objColor = new Color(colorVal[0] / 255, colorVal[1] / 255, colorVal[2] / 255);

        if (objID == 4)
        {
            OscMessage reply;
            reply = new OscMessage();
            reply.address = "/loaded";
            reply.values.Add('1');
            osc.Send(reply);
        }
    }

    void Update()
    {
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_Color", objColor);
        _renderer.SetPropertyBlock(_propBlock);
    }
}
