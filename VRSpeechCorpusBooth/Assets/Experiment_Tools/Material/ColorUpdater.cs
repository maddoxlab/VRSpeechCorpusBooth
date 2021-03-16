using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorUpdater : MonoBehaviour
{
    public Color objColor;
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_Color", objColor);
        _renderer.SetPropertyBlock(_propBlock);
    }
}
