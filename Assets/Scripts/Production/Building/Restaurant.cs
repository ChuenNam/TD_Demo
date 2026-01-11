using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Restaurant : Building, ITrading
{
    [Header("专属信息")]
    public List<Blueprint> todayMenu = new();
    public Queue<Blueprint> todayDishQueue = new();

    public override void Initialize(string id, GridManager manager)
    {
        base.Initialize(id, manager);
        UIManager.instance.restaurantPanel.restaurant = this;

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

    public string RewriteBlueprintText(Blueprint blueprint)
    {
        return blueprint.blueprintName;
    }

    public void SetDishToQueue()
    {
        foreach (var dish in todayMenu)
        {
            todayDishQueue.Enqueue(dish);
        }
        CurrentBlueprint = todayDishQueue.Dequeue();
    }
}
