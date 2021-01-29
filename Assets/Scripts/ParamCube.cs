using System;
using UnityEngine;

public class ParamCube : MonoBehaviour
{
    public int _band;
    public float _startScale;
    public float _scaleMultiplier;
    public bool _useBuffer = true;

    [SerializeField][ColorUsage(true,true)] Color originalColor = Color.blue;
    [SerializeField] [ColorUsage(true, true)] Color targetColor = Color.white;

    float yScale = 1f;

    Renderer m_CubeRender;
    MaterialPropertyBlock m_Block;

    private void Start()
    {
        m_CubeRender = GetComponentInChildren<Renderer>();
        m_Block = new MaterialPropertyBlock();
    }


    private void FixedUpdate()
    {
        if (_useBuffer)
        {
            yScale = AudioPeer._bandBuffer[_band] * _scaleMultiplier + _startScale;
            transform.localScale = new Vector3(transform.localScale.x, (AudioPeer._bandBuffer[_band] * _scaleMultiplier) + _startScale, transform.localScale.z);
        }
        else
        {
            yScale = (AudioPeer._freqBand[_band] * _scaleMultiplier) + _startScale;
        }

        Color color = Color.Lerp(originalColor, targetColor, Mathf.Clamp01(yScale / 15f));
        m_Block.SetColor("_BaseColor", color);
        m_Block.SetColor("_EmissionColor", color);
        m_CubeRender.SetPropertyBlock(m_Block);

        transform.localScale = new Vector3(transform.localScale.x, yScale, transform.localScale.z);

    }
}
