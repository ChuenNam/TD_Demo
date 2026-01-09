using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public GridObjectData objectData;

    public int level = 1;
    public List<Blueprint> blueprints;

    [SerializeField]private Blueprint _currentBlueprint;
    public Blueprint CurrentBlueprint
    {
        get => _currentBlueprint;
        set => _currentBlueprint = value;
    }

    public bool inProduction;
    public float timeCounter;

    public void Initialize(string id, GridManager manager)
    {
        objectData = manager.GetObjectData(id);
        blueprints = objectData.blueprintConfig.blueprints;
        CurrentBlueprint = blueprints[0];
    }
    
    public void Product()
    {
        if (timeCounter >= CurrentBlueprint.time)
        {
            timeCounter = 0;        // 重置计时器
            
            // 更新资源数据
            foreach (var useItemGroup in CurrentBlueprint.useGroup)
            {
                if (useItemGroup.item.count - useItemGroup.count < 0)
                {
                    Debug.Log("原料不足");
                    inProduction  = false;
                    CurrentBlueprint = null;
                    return;
                }
                useItemGroup.item.count -= useItemGroup.count;
            }
            foreach (var productItemGroup in CurrentBlueprint.productGroup)
            {
                productItemGroup.item.count += productItemGroup.count;
            }
        }
    }

    public Blueprint GetBlueprintByProductInfo(string productInfo)
    {
        foreach (var bp in blueprints)
            if (bp.ProductInfo() == productInfo)
                return bp;
        return null;
    }
}
