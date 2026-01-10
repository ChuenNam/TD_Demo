using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClockUI : MonoBehaviour
{
    public Image dayCircle;
    public RectTransform hand;
    public Text dayInfo;
    
    private TimeLogic _timeLogic;
    
    void Start()
    {
        _timeLogic = TimeLogic.instance;
    }
    
    void Update()
    {
        dayCircle.fillAmount = _timeLogic.timeConfig.nightChangePoint;
        hand.rotation = Quaternion.Euler(new Vector3(0, 0, _timeLogic.dayProgress * -360) + new Vector3(0, 0, 90));
        var dayOrNight = _timeLogic.isDay ? "白天" : "夜晚";
        dayInfo.text = $"第{_timeLogic.day}天  {dayOrNight}";
    }
}
