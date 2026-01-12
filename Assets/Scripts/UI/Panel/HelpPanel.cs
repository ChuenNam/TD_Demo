using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanel : BasePanel
{
    private Action _tempAction;
    
    [Header("UI控件")] 
    [SerializeField]private Text _title;
    [SerializeField]private Text _content;
    
    public void Show(string newContent)
    {
        TimeLogic.instance.timeSpeed = 0;
        _content.text = newContent;
        ShowPanel();
    }
    public void Show(string newTitle,string newContent)
    {
        _title.text = newTitle;
        _content.text = newContent;
        ShowPanel();
    }

    public void AddCloseAction(Action action) => _tempAction = action;

    private void DoActionOnClose()
    {
        _tempAction?.Invoke();
        _tempAction = null;
    }
    protected override void Init()
    {
        base.Init();
        closeButton.onClick.AddListener(DoActionOnClose);
        closeButton.onClick.AddListener(() =>
        {
            _title.text = "提示";
            _content.text = "";
            TimeLogic.instance.timeSpeed = 1;
        });
    }
}
