using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public HelpPanel helpPanel;
    public ObjectInfoPanel objectInfoPanel;
    public EventChosePanel eventChosePanel;
    public BpListPanel bpListPanel;
    public RestaurantPanel restaurantPanel;
    public TradePanel tradePanel;
    public TaskPanel taskPanel;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    
    
    private void OnEnable()
    {
        // 注册场景加载/卸载回调
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 取消回调，避免内存泄漏
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    // 场景卸载时：清空旧UI引用（核心步骤）
    private void OnSceneUnloaded(Scene scene)
    {
        // 主动清空所有UI引用，避免残留旧场景的无效对象
        helpPanel = null;
        objectInfoPanel = null;
        eventChosePanel = null;
        bpListPanel = null;
        restaurantPanel = null;
        tradePanel = null;
        taskPanel =  null;
    }
    
    // 场景加载完成后：重新获取新场景的UI引用（核心步骤）
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        // 仅处理当前激活场景
        if (scene != SceneManager.GetActiveScene()) return;

        // 重新查找并缓存新场景的UI组件（替换旧引用）
        FindAndCacheNewUI();
    }
    
    // 查找新场景UI，缓存有效引用
    private void FindAndCacheNewUI()
    {
        // 通过对象名称查找（确保新场景UI名称一致）
        helpPanel = Resources.FindObjectsOfTypeAll<HelpPanel>()[0];
        objectInfoPanel = Resources.FindObjectsOfTypeAll<ObjectInfoPanel>()[0];
        eventChosePanel = Resources.FindObjectsOfTypeAll<EventChosePanel>()[0];
        bpListPanel = Resources.FindObjectsOfTypeAll<BpListPanel>()[0];
        restaurantPanel = Resources.FindObjectsOfTypeAll<RestaurantPanel>()[0];
        tradePanel = Resources.FindObjectsOfTypeAll<TradePanel>()[0];
        taskPanel = Resources.FindObjectsOfTypeAll<TaskPanel>()[0];
        
    }

}
