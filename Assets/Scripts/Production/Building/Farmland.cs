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
    
    [Header("Buff数据")] 
    public float daytimeBonus = 1;
    public float nighttimeBonus = 1;
    
    public float GetDayBonus() => daytimeBonus;
    public float GetNightBonus()  => nighttimeBonus;

    public int Level => level;
    public int MaxLevel
    {
        get => maxLevel;
        set => maxLevel = value;
    }

    public override void Initialize(string id, GridManager manager)
    {
        base.Initialize(id, manager);
        LevelUp();      // 初始化调用,附加 Lv.1 的 buff
    }

    public void LevelUp()
    {
        if (level >= maxLevel)
        {
            level = maxLevel;
            UIManager.instance.helpPanel.Show("已经到达等级上限");
            return;
        }
        // 检查材料
        var cost = eachLevelBuff[level].levelUpCost;
        if (cost.Any(group => group.count > ItemManager.instance.GetItemCount(group.item)))
        {
            UIManager.instance.helpPanel.Show("材料不足");
            return;
        }
        
        // 扣除材料
        foreach (var group in cost) 
            group.item.count -= group.count;
        
        // 添加buff
        var levelUpBuff = eachLevelBuff[level];
        var buffsToAdd = new List<Buff>();
        foreach (var config in levelUpBuff.buffConfigs)
        {
            switch (config.buffType)
            {
                case BuffType.Productivity:
                    var buff = BuffManager.CreatProductivityBuff(this, config);
                    buff.buffName += 'L';
                    buffsToAdd.Add(buff);
                    break;
                case BuffType.ExtraOutput:
                    buff = BuffManager.CreatExtraOutputBuff(this, config);
                    buff.buffName += 'L';
                    buffsToAdd.Add(buff);
                    break;
            }
        }
        // 根据添加方式添加
        if (level == 0)
            buffList.AddRange(buffsToAdd);
        else
        {
            switch (setLevelBuffMode)
            {
                case SetLevelBuffMode.Add:
                    buffList.AddRange(buffsToAdd);
                    break;
                case SetLevelBuffMode.Replace:
                    foreach (var buff in buffList.ToList())
                    {
                        Debug.Log(buff.buffName[^1]+"******************");
                        if (buff.buffName[^1] == 'L')
                        {
                            buffList.Remove(buff);
                        }
                    }
                    buffList.AddRange(buffsToAdd);
                    break;
            }
        }
        
        Debug.Log("升级");
        level++;        // 升级
    }

    public string GetCostInfo()
    {
        var cost = "";
        for (var i = 0; i < eachLevelBuff[level].levelUpCost.Count; i++)
        {
            var group = eachLevelBuff[level].levelUpCost[i];
            cost += $"{group.count}{group.item.itemName}";
            if (i == eachLevelBuff[level].levelUpCost.Count - 1) 
                return cost;
            cost += "+";
        }
        return cost;
    }


    public enum SetLevelBuffMode
    {
        Add,
        Replace,
    }
}
