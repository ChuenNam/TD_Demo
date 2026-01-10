using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Event Config", menuName = "Buff | Event/New Event Config")]
public class RandomEvent : ScriptableObject
{
    [Header("基础信息")]
    public string eventName;
    [TextArea]public string eventDescription;
    [Header("Event 数据")]
    public List<ProductionBuffConfig> productionBuffConfigs;
    public List<Buff> buffs;

    public List<Buff> Init(Building building)
    {
        buffs.Clear();
        foreach (var productionBuffConfig in productionBuffConfigs)
        {
            if (productionBuffConfig.building.GetType() == building.GetType())
            {
                buffs.Add(BuffManager.CreatProductivityBuff(building ,productionBuffConfig));
            }
        }
        return buffs;
    }
}