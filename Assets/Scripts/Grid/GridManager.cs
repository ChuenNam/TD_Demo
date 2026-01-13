using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("网格设置")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;
    public Vector2 gridOffset = Vector2.zero;
    
    // 网格数据
    private GridCell[,] gridCells;
    private Dictionary<string, GridObjectData> placedObjects = new();
    
    public Dictionary<string, GridObjectData> GetPlacedObjects() => placedObjects;
    public GridObjectData GetObjectData(string objectID) =>
        placedObjects.ContainsKey(objectID) ? placedObjects[objectID] : null;
    
    [Header("建造设置")]
    public BuildMode buildMode;
    
    [Header("物体设置")]
    public List<GridObjectConfig> availableObjects = new();
    public LayerMask gridLayerMask;
    
    [Header("预览设置")]
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;
    public Material previewMaterial;
    
    [Header("当前操作的物体")]
    [SerializeField]public GridObjectData currentObject;
    [SerializeField]private GridObjectConfig selectedObjectConfig;
    [SerializeField]private GameObject previewInstance;
    public bool isPlacing = false;
    
    private Camera mainCamera;
    private UIManager ui;
    
    void Start()
    {
        mainCamera  = Camera.main;
        ui = UIManager.instance;
        InitializeGrid();
    }
    
    void Update()
    {
        if (isPlacing && selectedObjectConfig != null)
        {
            UpdatePreview();
            
            if (Input.GetMouseButtonDown(0))
            {
                PlaceCurrentObject();
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
            
            if (Input.GetKeyDown(KeyCode.R) && currentObject.canRotate)
            {
                RotateCurrentObject();
            }
        }
        
        /*// 快捷键：删除物体
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            RemoveObject(currentObject.objectID);
            DeleteSelectedObject();
        }*/
        
    }

    void InitializeGrid()
    {
        gridCells = new GridCell[gridWidth, gridHeight];
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = GetWorldPosition(new Vector2Int(x, y));
                gridCells[x, y] = new GridCell(new Vector2Int(x, y), worldPos);
            }
        }
    }
    
    #region 放置功能
    
    // 开始放置物体
    public void StartPlacingObject(GridObjectConfig config)
    {
        if (isPlacing) CancelPlacement();
        
        selectedObjectConfig = config;
        currentObject = config.CreateInstance(Vector2Int.zero);
        isPlacing = true;
        
        // 创建预览实例
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = Instantiate(config.prefab);
        previewInstance.name = "Preview_" + config.objectName;
        
        // 应用预览材质
        ApplyPreviewMaterial(previewInstance, previewMaterial);
        
        // 隐藏碰撞器
        Collider[] colliders = previewInstance.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }
    
    // 更新预览位置
    public void UpdatePreview()
    {
        if (currentObject == null || previewInstance == null) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f, gridLayerMask))
        {
            Vector2Int gridPos = WorldToGridPosition(hit.point);
            currentObject.gridPosition = gridPos;
            
            // 更新预览物体位置
            Vector3 worldPos = GetWorldPosition(gridPos);
            
            // 考虑物体枢轴偏移
            Vector3 pivotOffset = new Vector3(
                selectedObjectConfig.pivotOffset.x * cellSize,
                0,
                selectedObjectConfig.pivotOffset.y * cellSize
            );
            
            previewInstance.transform.position = worldPos + pivotOffset;
            previewInstance.transform.rotation = Quaternion.Euler(0, (int)currentObject.orientation, 0);
            
            // 修正位置表现
            var piv = currentObject.pivotPosition;
            var times = (int)currentObject.orientation / 90;
            var offset = Vector2.zero;
            
            var a = new Vector2(piv.x + piv.y,piv.x - piv.y);
            for (int i = 0; i < times; i++)
            {
                a = new Vector2(a.y, -a.x);
                offset += a;
            }
            previewInstance.transform.position += new Vector3(offset.x, 0, offset.y);
            
            // 检查放置是否有效
            bool canPlace = CanPlaceObject(currentObject);
            
            // 更新预览材质
            ApplyPreviewMaterial(previewInstance, 
                canPlace ? validPlacementMaterial : invalidPlacementMaterial);
            
            // 高亮显示占用网格
            HighlightOccupiedCells(currentObject, canPlace);
        }
    }
    
    // 放置物体
    void PlaceCurrentObject()
    {
        if (currentObject == null || !CanPlaceObject(currentObject)) return;
        
        // 计算实际位置
        Vector3 worldPos = GetWorldPosition(currentObject.gridPosition);
        Vector3 pivotOffset = new Vector3(
            selectedObjectConfig.pivotOffset.x * cellSize,
            0,
            selectedObjectConfig.pivotOffset.y * cellSize
        );
        
        // 创建实际物体实例
        GameObject objInstance = Instantiate(selectedObjectConfig.prefab);
        objInstance.transform.position = worldPos + pivotOffset;
        objInstance.transform.rotation = Quaternion.Euler(0, (int)currentObject.orientation, 0);
        objInstance.name = $"{selectedObjectConfig.objectName}_{currentObject.objectID}";
        
        // 修正位置表现
        var piv = currentObject.pivotPosition;
        var times = (int)currentObject.orientation / 90;
        var offset = Vector2.zero;
            
        var a = new Vector2(piv.x + piv.y,piv.x - piv.y);
        for (int i = 0; i < times; i++)
        {
            a = new Vector2(a.y, -a.x);
            offset += a;
        }
        objInstance.transform.position += new Vector3(offset.x, 0, offset.y);
        
        // 更新物体数据
        currentObject.instance = objInstance;
        
        // 标记占用的网格
        List<Vector2Int> occupied = currentObject.GetOccupiedCells();
        foreach (Vector2Int cell in occupied)
        {
            if (IsInGrid(cell))
            {
                gridCells[cell.x, cell.y].SetOccupied(objInstance);
            }
        }
        
        // 保存物体
        placedObjects.Add(currentObject.objectID, currentObject);
        
        // 设置相关脚本
        PlaceableObject placeable = objInstance.AddComponent<PlaceableObject>();
        placeable.Initialize(currentObject.objectID, this, buildMode);
        
        Building building = objInstance.GetComponent<Building>();
        building.Initialize(currentObject.objectID, this);
        
        // 清除预览
        CancelPlacement();
    }
    
    // 取消放置
    void CancelPlacement()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
        
        currentObject = null;
        selectedObjectConfig = null;
        isPlacing = false;
        
        // 清除高亮
        ClearHighlights();
    }
    
    // 旋转当前物体
    void RotateCurrentObject()
    {
        if (currentObject == null) return;
        
        int currentAngle = (int)currentObject.orientation;
        currentAngle = (currentAngle + 90) % 360;
        currentObject.orientation = (ObjectOrientation)currentAngle;
        
        // 检查旋转后是否还能放置
        UpdatePreview();
    }
    
    #endregion
    
    #region 放置检查
    
    // 检查物体是否可以放置
    public bool CanPlaceObject(GridObjectData objectData)
    {
        if (objectData == null) return false;
        
        List<Vector2Int> occupiedCells = objectData.GetOccupiedCells();
        
        // 检查每个单元格
        foreach (Vector2Int cell in occupiedCells)
        {
            // 检查是否在网格范围内
            if (!IsInGrid(cell))
            {
                return false;
            }
            
            // 检查单元格是否可用
            GridCell gridCell = gridCells[cell.x, cell.y];
            if (!gridCell.IsAvailable() && gridCell.state != GridCellState.Preview)
            {
                return false;
            }
        }
        
        return true;
    }
    
    // 检查特定位置是否可以放置
    public bool CanPlaceAtPosition(GridObjectConfig config, Vector2Int gridPos, ObjectOrientation orientation)
    {
        GridObjectData tempObject = config.CreateInstance(gridPos);
        tempObject.orientation = orientation;
        
        return CanPlaceObject(tempObject);
    }
    
    // 检查网格坐标是否在范围内
    public bool IsInGrid(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }
    
    // 检查一组网格坐标是否全部在范围内
    public bool AreAllCellsInGrid(List<Vector2Int> cells)
    {
        foreach (Vector2Int cell in cells)
        {
            if (!IsInGrid(cell))
            {
                return false;
            }
        }
        return true;
    }
    
    #endregion
    
    #region 物体管理
    public GridObjectData GetSelectObject() => currentObject;
    // 选择物体
    public void SelectObject(string objectID)
    {
        if (placedObjects.TryGetValue(objectID, out var obj))
        {
            if (currentObject == null || obj.objectID != currentObject.objectID)
            {
                // 可以在这里添加选择效果
                currentObject = obj;
                currentObject.ShowDataUI(ui.objectInfoPanel);
            }
            else
            {
                DeSelectedObject();
            }
        }
    }
    
    // 取消选中
    public void DeSelectedObject()
    {
        currentObject.CloseDataUI(ui.objectInfoPanel);
        currentObject = null;
    }
    
    // 移除物体
    public void RemoveObject(string objectID)
    {
        if (!placedObjects.TryGetValue(objectID, out var objectData)) return;

        // 清除占用的网格
        List<Vector2Int> occupiedCells = objectData.GetOccupiedCells();
        foreach (Vector2Int cell in occupiedCells)
        {
            if (IsInGrid(cell))
            {
                gridCells[cell.x, cell.y].SetEmpty();
            }
        }
        
        // 销毁场景中的物体
        if (objectData.instance != null)
        {
            Destroy(objectData.instance);
        }
        
        // 从字典中移除
        placedObjects.Remove(objectID);
        
        Debug.Log($"物体移除成功: {objectID}");
    }
    
    // 移动物体
    public bool MoveObject(string objectID, Vector2Int newGridPosition)
    {
        if (!placedObjects.TryGetValue(objectID, out var objectData)) return false;

        // 临时移除原位置的占用标记
        List<Vector2Int> originalCells = objectData.GetOccupiedCells();
        foreach (Vector2Int cell in originalCells)
        {
            if (IsInGrid(cell))
            {
                gridCells[cell.x, cell.y].state = GridCellState.Empty;
            }
        }
        
        // 保存原位置
        Vector2Int oldPosition = objectData.gridPosition;
        objectData.gridPosition = newGridPosition;
        
        // 检查新位置是否可以放置
        if (!CanPlaceObject(objectData))
        {
            // 恢复原位置
            objectData.gridPosition = oldPosition;
            foreach (Vector2Int cell in originalCells)
            {
                if (IsInGrid(cell))
                {
                    gridCells[cell.x, cell.y].state = GridCellState.Occupied;
                }
            }
            return false;
        }
        
        // 更新物体位置
        if (objectData.instance != null)
        {
            Vector3 worldPos = GetWorldPosition(newGridPosition);
            Vector3 pivotOffset = new Vector3(
                GetObjectConfig(objectID)?.pivotOffset.x ?? 0 * cellSize,
                0,
                GetObjectConfig(objectID)?.pivotOffset.y ?? 0 * cellSize
            );
            var offset = new Vector2(pivotOffset.x + pivotOffset.y, pivotOffset.x - pivotOffset.y);
            var times = (int)objectData.orientation / 90;
            for (int t = 0; t < times; t++)
            {
                offset = new Vector2(offset.y, -offset.x);
            }
            objectData.instance.transform.position = worldPos + new Vector3(offset.x, 0, offset.y);
        }
        
        // 更新网格占用
        List<Vector2Int> newCells = objectData.GetOccupiedCells();
        foreach (Vector2Int cell in newCells)
        {
            if (IsInGrid(cell))
            {
                gridCells[cell.x, cell.y].SetOccupied(objectData.instance);
            }
        }
        
        return true;
    }

    public void RotateObject(string objectID)
    {
        if (objectID == null) return;
        if (placedObjects.TryGetValue(objectID, out var objectData))
        {
            // 检查是否冲突
            //记录旧数据
            int oldAngle = (int)currentObject.orientation;
            List<Vector2Int> originalCells = objectData.GetOccupiedCells();     
            //记录新数据
            int newAngle = (oldAngle + 90) % 360;
            currentObject.orientation = (ObjectOrientation)newAngle;
            List<Vector2Int> newCells = objectData.GetOccupiedCells();          
            
            foreach (Vector2Int cell in newCells)
            {
                if (!IsInGrid(cell))
                {
                    Debug.Log("超出边界！");
                    currentObject.orientation = (ObjectOrientation)oldAngle;    //还原
                    return;
                }

                var gridCell = gridCells[cell.x, cell.y];
                if (gridCells[cell.x, cell.y].occupyingObject != objectData.instance && 
                    gridCell.state is GridCellState.Occupied or GridCellState.Blocked)
                {
                    Debug.Log("位置冲突！");
                    currentObject.orientation = (ObjectOrientation)oldAngle;    //还原
                    return;
                }
            }
            
            // 临时移除原位置的占用标记
            foreach (Vector2Int cell in originalCells)
            {
                if (IsInGrid(cell))
                {
                    gridCells[cell.x, cell.y].state = GridCellState.Empty;
                }
            }
            
            // 更新表现
            var objTrans = objectData.instance.transform;
            objTrans.rotation = Quaternion.Euler(0, (int)placedObjects[objectID].orientation, 0);
            var piv = objectData.pivotPosition;
            var offset = new Vector2(piv.x + piv.y, piv.x - piv.y);
            var times = (int)currentObject.orientation / 90;
            for (int t = 0; t < times; t++)
            {
                offset = new Vector2(offset.y, -offset.x);
            }

            objTrans.position += new Vector3(offset.x, objTrans.position.y, offset.y);

            // 更新网格占用
            foreach (Vector2Int cell in newCells)
            {
                if (IsInGrid(cell))
                {
                    gridCells[cell.x, cell.y].SetOccupied(objectData.instance);
                }
            }
        }

    }
    
    
    GridObjectConfig GetObjectConfig(string objectID)
    {
        if (!placedObjects.ContainsKey(objectID)) return null;
        
        string prefabName = placedObjects[objectID].prefab.name.Replace("(Clone)", "");
        foreach (GridObjectConfig config in availableObjects)
        {
            if (config.prefab.name == prefabName)
            {
                return config;
            }
        }
        return null;
    }
    
    #endregion
    
    #region 可视化辅助
    
    // 高亮显示占用的单元格
    void HighlightOccupiedCells(GridObjectData objectData, bool isValid)
    {
        ClearHighlights();
        
        List<Vector2Int> occupiedCells = objectData.GetOccupiedCells();
        
        foreach (Vector2Int cell in occupiedCells)
        {
            if (IsInGrid(cell) && gridCells[cell.x, cell.y].state != GridCellState.Occupied)
            {
                gridCells[cell.x, cell.y].SetPreview(null);
                
                // 这里可以添加更明显的高亮效果
                // 比如创建临时的方块或改变网格颜色
            }
        }
    }
    
    // 清除高亮
    void ClearHighlights()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridCells[x, y].state == GridCellState.Preview)
                {
                    gridCells[x, y].state = GridCellState.Empty;
                }
            }
        }
    }
    
    // 应用预览材质
    void ApplyPreviewMaterial(GameObject obj, Material material)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }
            renderer.materials = materials;
        }
    }
    
    #endregion
    
    #region 坐标转换
    
    // 世界坐标转网格坐标
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - transform.position;
        int x = Mathf.FloorToInt((localPos.x - gridOffset.x) / cellSize);
        int y = Mathf.FloorToInt((localPos.z - gridOffset.y) / cellSize);
        
        return new Vector2Int(Mathf.Clamp(x, 0, gridWidth - 1), 
                            Mathf.Clamp(y, 0, gridHeight - 1));
    }
    
    // 网格坐标转世界坐标（返回单元格中心）
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        float x = transform.position.x + gridPosition.x * cellSize + gridOffset.x + cellSize / 2f;
        float z = transform.position.z + gridPosition.y * cellSize + gridOffset.y + cellSize / 2f;
        float y = transform.position.y;
        
        return new Vector3(x, y, z);
    }
    
    // 获取单元格世界坐标
    public Vector3 GetCellWorldPosition(Vector2Int gridPosition)
    {
        return gridCells[gridPosition.x, gridPosition.y].worldPosition;
    }
    
    #endregion
    
    #region 调试与可视化
    
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // 绘制网格边界
        Gizmos.color = Color.cyan;
        Vector3 size = new Vector3(gridWidth * cellSize, 0.1f, gridHeight * cellSize);
        Vector3 center = transform.position + new Vector3(
            gridWidth * cellSize / 2 + gridOffset.x,
            0,
            gridHeight * cellSize / 2 + gridOffset.y
        );
        Gizmos.DrawWireCube(center, size);
        
        // 绘制不可放置的区域（如果存在）
        // ...
    }
    
    // 在Scene视图中显示网格信息
    void OnDrawGizmosSelected()
    {
        // 绘制所有占用状态
        if (gridCells != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    GridCell cell = gridCells[x, y];
                    
                    switch (cell.state)
                    {
                        case GridCellState.Occupied:
                            Gizmos.color = Color.red;
                            break;
                        case GridCellState.Blocked:
                            Gizmos.color = Color.gray;
                            break;
                        case GridCellState.Preview:
                            Gizmos.color = Color.yellow;
                            break;
                        default:
                            Gizmos.color = Color.green;
                            break;
                    }
                    
                    Vector3 pos = GetCellWorldPosition(new Vector2Int(x, y));
                    Gizmos.DrawWireCube(pos + Vector3.up * 0.05f, 
                        new Vector3(cellSize * 0.9f, 0.01f, cellSize * 0.9f));
                }
            }
        }
    }
    
    #endregion
}
