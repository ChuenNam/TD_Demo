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
    
    public void Write(string newContent) => _content.text = newContent;
    public void Write(string newTitle,string newContent)
    {
        _title.text = newTitle;
        _content.text = newContent;
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
        });
    }
}
