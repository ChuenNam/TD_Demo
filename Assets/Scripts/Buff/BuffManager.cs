using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BuffManager : MonoBehaviour
{
    #region 单例 instance
        public static BuffManager instance;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }
    #endregion
    private static TimeLogic timeLogic;
    
    public List<Building> buildings = new();
    public List<RandomEvent> seasonEvents = new();
    //存储选择的事件与对应的buff列表
    public List<RandomEvent> allEventBuffs = new();
    [Header("所有随机事件")]
    public List<RandomEvent> positiveRandomEvents = new();
    public List<RandomEvent> negativeRandomEvents = new();

    public void AddEventBuff(RandomEvent e)
    {
        allEventBuffs.Add(e);
    }

    public void AddBuffToBuilding(Building building)
    {
        foreach (var e in allEventBuffs)
        {
            for (var i = 0; i < e.productionBuffConfigs.Count; i++)
            {
                e.Init(building);
                if (i >= 0 && i < e.buffs.Count) 
                    building.AddBuff(e.buffs[i]);
            }
        }
    }

    private void Start()
    {
        timeLogic = TimeLogic.instance;
        buildings = timeLogic.buildings;
        
        ChoseSeasonEvent();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ChoseRandomEvent(true,3);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ChoseRandomEvent(false,3);
        }
    }
    
    private void OnDestroy()
    {
        foreach (var e in positiveRandomEvents)
            e.buffs.Clear();
        foreach (var e in negativeRandomEvents)
            e.buffs.Clear();
        foreach (var e in seasonEvents)
            e.buffs.Clear();
        
        allEventBuffs.Clear();
    }

    public void ChoseSeasonEvent()
    {
        UIManager.instance.eventChosePanel.ShowEventChose(seasonEvents);
    }
    public void ChoseRandomEvent(bool isPositive ,int count)
    {
        var source = isPositive ? positiveRandomEvents : negativeRandomEvents;
        List<RandomEvent> eventsList = new();
        List<int> usedIndices = new();
        for (var i = 0; i < count; i++)
        {
            int idx;
            do { idx = Random.Range(0, source.Count); } 
            while (usedIndices.Contains(idx));
            usedIndices.Add(idx);
            eventsList.Add(source[idx]);
        }
        UIManager.instance.eventChosePanel.ShowEventChose(eventsList);
    }
    

    public List<Buff> DayBuff(Building building)
    {
        List<Buff> buffs = new List<Buff>();
        switch (building)
        {
            case IDayNightBonus b:
                if (Mathf.Approximately(b.GetDayBonus(), 1)) 
                    break;
                var buff = CreatProductivityBuff(building, b.GetDayBonus(), timeLogic.GetDaySeconds());
                var bonus = b.GetDayBonus();
                buff.buffName = bonus switch
                {
                    < 1 and > 0 => $"产速-{(1-bonus)*100}% (昼)",
                    > 1 => $"产速+{(bonus-1)*100}% (昼)",
                    <= 0 and >= 0 => "停产 (昼)",
                    _ => buff.buffName
                };
                buffs.Add(buff);
                break;
        }
        return buffs;
    }
    public List<Buff> NightBuff(Building building)
    {
        List<Buff> buffs = new List<Buff>();
        switch (building)
        {
            case IDayNightBonus b:
                if (Mathf.Approximately(b.GetNightBonus(), 1)) 
                    break;
                var buff = CreatProductivityBuff(building, b.GetNightBonus(), timeLogic.GetNightSeconds());
                var bonus = b.GetNightBonus();
                buff.buffName = bonus switch
                {
                    < 1 and > 0 => $"产速-{(1-bonus)*100}% (夜)",
                    > 1 => $"产速+{(bonus-1)*100}% (夜)",
                    <= 0 and >= 0 => "停产 (夜)",
                    _ => ""
                };
                buffs.Add(buff);
                break;
        }
        return buffs;
    }

    // 建筑生产效率加倍 Buff
    public static Buff CreatProductivityBuff(Building building, float multiple, float duration, string buffName = "", BaseItem targetItems = null)
    {
        return targetItems is null
            ? new Buff(
                duration,
                AddOrDelBuff_timeMultiplier(true, multiple-1, building),
                AddOrDelBuff_timeMultiplier(false, multiple-1, building))
            : new Buff(
                duration,
                AddOrDelBuff_timeMultiplier(true, multiple-1, building, targetItems),
                AddOrDelBuff_timeMultiplier(false, multiple-1, building, targetItems));
    }
    public static Buff CreatProductivityBuff(Building building, ProductionBuffConfig config)
    {
        var buff = config.targetItems is null
            ? new Buff(
                config.CalculateDuration(),
                AddOrDelBuff_timeMultiplier(true, config.value-1, building),
                AddOrDelBuff_timeMultiplier(false, config.value-1, building))
            : new Buff(
                config.CalculateDuration(),
                AddOrDelBuff_timeMultiplier(true, config.value-1, building, config.targetItems),
                AddOrDelBuff_timeMultiplier(false, config.value-1, building, config.targetItems));
        
        buff.buffName = config.buffName;
        buff.buffDescription = config.buffDescription;
        return buff;
    }
    
    // 创建产量加倍 Buff
    public static Buff CreatExtraOutputBuff(Building building, int extra, float duration, BaseItem targetItem = null)
    {
        return targetItem is null
            ? new Buff(
                duration, 
                AddOrDelBuff_Count(true, extra, building), 
                AddOrDelBuff_Count(false, extra, building))
            : new Buff(
                duration, 
                AddOrDelBuff_Count(true, extra, building,targetItem), 
                AddOrDelBuff_Count(false, extra, building,targetItem));
    }
    public static Buff CreatExtraOutputBuff(Building building, ProductionBuffConfig config)
    {
        var buff = config.targetItems is null
            ? new Buff(
                config.CalculateDuration(),
                AddOrDelBuff_Count(true, (int)config.value, building),
                AddOrDelBuff_Count(false, (int)config.value, building))
            : new Buff(
                config.CalculateDuration(),
                AddOrDelBuff_Count(true, (int)config.value, building, config.targetItems),
                AddOrDelBuff_Count(false, (int)config.value, building, config.targetItems));
        
        buff.buffName = config.buffName;
        buff.buffDescription = config.buffDescription;
        return buff;
    }

    // 创建解锁配方 Buff
    public static Buff CreateUnLockBpBuff(Building building, ProductionBuffConfig config)
    {
        var buff = new Buff(
            config.CalculateDuration(),
            AddOrDelBuff_BpLock(true, (int)config.value, building),
            AddOrDelBuff_BpLock(false, (int)config.value, building));
        
        buff.buffName = config.buffName;
        buff.buffDescription = config.buffDescription;
        return buff;
    }
    
    private static Action AddOrDelBuff_Count(bool isAdd, int num, Building building, BaseItem targetItems = null)
    {
        Action action = () =>
        {
            var rstNum = isAdd ? num : -num;
            foreach (var bp in building.blueprints)
            {
                foreach (var product in bp.productGroup)
                {
                    if (targetItems is not null && targetItems != product.item)
                        continue;
                    
                    product.count += rstNum;    // TODO:实现赋值对象传递？
                    building.objectData.UpdateDataUI();
                }
            }
        };
        return action;
    }
    private static Action AddOrDelBuff_timeMultiplier(bool isAdd, float num, Building building, BaseItem targetItems = null)
    {
        Action action = () =>
        {
            var rstNum = isAdd ? num : -num;
            foreach (var bp in building.blueprints)
            {
                foreach (var product in bp.productGroup)
                {
                    if (targetItems is not null && targetItems != product.item)
                        continue;
                    
                    bp.timeMultiplier += rstNum;    // TODO:实现赋值对象传递？
                    building.objectData.UpdateDataUI();
                }
            }
        };
        return action;
    }
    private static Action AddOrDelBuff_BpLock(bool isAdd, int index, Building building)
    {
        Action action = () =>
        {
            var rst = !isAdd;       // 添加buff为解锁, 删除buff为禁用
            var bp =  building.blueprints[index];

            bp.isLocked = rst;    // TODO:实现赋值对象传递？
            building.objectData.UpdateDataUI();
        };
        return action;
    }

}
