using UnityEngine;

/// <summary>
/// 触屏移动信息
/// </summary>
public class PointerActionInfo : PointerInfo
{
    /// <summary>
    /// 点击屏幕的起始位置
    /// </summary>
    public Vector2 startPosition;

    /// <summary>
    /// 滑动速度，即deltas的平均值
    /// </summary>
    public Vector2 flickVelocity;

    /// <summary>
    /// 移动的总距离
    /// </summary>
    public float totalMovement;

    /// <summary>
    /// 开始接触屏幕的时间
    /// </summary>
    public float startTime;

    /// <summary>
    /// 当前输入是否拖拽
    /// </summary>
    public bool isDrag;

    /// <summary>
    /// 当前输入是否按住屏幕
    /// </summary>
    public bool isHold;

    /// <summary>
    /// 是否先按住屏幕然后拖动
    /// </summary>
    public bool wasHold;
}