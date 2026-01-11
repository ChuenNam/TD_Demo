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
    public List<RandomEvent> randomEvents = new();
    //存储选择的事件与对应的buff列表
    public List<RandomEvent> allEventBuffs = new();

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
            ChoseRandomEvent(3);
        }
    }
    
    private void OnDestroy()
    {
        foreach (var e in randomEvents)
            e.buffs.Clear();
        foreach (var e in seasonEvents)
            e.buffs.Clear();
        allEventBuffs.Clear();
    }

    public void ChoseSeasonEvent()
    {
        UIManager.instance.eventChosePanel.ShowEventChose(seasonEvents);
    }
    public void ChoseRandomEvent(int count)
    {
        List<RandomEvent> eventsList = new();
        for (int i = 0; i < count; i++)
        {
            var idx = Random.Range(0, randomEvents.Count);
            eventsList.Add(randomEvents[idx]);
        }
        UIManager.instance.eventChosePanel.ShowEventChose(eventsList);
    }
    
    

    public List<Buff> DayBuff(Building building)
    {
        List<Buff> buffs = new List<Buff>();
        switch (building)
        {
            case IDayNightBonus b:
                buffs.Add(CreatProductivityBuff(building, b.GetDayBonus(), timeLogic.GetDaySeconds()));
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
                buffs.Add(CreatProductivityBuff(building, b.GetNightBonus(), timeLogic.GetNightSeconds()));
                break;
        }
        return buffs;
    }

    // 建筑生产效率加倍 Buff
    public static Buff CreatProductivityBuff(Building building, float multiple, float duration)
    {

        //var bp = building.CurrentBlueprint;
        Action onAddBuff = () =>
        {
            foreach (var bp in building.blueprints)
            {
                bp.timeMultiplier += multiple-1;
                building.objectData.UpdateDataUI();
            }
        };
        Action onDelBuff = () =>
        {
            foreach (var bp in building.blueprints)
            {
                bp.timeMultiplier -= multiple-1;
                building.objectData.UpdateDataUI();
            }
        };

        var buff = new Buff(duration, onAddBuff, onDelBuff);
        return buff;
    }
    public static Buff CreatProductivityBuff(Building building, ProductionBuffConfig config)
    {
        //var bp = building.CurrentBlueprint;
        Action onAddBuff = () =>
        {
            foreach (var bp in building.blueprints)
            {
                bp.timeMultiplier += config.multiple-1;
                building.objectData.UpdateDataUI();
            }
        };
        Action onDelBuff = () =>
        {
            foreach (var bp in building.blueprints)
            {
                bp.timeMultiplier -= config.multiple-1;
                building.objectData.UpdateDataUI();
            }
        };

        var buff = new Buff(config.CalculateDuration(), onAddBuff, onDelBuff);
        buff.buffName = config.buffName;
        buff.buffDescription = config.buffDescription;
        return buff;
    }
    
}
