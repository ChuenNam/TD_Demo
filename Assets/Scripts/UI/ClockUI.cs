using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClockUI : MonoBehaviour
{
    public Image dayCircle;
    public RectTransform hand;
    public Text dayInfo;
    
    public Button stopButton;
    public Button startButton;
    public Button fastButton;
    public Color selectedColor;
    public Color unselectedColor;
    
    private TimeLogic _timeLogic;
    
    void Start()
    {
        _timeLogic = TimeLogic.instance;
        stopButton.onClick.AddListener(() =>
        {
            _timeLogic.timeSpeed = 0;
            stopButton.image.color = selectedColor;
            startButton.image.color = unselectedColor;
            fastButton.image.color = unselectedColor;
        });
        startButton.onClick.AddListener(() =>
        {
            _timeLogic.timeSpeed = 1;
            startButton.image.color = selectedColor;
            stopButton.image.color = unselectedColor;
            fastButton.image.color = unselectedColor;
        });
        fastButton.onClick.AddListener(() =>
        {
            _timeLogic.timeSpeed = 2;
            fastButton.image.color = selectedColor;
            stopButton.image.color = unselectedColor;
            startButton.image.color = unselectedColor;
        });
    }
    
    void Update()
    {
        dayCircle.fillAmount = _timeLogic.timeConfig.nightChangePoint;
        hand.rotation = Quaternion.Euler(new Vector3(0, 0, _timeLogic.dayProgress * -360) + new Vector3(0, 0, 90));
        var dayOrNight = _timeLogic.isDay ? "白天" : "夜晚";
        dayInfo.text = $"第{_timeLogic.day}天  {dayOrNight}";
    }
}
