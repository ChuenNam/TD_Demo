using System;
using UnityEngine;

public enum EffectType
{
    None,
    Number,
    Bool
}


[Serializable]
public class Buff
{
    public string name;
    public string description;
    
    public object target;
    public EffectType effectType;
    
    private Buff(object target, EffectType effectType = EffectType.None)
    {
        this.target = target;
        this.effectType = effectType;
    }
    
}