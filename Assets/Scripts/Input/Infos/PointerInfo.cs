using UnityEngine;

/// <summary>
/// 点击信息
/// </summary>
public abstract class PointerInfo
{
    /// <summary>
    /// 当前位置
    /// </summary>
    public Vector2 currentPosition;

    /// <summary>
    /// 前一帧的位置
    /// </summary>
    public Vector2 previousPosition;

    /// <summary>
    /// 当前位置和前一帧位置差
    /// </summary>
    public Vector2 delta;

    /// <summary>
    /// 是否是从UI上开始
    /// </summary>
    public bool startedOverUI;
}