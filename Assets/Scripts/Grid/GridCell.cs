using UnityEngine;
using System.Collections.Generic;

// 网格单元格
[System.Serializable]
public class GridCell
{
    public Vector2Int coordinates;
    public GridCellState state;
    public GameObject occupyingObject; // 占用该单元格的物体
    public Vector3 worldPosition;      // 世界坐标
    
    public List<Buff> buffs;            // 拥有的buff
    
    public GridCell(Vector2Int coords, Vector3 worldPos)
    {
        coordinates = coords;
        worldPosition = worldPos;
        state = GridCellState.Empty;
        occupyingObject = null;
    }
    
    public bool IsAvailable()
    {
        return state == GridCellState.Empty;
    }
    
    public void SetOccupied(GameObject obj)
    {
        if (state == GridCellState.Occupied)
        {
            Debug.Log("位置冲突！");
            return;
        }
        state = GridCellState.Occupied;
        occupyingObject = obj;
    }
    
    public void SetEmpty()
    {
        state = GridCellState.Empty;
        occupyingObject = null;
    }
    
    public void SetPreview(GameObject obj)
    {
        state = GridCellState.Preview;
        occupyingObject = obj;
    }
}