using UnityEngine;
using System.Collections.Generic;

// 物体配置（ScriptableObject，用于创建不同类型的物体）
[CreateAssetMenu(fileName = "NewGridObject", menuName = "Grid System/Grid Object")]
public class GridObjectConfig : ScriptableObject
{
    public string objectName;
    public GameObject prefab;
    public List<Vector2Int> occupiedCells = new(); // 相对坐标
    public Vector2 pivotOffset = Vector2.zero; // 物体枢轴偏移（相对于左下角）
    public bool canRotate = true;
    public int rotationSnap = 90; // 旋转角度间隔
    
    [TextArea]
    public string description;
    
    // 创建物体数据实例
    public GridObjectData CreateInstance(Vector2Int gridPosition)
    {
        return new GridObjectData
        {
            objectID = System.Guid.NewGuid().ToString(),
            prefab = prefab,
            gridPosition = gridPosition,
            orientation = ObjectOrientation.North,
            occupiedCells = new List<Vector2Int>(occupiedCells),
            canRotate = canRotate,
        };
    }
}