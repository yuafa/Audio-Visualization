using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 管理tap/drag/pinch等手势的类
/// </summary>
public class InputController : MonoBehaviour
{
    public static InputController Instance;

    public float dragThresholdTouch = 5f;
    public float dragThresholdMouse = 1f;
    public float tapTime = 0.2f;
    public float holdTime = 0.8f;
    public float mouseWheelSensitivity = 1f;
    public int trackMouseButtons = 3;
    public float flickThreshold = 2f;

    private const float k_FlickAccumulationFactor = 0.8f;
    private List<TouchInfo> m_Touches;
    private List<MouseButtonInfo> m_MouseInfos;

    public int ActiveTouchCount { get { return m_Touches.Count; } }
    public bool MouseButtonPressedThisFrame { get; private set; }
    public bool MouseMovedOnThisFrame { get; private set; }
    public bool TouchPressedThisFrame { get; private set; }
    public PointerInfo BasicMouseInfo { get; private set; }

    public event Action<PointerActionInfo> Pressed;
    public event Action<PointerActionInfo> Released;
    public event Action<PointerActionInfo> Tapped;
    public event Action<PointerActionInfo> StartedDrag;
    public event Action<PointerActionInfo> Dragged;
    public event Action<PointerActionInfo> StartedHold;
    public event Action<WheelInfo> SpunWheel;
    public event Action<PinchInfo> Pinched;
    public event Action<PointerInfo> MouseMoved;

    private void Awake()
    {
        Instance = this;

        m_Touches = new List<TouchInfo>();
        if (Input.mousePresent)
        {
            m_MouseInfos = new List<MouseButtonInfo>();
            BasicMouseInfo = new MouseCursorInfo { currentPosition = Input.mousePosition };

            for (int i = 0; i < trackMouseButtons; i++)
            {
                m_MouseInfos.Add(new MouseButtonInfo
                {
                    currentPosition = Input.mousePosition,
                    mouseButtonId = i,
                });
            }
        }

        Input.simulateMouseWithTouches = true;
    }

    private void Update()
    {
        if (BasicMouseInfo != null)
        {
            UpdateMouse();
        }

        UpdateTouches();
    }

    private void UpdateMouse()
    {
        BasicMouseInfo.previousPosition = BasicMouseInfo.currentPosition;
        BasicMouseInfo.currentPosition = Input.mousePosition;
        BasicMouseInfo.delta = BasicMouseInfo.currentPosition - BasicMouseInfo.previousPosition;
        MouseMovedOnThisFrame = BasicMouseInfo.delta.sqrMagnitude >= Mathf.Epsilon;
        MouseButtonPressedThisFrame = false;

        // 移动事件
        if (BasicMouseInfo.delta.sqrMagnitude > Mathf.Epsilon)
        {
            MouseMoved?.Invoke(BasicMouseInfo);
        }

        // 鼠标按键事件
        for (int i = 0; i < trackMouseButtons; i++)
        {
            MouseButtonInfo mouseButton = m_MouseInfos[i];
            mouseButton.delta = BasicMouseInfo.delta;
            mouseButton.previousPosition = BasicMouseInfo.previousPosition;
            mouseButton.currentPosition = BasicMouseInfo.currentPosition;
            if (Input.GetMouseButton(i))
            {
                if (!mouseButton.isDown)
                {
                    // 第一次点击
                    MouseButtonPressedThisFrame = true;
                    mouseButton.isDown = true;
                    mouseButton.startPosition = Input.mousePosition;
                    mouseButton.startTime = Time.realtimeSinceStartup;
                    mouseButton.startedOverUI = EventSystem.current.IsPointerOverGameObject(-mouseButton.mouseButtonId - 1);
                    // 重置一些参数
                    mouseButton.totalMovement = 0;
                    mouseButton.isDrag = false;
                    mouseButton.wasHold = false;
                    mouseButton.isHold = false;
                    mouseButton.flickVelocity = Vector2.zero;

                    Pressed?.Invoke(mouseButton);
                }
                else
                {
                    float moveDist = mouseButton.delta.magnitude;
                    mouseButton.totalMovement += moveDist;
                    // 拖动?
                    if (mouseButton.totalMovement > dragThresholdMouse)
                    {
                        bool wasDrag = mouseButton.isDrag;

                        mouseButton.isDrag = true;
                        if (mouseButton.isHold)
                        {
                            mouseButton.wasHold = mouseButton.isHold;
                            mouseButton.isHold = false;
                        }

                        if (!wasDrag)
                        {
                            StartedDrag?.Invoke(mouseButton);
                        }
                        Dragged?.Invoke(mouseButton);

                        // 滑动?
                        if (moveDist > flickThreshold)
                        {
                            mouseButton.flickVelocity = (mouseButton.flickVelocity * (1 - k_FlickAccumulationFactor)) + (mouseButton.delta * k_FlickAccumulationFactor);
                        }
                        else
                        {
                            mouseButton.flickVelocity = Vector2.zero;
                        }
                    }
                    else
                    {
                        // 静止的
                        if (!mouseButton.isHold && !mouseButton.isDrag && (Time.realtimeSinceStartup - mouseButton.startTime) >= holdTime)
                        {
                            mouseButton.isHold = true;
                            StartedHold?.Invoke(mouseButton);
                        }
                    }
                }
            }
            else
            {
                if (mouseButton.isDown)
                {
                    mouseButton.isDown = false;
                    if (!mouseButton.isDrag && Time.realtimeSinceStartup - mouseButton.startTime < tapTime)
                    {
                        Tapped?.Invoke(mouseButton);
                    }
                    Released?.Invoke(mouseButton);
                }
            }
        }

        // 鼠标滚轮
        if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > Mathf.Epsilon)
        {
            SpunWheel?.Invoke(new WheelInfo
            {
                zoomAmount = Input.GetAxis("Mouse ScrollWheel") * mouseWheelSensitivity
            });
        }
    }

    private void UpdateTouches()
    {
        TouchPressedThisFrame = false;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // 找到已经存在的touch，如果没有就创建一个新的
            TouchInfo existingTouch = m_Touches.FirstOrDefault(t => t.touchId == touch.fingerId);
            if (existingTouch == null)
            {
                existingTouch = new TouchInfo
                {
                    touchId = touch.fingerId,
                    startPosition = touch.position,
                    currentPosition = touch.position,
                    previousPosition = touch.position,
                    startTime = Time.realtimeSinceStartup,
                    startedOverUI = EventSystem.current.IsPointerOverGameObject(touch.fingerId),
                };
                m_Touches.Add(existingTouch);

                // 完整性检查
                Debug.Assert(touch.phase == TouchPhase.Began);
            }
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    TouchPressedThisFrame = true;
                    Pressed?.Invoke(existingTouch);
                    break;
                case TouchPhase.Moved:
                    bool wasDrag = existingTouch.isDrag;
                    UpdateMovingFinger(touch, existingTouch);

                    // 这是一个拖拽吗？
                    existingTouch.isDrag = existingTouch.totalMovement >= dragThresholdTouch;

                    if (existingTouch.isDrag)
                    {
                        if (existingTouch.isHold)
                        {
                            existingTouch.wasHold = existingTouch.isHold;
                            existingTouch.isHold = false;
                        }

                        // 是否刚刚开始
                        if (!wasDrag)
                        {
                            StartedDrag?.Invoke(existingTouch);
                        }
                        Dragged?.Invoke(existingTouch);

                        if (existingTouch.delta.sqrMagnitude > flickThreshold * flickThreshold)
                        {
                            existingTouch.flickVelocity = (existingTouch.flickVelocity * (1 - k_FlickAccumulationFactor)) + (existingTouch.delta * k_FlickAccumulationFactor);
                        }
                        else
                        {
                            existingTouch.flickVelocity = Vector2.zero;
                        }
                    }
                    else
                    {
                        UpdateHoldingFinger(existingTouch);
                    }
                    break;
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    // 可能还会移动一点
                    UpdateMovingFinger(touch, existingTouch);
                    if (!existingTouch.isDrag && (Time.realtimeSinceStartup - existingTouch.startTime) < tapTime)
                    {
                        Tapped?.Invoke(existingTouch);
                    }
                    Released?.Invoke(existingTouch);

                    m_Touches.Remove(existingTouch);
                    break;
                case TouchPhase.Stationary:
                    UpdateMovingFinger(touch, existingTouch);
                    UpdateHoldingFinger(existingTouch);
                    existingTouch.flickVelocity = Vector2.zero;
                    break;
            }
        }

        if (ActiveTouchCount >= 2 && (m_Touches[0].isDrag || m_Touches[1].isDrag))
        {
            Pinched?.Invoke(new PinchInfo
            {
                touch1 = m_Touches[0],
                touch2 = m_Touches[1]
            });
        }

    }

    private void UpdateHoldingFinger(PointerActionInfo existingTouch)
    {
        if (!existingTouch.isHold && !existingTouch.isDrag && (Time.realtimeSinceStartup - existingTouch.startTime) >= holdTime)
        {
            existingTouch.isHold = true;
            StartedHold?.Invoke(existingTouch);
        }
    }

    private void UpdateMovingFinger(Touch touch, PointerActionInfo existingTouch)
    {
        float dragDist = touch.deltaPosition.magnitude;

        existingTouch.previousPosition = existingTouch.currentPosition;
        existingTouch.currentPosition = touch.position;
        existingTouch.delta = touch.deltaPosition;
        existingTouch.totalMovement += dragDist;
    }
}