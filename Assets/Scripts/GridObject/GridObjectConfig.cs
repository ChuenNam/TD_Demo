using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// GridObjectConfig.cs
[CreateAssetMenu(fileName = "NewGridObject", menuName = "Grid System/Grid Object")]
public class GridObjectConfig : ScriptableObject
{
    [Header("基础信息")]
    public string objectName;
    public GameObject prefab;
    public Sprite icon;
    
    [TextArea]
    public string description;
    
    [Header("网格占用")]
    public List<Vector2Int> occupiedCells = new(); // 相对坐标
    public List<Vector2Int> buffCells = new();      // buff网格坐标
    
    [Header("物体设置")]
    public Vector2 pivotOffset = Vector2.zero; // 物体枢轴偏移（相对于左下角）
    public bool canRotate = true;

    [Header("蓝图设置")] 
    public BlueprintConfig blueprintConfig;     // 蓝图
    
    [Header("编辑器显示")]
    public Color editorColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
    public Color pivotColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
    
    // 编辑器用的方法
    public void AddCell(Vector2Int cell)
    {
        if (!occupiedCells.Contains(cell))
        {
            occupiedCells.Add(cell);
        }
    }
    public void AddBuffCell(Vector2Int cell)
    {
        if (!buffCells.Contains(cell))
        {
            buffCells.Add(cell);
        }
    }
    
    public void RemoveCell(Vector2Int cell)
    {
        occupiedCells.Remove(cell);
    }
    public void RemoveBuffCell(Vector2Int cell)
    {
        buffCells.Remove(cell);
    }
    
    public bool ContainsCell(Vector2Int cell)
    {
        return occupiedCells.Contains(cell);
    }
    public bool ContainsBuffCell(Vector2Int cell)
    {
        return buffCells.Contains(cell);
    }
    
    public void ClearCells()
    {
        occupiedCells.Clear();
    }
    public void ClearBuffCells()
    {
        buffCells.Clear();
    }
    
    // 获取边界框
    public BoundsInt GetBounds()
    {
        if (occupiedCells.Count == 0)
            return new BoundsInt(Vector3Int.zero, Vector3Int.zero);
            
        Vector2Int min = occupiedCells[0];
        Vector2Int max = occupiedCells[0];
        
        foreach (var cell in occupiedCells)
        {
            min = Vector2Int.Min(min, cell);
            max = Vector2Int.Max(max, cell);
        }
        
        Vector3Int center = new Vector3Int(
            Mathf.RoundToInt((min.x + max.x) * 0.5f),
            Mathf.RoundToInt((min.y + max.y) * 0.5f),
            0
        );
        
        Vector3Int size = new Vector3Int(
            max.x - min.x + 1,
            max.y - min.y + 1,
            1
        );
        
        return new BoundsInt(center, size);
    }
    
    // 创建物体数据实例
    public GridObjectData CreateInstance(Vector2Int gridPosition)
    {
        return new GridObjectData
        {
            name = objectName,
            description = description,
            objectID = System.Guid.NewGuid().ToString(),
            prefab = prefab,
            gridPosition = gridPosition,
            pivotPosition = pivotOffset,
            orientation = ObjectOrientation.North,
            occupiedCells = new List<Vector2Int>(occupiedCells),
            canRotate = canRotate,
            blueprintConfig = blueprintConfig
        };
    }
}

[Serializable]
public class GridObjectConfigList
{
    public List<GridObjectConfig> configs = new();
    public static List<GridObjectConfig> AllConfigs(List<GridObjectConfigList> targetList)
    {
        return targetList.SelectMany(configList => configList.configs).ToList();
    }
}