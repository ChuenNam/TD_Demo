using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += ResetItemCountOnLoad;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= ResetItemCountOnLoad;
    }


    private void ResetItemCountOnLoad(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name != SceneManager.GetActiveScene().name) 
            return;
        Debug.Log("场景" + scene.name + "完全加载完成，开始初始化数据");
        
        // 初始化时间
        var timeMng = TimeLogic.instance;
        timeMng.globalTime = 0; 
        timeMng.dayTime = 0;
        timeMng.day = 0;
        timeMng.isDay = false;
        timeMng.dayProgress = 0;

        // 初始化资源
        var itemMng = ItemManager.instance;
        foreach (var item in itemMng.possesItem)
        {
            item.count = 0;
        }
        itemMng.GetItemByName("金币").count = 10;
        
        // 初始化buff
        var buffMng = BuffManager.instance;
        foreach (var e in buffMng.positiveRandomEvents)
            e.buffs.Clear();
        foreach (var e in buffMng.negativeRandomEvents)
            e.buffs.Clear();
        buffMng.allEventBuffs.Clear();
        
        buffMng.ChoseSeasonEvent();
    }
}