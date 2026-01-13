using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "New TaskNode", menuName = "Task/New TaskNode")]
public class TaskNode : ScriptableObject
{
    public List<TaskData> taskDataList;

    public bool CheckNodeDone()
    {
        var isDone = true;
        foreach (var taskData in taskDataList)
        {
            isDone = isDone && taskData.isDone;
        }
        return isDone;
    }
}