using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class BuildMode : MonoBehaviour
{
    [SerializeField]private bool isBuildMode;
    public bool IsBuildMode
    {
        get => isBuildMode;
        set
        {
            if (isBuildMode != value)
            {
                isBuildMode = value;
                
                if (isBuildMode)
                    OnBuildMode();
                else
                    OnExitBuildMode();
            }
        }
    }
    
    public Button buildButton;
    public BasePanel childPanel;
    public bool moveMode;
    public Button moveBtn;
    public Button rotateBtn;
    public Button removeBtn;
    
    public GridManager gridManager;
    public Material gridMaterial;
    public CinemachineVirtualCamera sceneCam;

    private void Start()
    {
        //gridMaterial = gridManager.GetComponent<MeshRenderer>().material;
        //sceneCam.enabled = true;      //初始为场景相机
        
        buildButton.onClick.AddListener(() =>
        {
            if(gridManager.isPlacing)   
                return;
            
            OpenChildPanel(childPanel);
            IsBuildMode = !IsBuildMode;
            TimeLogic.instance.timeSpeed = IsBuildMode ? 0 : 1;
            
            rotateBtn.gameObject.SetActive(IsBuildMode);
            removeBtn.gameObject.SetActive(IsBuildMode);
            moveBtn.gameObject.SetActive(IsBuildMode);
            
            sceneCam.enabled = !isBuildMode;
            if (IsBuildMode) UIManager.instance.objectInfoPanel.ClosePanel();
        });
        
        rotateBtn.onClick.AddListener(()=>
        {
            if(gridManager.isPlacing)
                return;
            DoRotate();
        });
        removeBtn.onClick.AddListener(()=>
        {
            if(gridManager.isPlacing)
                return;
            DoRemove();
        });
        moveBtn.onClick.AddListener(() =>
        {
            if (gridManager.isPlacing)  
                return;
            moveMode = !moveMode;
            moveBtn.image.color = moveMode ? Color.yellow : Color.black;
        });
    }

    private void OpenChildPanel(BasePanel panel)
    {
        if (panel.isActive)
            panel.ClosePanel();
        else
            panel.ShowPanel();
    }

    private void DoRotate()
    {
        if (!isBuildMode) return;

        var obj = gridManager.GetSelectObject();
        if (obj == null)
        {
            Debug.Log("未选中物体");
            return;
        }
        gridManager.RotateObject(obj.objectID);
    }
    private void DoRemove()
    {
        if (!isBuildMode) return;

        var obj = gridManager.GetSelectObject();
        if (obj == null)
        {
            Debug.Log("未选中物体");
            return;
        }

        UIManager.instance.helpPanel.Show($"是否要删除{obj.name}?\n(点击X取消删除)", true);
        UIManager.instance.helpPanel.AddConfirmAction(() =>
        {
            gridManager.RemoveObject(obj.objectID);
            UIManager.instance.objectInfoPanel.ClosePanel();
            UIManager.instance.helpPanel.Show("已删除");
        });
    }
    
    public void OnBuildMode()
    {
        gridMaterial.SetColor("_LineColor", Color.black);
    }
    public void OnExitBuildMode()
    {
        moveMode = false;
        gridMaterial.SetColor("_LineColor", Color.clear);
    }

    private void Update()
    {
        #region 快捷键
        //移动(M)
        if (Input.GetKeyDown(KeyCode.M)) moveMode = !moveMode;
        //旋转(R)
        if (Input.GetKeyDown(KeyCode.R)) DoRotate();
        #endregion
    }
}