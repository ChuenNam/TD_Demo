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
    }

    public void ShowEventChose(List<RandomEvent> eventList)
    {
        ShowPanel();
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
                //BuffManager.instance.allEventBuffs.Add(e,e.Init());  // 添加选择的buff
                
                //清除数据
                foreach (var p in eventPanelList)
                    Destroy(p);
                eventPanelList.Clear();
                eventList.Clear();
                
                ClosePanel();       //关闭面板
                TimeLogic.instance.timeSpeed = 0;
            });
        }
    }
}
