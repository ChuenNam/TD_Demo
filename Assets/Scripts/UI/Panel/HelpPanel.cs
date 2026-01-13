using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanel : BasePanel
{
    private Action _tempAction;
    private Action _confirmAction;
    
    [Header("UI控件")] 
    [SerializeField]private Text _title;
    [SerializeField]private Text _content;
    [SerializeField]private Button _confirmBtn;

    public void Show(string newContent, bool showConfirm = false, string confirmButtonText = "")
    {
        TimeLogic.instance.timeSpeed = 0;
        _content.text = newContent;
        _confirmBtn.gameObject.SetActive(false);
        ShowPanel();

        if (showConfirm)
        {
            var txt = confirmButtonText == "" ? "确认" : confirmButtonText;
            _confirmBtn.GetComponentInChildren<Text>().text = txt;
            _confirmBtn.gameObject.SetActive(true);
        }
    }
    public void Show(string newTitle, string newContent, bool showConfirm = false)
    {
        TimeLogic.instance.timeSpeed = 0;
        _title.text = newTitle;
        _content.text = newContent;
        ShowPanel();
        if (showConfirm) _confirmBtn.gameObject.SetActive(true);
    }

    public void AddCloseAction(Action action) => _tempAction = action;
    public void AddConfirmAction(Action action) => _confirmAction = action;

    private void DoActionOnClose()
    {
        _tempAction?.Invoke();
        _tempAction = null;
    }
    private void DoActionOnConfirm()
    {
        _confirmAction?.Invoke();
        _confirmAction = null;
    }
    protected override void Init()
    {
        base.Init();
        _confirmBtn.onClick.AddListener(DoActionOnConfirm);
        closeButton.onClick.AddListener(DoActionOnClose);
        closeButton.onClick.AddListener(() =>
        {
            _title.text = "提示";
            _content.text = "";
        });
    }
}
