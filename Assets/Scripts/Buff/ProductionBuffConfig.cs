using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Buff Config", menuName = "Buff | Event/New Buff Config")]
public class ProductionBuffConfig : ScriptableObject
{
    [Header("基础信息")]
    public string buffName;
    [TextArea]public string buffDescription;
    [Header("Buff 数据")]
    public BuffType  buffType;
    public float multiple = 1;
    public DurationType duration;
    public Building building;
    public BaseItem targetItems;

    public float CalculateDuration()
    {
        switch (duration)
        {
            case DurationType.OneDay:
                return TimeLogic.instance.timeConfig.secondsPerDay;
            case DurationType.DayTime:
                return TimeLogic.instance.timeConfig.secondsPerDay * TimeLogic.instance.timeConfig.nightChangePoint;
            case DurationType.Night:
                return TimeLogic.instance.timeConfig.secondsPerDay * (1-TimeLogic.instance.timeConfig.nightChangePoint);
            case DurationType.Ever:
                return -1;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum BuffType
{
    Productivity,
    ExtraOutput,
}

public enum DurationType
{
    OneDay,
    DayTime,
    Night,
    Ever
}
