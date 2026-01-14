using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasePanel : MonoBehaviour
{
    public bool isActive = false;
    public Button closeButton;

    private void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        if(closeButton)
            closeButton.onClick.AddListener(ClosePanel);
    }

    public void ClosePanel()
    {
        if (isActive)
        {
            isActive = false;
            gameObject.SetActive(false);
        }
    }
    public void ShowPanel()
    {
        if (this != null && !isActive)
        {
            isActive = true;
            gameObject.SetActive(true);
        }
    }
    public void ClosePanel(BasePanel panel)
    {
        if (panel.isActive)
        {
            panel.ClosePanel();
        }
    }
    public void ShowPanel(BasePanel panel)
    {
        if (!panel.isActive)
        {
            panel.ShowPanel();
        }
    }
}
