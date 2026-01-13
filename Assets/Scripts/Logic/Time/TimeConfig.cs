using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Time Config", menuName = "Time/New Time Config")]
public class TimeConfig : ScriptableObject
{
    public float secondsPerDay;
    [Range(0, 1)] public float nightChangePoint;
    
}
