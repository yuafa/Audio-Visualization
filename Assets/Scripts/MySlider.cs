using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySlider : MonoBehaviour
{
    [SerializeField] Vector3 m_MinValue = Vector3.zero;
    [SerializeField] Vector3 m_MaxValue = Vector3.zero;
    [SerializeField] Transform m_Handle = null;

    AudioInfo m_AuidoInfo;

    Vector3 m_StartPos;
    Vector3 m_EndPos;

    private void Start()
    {
        m_AuidoInfo = FindObjectOfType<AudioInfo>();

        m_StartPos = transform.InverseTransformPoint(m_Handle.position + m_MinValue);
        m_EndPos = transform.InverseTransformPoint(m_Handle.position + m_MaxValue);
    }

    private void Update()
    {
        SetValue(m_AuidoInfo.Process);
    }

    private void SetValue(float process)
    {
        m_Handle.localPosition = Vector3.Lerp(m_StartPos, m_EndPos, process);
    }


    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + m_MinValue, 0.001f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + m_MaxValue, 0.001f);
    }
}
