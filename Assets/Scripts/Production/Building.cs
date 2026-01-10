using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Building : MonoBehaviour
{
    public GridObjectData objectData;

    public int level = 1;
    public List<Blueprint> blueprints;
    public List<Buff> buffList = new();

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

    public float multiplier = 2;
    private void Update()
    {
        // 测试
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var buff = BuffManager.CreatMultipleProductivityBuff(this, multiplier, 10);
            AddBuff(buff);
        }    
    }

    public void Product()
    {
        if (timeCounter >= CurrentBlueprint.Time)
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

    public void UpdateBuff()
    {
        foreach (var buff in buffList.ToList())
        {
            if (buff.duration == -1)
                continue;
            
            buff.remainDuration -= Time.deltaTime;
            if (buff.remainDuration <= 0)
            {
                DelBuff(buff);
            }
        }
    }
    public void AddBuff(Buff buff)
    {
        buffList.Add(buff);
        buff.addBuffAction?.Invoke();
        Debug.Log($"添加{buff}-时间{buff.duration}");
    }
    public void DelBuff(Buff buff)
    {
        buffList.Remove(buff);
        buff.delBuffAction?.Invoke();
        Debug.Log($"移除{buff}");
    }

    public Blueprint GetBlueprintByProductInfo(string productInfo)
    {
        foreach (var bp in blueprints)
            if (bp.ProductInfo() == productInfo)
                return bp;
        return null;
    }
}
