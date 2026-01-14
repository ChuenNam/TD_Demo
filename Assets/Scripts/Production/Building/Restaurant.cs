using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Restaurant : Building, ITrading, ILevelUp
{
    [Header("等级数据")] 
    public int level;
    public int limitLevel = 3;
    public int maxLevel = 3;
    public SetLevelBuffMode setLevelBuffMode;
    public List<Level> eachLevelBuff = new(); 
    
    [Header("专属信息")]
    public List<Blueprint> todayMenu = new();
    public Queue<Blueprint> todayDishQueue = new();
    public Action onLevelUpCompleted;

    public override void Initialize(string id, GridManager manager)
    {
        base.Initialize(id, manager);
        UIManager.instance.restaurantPanel.restaurant = this;
        ((ILevelUp)this).LevelUp(this);

        // 通过生产完成的回调改变生产目标蓝图
        onComplete += () =>
        {
            if (todayDishQueue.Count == 0)
            {
                Debug.Log("生产完毕");
                CurrentBlueprint = null;
                return;
            }
            CurrentBlueprint = todayDishQueue.Dequeue();
        };
    }
    
    public void SetDishToQueue()
    {
        foreach (var dish in todayMenu)
        {
            todayDishQueue.Enqueue(dish);
        }
        CurrentBlueprint = todayDishQueue.Dequeue();
    }

    public void OnLevelUpCompleted()
    {
        var allBuildings = TimeLogic.instance.buildings;

        // 提升其他建筑等级上限
        foreach (var building in allBuildings)
        {
            if (building == this || building is not ILevelUp levelBuilding)
                continue;
            
            if (levelBuilding.LimitLevel != levelBuilding.MaxLevel) 
                levelBuilding.LimitLevel = Level;
        }
        onLevelUpCompleted?.Invoke();
    }

    public string RewriteBlueprintText(Blueprint blueprint)
    {
        return blueprint.blueprintName;
    }
    public int Level
    {
        get => level;
        set => level = value;
    }
    public int LimitLevel
    {
        get => limitLevel;
        set => limitLevel = value;
    }
    public int MaxLevel
    {
        get => maxLevel;
        set => maxLevel = value;
        
    }
    public List<Level> GetEachLevelBuff() => eachLevelBuff;
    public SetLevelBuffMode GetLevelBuffMode() => setLevelBuffMode;

}
