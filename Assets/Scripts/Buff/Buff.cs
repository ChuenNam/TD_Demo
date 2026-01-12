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
    public float duration;
    public float remainDuration;

    public Buff(float duration, Action onAddBuff, Action onDelBuff, string buffName = "", string buffDescription = "")
    {
        this.buffName = buffName;
        this.buffDescription = buffDescription;
        
        this.duration = duration;
        remainDuration = duration;
        addBuffAction = onAddBuff;
        delBuffAction = onDelBuff;
    }
    
}

[Serializable]
public class Level
{
    public List<ProductionBuffConfig> buffConfigs = new();
    public List<ItemGroup> levelUpCost = new();
}