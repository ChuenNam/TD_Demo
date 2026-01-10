using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLogic : MonoBehaviour
{
    #region 单例 instance
        public static TimeLogic instance;
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
    
    [Header("时间数据")] 
    public TimeConfig timeConfig;
    [Range(0,5)]public int timeSpeed = 1;
    public float globalTime;
    public float dayTime;
    public int day;
    public bool isDay;
    [Range(0,1)]public float dayProgress;
    
    [Header("建筑信息")]
    public List<Building> buildings;
    
    [Header("引用")]
    public GridManager gridManager;
    private Dictionary<string, GridObjectData> placedObjects;

    private void Start()
    {
        // 获取场上建筑列表引用
        placedObjects = gridManager.GetPlacedObjects();
    }

    private void Update()
    {
        // 检查是否有变更
        if (placedObjects.Count > buildings.Count)
        {
            // 更新建筑列表
            foreach (var objPair in placedObjects)
            {
                var objData = objPair.Value;
                var build = objData.instance.GetComponent<Building>();
                if (buildings.Contains(build)) 
                    continue;
                buildings.Add(build);
            }
        }
        
        // 处理时间逻辑
        TimeCalculation(timeSpeed);
        // 处理建筑生产逻辑
        BuildingCalculation();
    }

    public void TimeCalculation(float timeSpeed = 1)
    {
        if (timeSpeed == 0)
        {
            return;
        }
        globalTime += Time.deltaTime * timeSpeed;
        dayTime += Time.deltaTime * timeSpeed;
        dayProgress = dayTime / timeConfig.secondsPerDay;
        
        // 更新天数 - 新一天开始
        if (globalTime / timeConfig.secondsPerDay > day)
        {
            day++;
            dayProgress = 0;
            dayTime = 0;
            isDay = true;
            foreach (var building in buildings)
            {
                building.AddBuff(BuffManager.instance.DayBuff(building));
                //building.objectData.UpdateDataUI();
            }
        }
        
        // 更新昼夜 - 夜晚开始
        if (dayProgress >= timeConfig.nightChangePoint && isDay)
        {
            isDay = false;
            foreach (var building in buildings)
            {
                building.AddBuff(BuffManager.instance.NightBuff(building));
                //building.objectData.UpdateDataUI();
            }
        }
    }

    public void BuildingCalculation()
    {
        if (timeSpeed == 0)
        {
            return;
        }

        // 生产运行逻辑
        foreach (var building in buildings)
        {
            if (building.CurrentBlueprint == null)
                continue;
            
            if (building.inProduction)
            {
                building.timeCounter += Time.deltaTime * timeSpeed;
                building.Product();
            }
        }
        
        // buff运行逻辑
        foreach (var building in buildings)
        {
            building.UpdateBuff();
        }
    }

    public float GetDaySeconds() => timeConfig.secondsPerDay * timeConfig.nightChangePoint - dayProgress * timeConfig.secondsPerDay;
    public float GetNightSeconds() => timeConfig.secondsPerDay - dayProgress * timeConfig.secondsPerDay;

}
