using System.Collections.Generic;
using UnityEngine;

// 物体朝向枚举
public enum ObjectOrientation
{
    North = 0,  // 默认朝向
    East = 90,
    South = 180,
    West = 270
}

// 网格占用状态
public enum GridCellState
{
    Empty = 0,
    Occupied,
    Blocked,
    Preview
}

// 单个物体数据
[System.Serializable]
public class GridObjectData
{
    public string name;                         // 名称
    public string description;                  // 描述
    public string objectID;                    // 物体唯一标识
    public GameObject prefab;                  // 预制体
    public Vector2Int gridPosition;           // 左下角网格坐标
    public Vector2 pivotPosition;               // 枢轴坐标
    public ObjectOrientation orientation;      // 朝向
    public List<Vector2Int> occupiedCells;    // 占用的网格位置（相对坐标）
    public GameObject instance;               // 场景中的实例
    public bool canRotate = true;             // 是否可以旋转
    public BlueprintConfig blueprintConfig;     // 拥有的蓝图
    
    // 根据朝向计算实际占用的网格
    public List<Vector2Int> GetOccupiedCells()
    {
        List<Vector2Int> rotatedCells = new List<Vector2Int>();
        
        if (occupiedCells != null)
            foreach (Vector2Int cell in occupiedCells)
            {
                Vector2Int rotatedCell = RotateCell(cell, orientation);
                rotatedCells.Add(gridPosition + rotatedCell);
            }
            
        return rotatedCells;
    }
    
    // 旋转单个相对坐标
    private Vector2Int RotateCell(Vector2Int cell, ObjectOrientation orientation)
    {
        switch (orientation)
        {
            case ObjectOrientation.North:
                return cell;
            case ObjectOrientation.East:
                return new Vector2Int(cell.y, -cell.x);
            case ObjectOrientation.South:
                return new Vector2Int(-cell.x, -cell.y);
            case ObjectOrientation.West:
                return new Vector2Int(-cell.y, cell.x);
            default:
                return cell;
        }
    }
    
    // 计算物体边界框（用于快速检查）
    public BoundsInt GetBounds()
    {
        List<Vector2Int> cells = GetOccupiedCells();
        
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;
        
        foreach (Vector2Int cell in cells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y > maxY) maxY = cell.y;
        }
        
        return new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
    }

    public void ShowDataUI(ObjectInfoPanel panel)
    {
        panel.ShowPanel();
        panel.WriteInfo(this);
    }
    public void UpdateDataUI()
    {
        if (UIManager.instance.objectInfoPanel.data != this)
            return;
        UIManager.instance.objectInfoPanel.WriteInfo(this);
    }
    public void CloseDataUI(ObjectInfoPanel panel)
    {
        panel.ClosePanel();
    }
}