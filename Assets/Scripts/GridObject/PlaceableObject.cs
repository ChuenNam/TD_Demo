using System.Collections.Generic;
using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    public string objectID;
    public GridManager gridManager;
    public BuildMode buildMode;
    
    private Material originalMaterial;
    private Material highlightMaterial;
    
    public void Initialize(string id, GridManager manager, BuildMode mode)
    {
        objectID = id;
        gridManager = manager;
        buildMode = mode;
        
        // 保存原始材质
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
        }
        
        // 创建高亮材质
        highlightMaterial = new Material(Shader.Find("Standard"));
        highlightMaterial.color = new Color(1, 1, 0.5f, 1);
        highlightMaterial.EnableKeyword("_EMISSION");
        highlightMaterial.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.2f));
    }
    
    void OnMouseEnter()
    {
        if (gridManager.isPlacing)
            return;
        
        ApplyHighlight(true);
    }
    
    void OnMouseExit()
    {
        if (gridManager.isPlacing)
            return;
        
        ApplyHighlight(false);
    }
    
    void OnMouseDown()
    {
        if (gridManager.isPlacing)
            return;
        var ui = UIManager.instance;
        if (ui.restaurantPanel.isActive || ui.tradePanel.isActive || ui.eventChosePanel.isActive)
            return;
        
        gridManager.SelectObject(objectID);
    }
    
    void OnMouseDrag()
    {
        if (gridManager.isPlacing)
            return;
        if (!buildMode.IsBuildMode || !buildMode.moveMode)
            return;
        
        // 实现拖拽移动
        Ray ray = gridManager.mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridManager.gridLayerMask))
        {
            Vector2Int gridPos = gridManager.WorldToGridPosition(hit.point);
            gridManager.MoveObject(objectID, gridPos);
            /*if (Input.GetKeyDown(KeyCode.R))
                gridManager.RotateObject(objectID);*/
        }
    }
    
    void ApplyHighlight(bool highlight)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = highlight ? highlightMaterial : originalMaterial;
        }
    }
    
    void OnDestroy()
    {
        if (highlightMaterial != null)
        {
            Destroy(highlightMaterial);
        }
    }
}
