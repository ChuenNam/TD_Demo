using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Farmland : Building, IDayNightBonus, ILevelUp
{
    [Header("等级数据")] 
    public int level;
    public int maxLevel = 2;
    public SetLevelBuffMode setLevelBuffMode;
    public List<Level> eachLevelBuff = new(); 
    public int Level
    {
        get => level;
        set => level = value;
    }
    public int MaxLevel
    {
        get => maxLevel;
        set => maxLevel = value;
    }
    public List<Level> GetEachLevelBuff() => eachLevelBuff;
    public SetLevelBuffMode GetLevelBuffMode() => setLevelBuffMode;

    [Header("Buff数据")] 
    public float daytimeBonus = 1;
    public float nighttimeBonus = 1;
    public float GetDayBonus() => daytimeBonus;
    public float GetNightBonus()  => nighttimeBonus;

    public override void Initialize(string id, GridManager manager)
    {
        base.Initialize(id, manager);
        ((ILevelUp)this).LevelUp(this);      // 初始化调用,附加 Lv.1 的 buff
    }
}
