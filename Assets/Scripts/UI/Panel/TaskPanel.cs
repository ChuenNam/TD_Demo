using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskPanel : BasePanel
{
    [Header("UI控件")]
    public RectTransform taskListRect;
    public GameObject taskPrefab;
    
    [Header("数据信息")]
    public List<TaskData> taskToShow;

    protected override void Init()
    {
        base.Init();
        CreateAndDisplayTask(TaskManager.instance.taskList[TaskManager.instance.currentNodeIndex].taskDataList);
    }

    public void CreateAndDisplayTask(List<TaskData> taskDataList)
    {
        // 清除所有 UI信息
        for (var i = 0; i < taskListRect.childCount; i++)
            Destroy(taskListRect.GetChild(i).gameObject);
        
        taskToShow = taskDataList;
        foreach (var t in taskToShow)
        {
            var obj = Instantiate(taskPrefab, taskListRect);
            obj.GetComponent<Text>().text = t.taskInfo;
        }
    }
    public void DeleteDisplayedTask(string taskInfo)
    {
        if (taskListRect == null) return;
        for (var i = 0; i < taskListRect.childCount; i++)
        {
            var txt = taskListRect.GetChild(i).gameObject.GetComponent<Text>();
            if (txt.text == taskInfo)
                Destroy(taskListRect.GetChild(i).gameObject);
        }
    }
}
