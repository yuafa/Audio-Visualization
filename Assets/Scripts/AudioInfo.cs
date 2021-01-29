using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AudioInfo : MonoBehaviour
{
    [SerializeField] AudioClip[] m_AudioClips = null;
    [SerializeField] TextMeshPro m_SoundTimeText = null;

    int m_CurrentHour;
    int m_CurrentMinute;
    int m_CurrentSecond;
    AudioSource m_AudioSource;

    int m_Index;

    public float Process
    {
        get;
        private set;
    }

    private void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
        m_Index = Random.Range(0, m_AudioClips.Length);
        m_AudioSource.clip = m_AudioClips[m_Index];

        m_AudioSource.Play();
    }

    public void Next()
    {
        if (m_Index < m_AudioClips.Length - 1)
        {
            m_Index++;
            m_AudioSource.clip = m_AudioClips[m_Index];
            m_AudioSource.Play();
            return;
        }

        m_Index = 0;
        m_AudioSource.clip = m_AudioClips[m_Index];
        m_AudioSource.Play();
    }

    public void Prev()
    {
        if (m_Index > 0)
        {
            m_Index--;
            m_AudioSource.clip = m_AudioClips[m_Index];
            m_AudioSource.Play();
            return;
        }

        m_Index = m_AudioClips.Length - 1;
        m_AudioSource.clip = m_AudioClips[m_Index];
        m_AudioSource.Play();
    }

    private void Update()
    {
        m_CurrentHour = (int)m_AudioSource.time / 3600;
        m_CurrentMinute = (int)(m_AudioSource.time - m_CurrentHour * 3600) / 60;
        m_CurrentSecond = (int)(m_AudioSource.time - m_CurrentHour * 3600 - m_CurrentMinute * 60);

        m_SoundTimeText.text = string.Format("{0:D2}:{1:D2}", m_CurrentMinute, m_CurrentSecond);

        Process = m_AudioSource.time / m_AudioSource.clip.length;
    }
}
