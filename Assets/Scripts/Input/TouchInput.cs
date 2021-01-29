using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TouchInput : MonoBehaviour
{
    public ARRaycastManager m_ARRayCastManager;
    public Transform m_AudioGo;
    public Transform m_LightGo;
    public AudioInfo m_AudioInfo;


    public float scaleSpeed = 1f;
    public float rotateSpeed = 1f;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private void OnEnable()
    {
        InputController controller = GetComponent<InputController>();
        controller.Pressed += OnPress;
        controller.StartedHold += OnStartedHold;
        controller.Dragged += OnDrag;
        controller.Pinched += OnPinch;
    }

    private void OnDisable()
    {
        InputController controller = GetComponent<InputController>();
        controller.Pressed -= OnPress;
        controller.StartedHold -= OnStartedHold;
        controller.Dragged -= OnDrag;
        controller.Pinched -= OnPinch;
    }

    private void OnPress(PointerActionInfo pointer)
    {
        if (pointer.startedOverUI) return;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(pointer.currentPosition), out RaycastHit hitInfo))
        {
            if (hitInfo.collider.gameObject.CompareTag("Prev"))
            {
                m_AudioInfo.Prev();
                return;
            }

            if (hitInfo.collider.gameObject.CompareTag("Next"))
            {
                m_AudioInfo.Next();
                return;
            }
        };
    }

    private void OnStartedHold(PointerActionInfo pointer)
    {
        Debug.Log("OnStartedHold");
        SetAudioGoPosition(pointer);
    }

    private void OnDrag(PointerActionInfo pointer)
    {
        DoAudioGoRotate(pointer);
    }

    private void OnPinch(PinchInfo pinch)
    {
        DoAudioGoScale(pinch);
    }

    /// <summary>
    /// 尝试设置AR物体位置
    /// </summary>
    protected void SetAudioGoPosition(PointerActionInfo pointer)
    {
        if (pointer.startedOverUI) return;

        if (m_ARRayCastManager.Raycast(pointer.currentPosition, s_Hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = s_Hits[0].pose;
            m_AudioGo.transform.position = hitPose.position;
            Application.targetFrameRate = 60;
        }
        else
        {
            Debug.Log("未能找到平面");
        }
    }

    /// <summary>
    /// AR物体旋转
    /// </summary>
    protected void DoAudioGoRotate(PointerActionInfo pointer)
    {
        if (pointer.startedOverUI) return;

        TouchInfo touchInfo = (TouchInfo)pointer;

        if (touchInfo != null)
        {
            Vector2 direction = touchInfo.currentPosition - touchInfo.previousPosition;
            m_AudioGo.Rotate(new Vector3(0, -direction.x * rotateSpeed * Time.deltaTime, 0f));
            m_LightGo.Rotate(new Vector3(0, -direction.y * rotateSpeed * Time.deltaTime, 0f));
        }

    }

    /// <summary>
    /// AR物体缩放
    /// </summary>
    protected void DoAudioGoScale(PinchInfo pinch)
    {
        if (pinch.touch1.startedOverUI && pinch.touch2.startedOverUI) return;

        float currentDistance = (pinch.touch1.currentPosition - pinch.touch2.currentPosition).magnitude;
        float prevDistance = (pinch.touch1.previousPosition - pinch.touch2.previousPosition).magnitude;

        float zoomChange = currentDistance - prevDistance;

        float scale = m_AudioGo.localScale.x + zoomChange * scaleSpeed * Time.deltaTime;
        scale = Mathf.Clamp(scale, 0.3f, 10f);

        m_AudioGo.localScale = Vector3.one * scale;
    }
}