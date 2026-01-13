using System;
using System.Collections;
using System.Collections.Generic;
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
    
    public GridManager gridManager;
    private Material gridMaterial;

    private void Start()
    {
        gridMaterial = gridManager.GetComponent<MeshRenderer>().material;
        
        buildButton.onClick.AddListener(() => OpenChildPanel(childPanel));
        buildButton.onClick.AddListener(() =>
        {
            IsBuildMode = !IsBuildMode;
            TimeLogic.instance.timeSpeed = IsBuildMode ? 0 : 1;
            
            rotateBtn.gameObject.SetActive(IsBuildMode);
            moveBtn.gameObject.SetActive(IsBuildMode);
        });
        
        rotateBtn.onClick.AddListener(DoRotate);
        moveBtn.onClick.AddListener(() =>
        {
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
    
    public void OnBuildMode()
    {
        gridMaterial.SetColor("_LineColor", Color.white);
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