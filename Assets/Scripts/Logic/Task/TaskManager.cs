using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    #region 单例 instance
    public static TaskManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    #endregion 
    
    public TaskNode finalTask;
    public List<TaskNode> taskList;
    public int currentNodeIndex;
    
    private void Update()
    {
        // 检查最终目标
        foreach (var finalTaskData in finalTask.taskDataList)
        {
            finalTaskData.CheckTaskDone();
        }
        if (finalTask.taskDataList[0].isDone)
        {
            UIManager.instance.helpPanel.Show("恭喜","你完成了最终目标！",true);
            UIManager.instance.helpPanel.AddCloseAction(() =>
            {
                //TODO: 完成游戏
            });
            UIManager.instance.helpPanel.AddConfirmAction(() =>
            {
                //TODO: 完成游戏
            });
        }
        
        if (currentNodeIndex > taskList.Count-1)    // 已完成所有阶段任务
            return;
        
        if (taskList[currentNodeIndex].CheckNodeDone())
        {
            currentNodeIndex++;
            if (currentNodeIndex > taskList.Count - 1)
                return;
            UIManager.instance.taskPanel.CreateAndDisplayTask(taskList[currentNodeIndex].taskDataList);
        }
        // 检查阶段目标
        foreach (var taskData in taskList[currentNodeIndex].taskDataList)
        {
            taskData.CheckTaskDone();
        }
    }
    
    private void OnDisable()
    {
        foreach (var taskData in taskList.SelectMany(taskNode => taskNode.taskDataList))
        {
            taskData.isDone = false;
        }
    }
}


[Serializable]
public class TaskData
{
    public string taskInfo;
    public bool isDone;
    [Space(10)]
    public BaseItem targetItem;
    public int itemCount;
    [Space(10)]
    public Building targetBuilding;
    public int buildingCount;
    [Space(10)]
    public Building levelBuilding;
    public int targetLevel;
    
    public void CheckTaskDone()
    {
        if (isDone) return;     // 已完成
        // 检查物品条件
        var itemCheck = true;
        if (targetItem is not null)
        {
            var count = ItemManager.instance.GetItemCount(targetItem);
            itemCheck = count >= itemCount;
        }
        // 检查建筑条件
        var buildingCheck = true;
        var allBuilding = TimeLogic.instance.buildings;
        if (targetBuilding is not null)
        {
            var count = allBuilding.Count(building => building.GetType() == targetBuilding.GetType());
            buildingCheck = count >= buildingCount;
        }
        // 检查建筑等级条件
        var levelCheck = true;
        if (levelBuilding is not null)
        {
            foreach (var building in allBuilding)
            {
                if (building is ILevelUp levelUp && building.GetType() == levelBuilding.GetType())
                {
                    levelCheck = levelUp.Level == targetLevel;
                }
            }
        }

        // 合并结果
        isDone = itemCheck && buildingCheck && levelCheck;
        // 完成清除UI显示
        if (isDone)
        {
            var panel = UIManager.instance.taskPanel;
            panel.DeleteDisplayedTask(taskInfo);
        }
    }
}
