using System;
using UnityEngine;

[Serializable]
public class Buff
{
    public Action addBuffAction;
    public Action delBuffAction;
    public object[] parameters;
    public int duration;
    public float remainDuration;

    public Buff(int duration, Action onAddBuff, Action onDelBuff, params object[] param)
    {
        this.duration = duration;
        remainDuration = duration;
        addBuffAction = onAddBuff;
        delBuffAction = onDelBuff;
        parameters = param;
    }
    
}