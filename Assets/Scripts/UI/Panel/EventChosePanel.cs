using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventChosePanel : BasePanel
{
    [Header("数据信息")]
    public List<RandomEvent> eventList;
    
    [Header("UI控件")]
    public GameObject eventPanelPrefab;
    public List<GameObject> eventPanelList;
    
    protected override void Init()
    {
        base.Init();
        //BuffManager.instance.ChoseSeasonEvent();
    }

    public void ShowEventChose(List<RandomEvent> eventList)
    {
        ShowPanel();
        var preTimeSpeed = TimeLogic.instance.timeSpeed;
        TimeLogic.instance.timeSpeed = 0;
        // 创建面板
        foreach (var e in eventList)
        {
            var panel = Instantiate(eventPanelPrefab, transform);
            eventPanelList.Add(panel);
            panel.transform.GetChild(0).GetComponent<Text>().text = e.eventName;
            panel.transform.GetChild(1).GetComponent<Text>().text = e.eventDescription;
            panel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                BuffManager.instance.AddEventBuff(e);  // 添加选择的buff
                
                //清除数据
                foreach (var p in eventPanelList)
                    Destroy(p);
                eventPanelList = null;
                eventList = null;
                
                ClosePanel();       //关闭面板
                TimeLogic.instance.timeSpeed = preTimeSpeed >= 1 ? preTimeSpeed : 0;
            });
        }
    }
    public void ShowEventChose(bool isEver, List<RandomEvent> eventList)
    {
        ShowPanel();
        var preTimeSpeed = TimeLogic.instance.timeSpeed;
        TimeLogic.instance.timeSpeed = 0;
        // 创建面板
        foreach (var e in eventList)
        {
            var panel = Instantiate(eventPanelPrefab, transform);
            eventPanelList.Add(panel);
            panel.transform.GetChild(0).GetComponent<Text>().text = e.eventName;
            panel.transform.GetChild(1).GetComponent<Text>().text = e.eventDescription;
            panel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                BuffManager.instance.allEventBuffs.Add(e);  // 添加选择的buff
                
                foreach (var p in eventPanelList)
                    Destroy(p);
                eventPanelList = null;
                eventList = null;
                
                ClosePanel();       //关闭面板
                TimeLogic.instance.timeSpeed = preTimeSpeed >= 1 ? preTimeSpeed : 0;
            });
        }
    }
}
