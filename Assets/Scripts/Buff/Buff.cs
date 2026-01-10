using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Buff
{
    public string buffName;
    public string buffDescription;
    public Action addBuffAction;
    public Action delBuffAction;
    public object[] parameters;
    public float duration;
    public float remainDuration;

    public Buff(float duration, Action onAddBuff, Action onDelBuff, params object[] param)
    {
        this.duration = duration;
        remainDuration = duration;
        addBuffAction = onAddBuff;
        delBuffAction = onDelBuff;
        parameters = param;
    }
    
}