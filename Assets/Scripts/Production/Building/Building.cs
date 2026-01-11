using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IDayNightBonus
{
    float GetDayBonus();
    float GetNightBonus();
}

public interface ITrading
{
    
}

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
        foreach (var bp in blueprints)
            bp.timeMultiplier = 1;
        
        BuffManager.instance.AddBuffToBuilding(this);
        AddBuff(TimeLogic.instance.isDay
            ? BuffManager.instance.DayBuff(this)
            : BuffManager.instance.NightBuff(this));
        this.objectData.UpdateDataUI();
    }

    //public float multiplier = 2;
    private void Update()
    {
        // 测试
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var buff = BuffManager.CreatProductivityBuff(this, 2, 10);
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
            if (Mathf.Approximately(buff.duration, -1))
                continue;
            
            buff.remainDuration -= Time.deltaTime * TimeLogic.instance.timeSpeed;
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
        //Debug.Log($"添加{buff}-时间{buff.duration}");
    }
    public void AddBuff(List<Buff> buffs)
    {
        foreach (var buff in buffs)
        {
            buffList.Add(buff);
            buff.addBuffAction?.Invoke();
        }
    }
    public void DelBuff(Buff buff)
    {
        buffList.Remove(buff);
        buff.delBuffAction?.Invoke();
        //Debug.Log($"移除{buff}");
    }
    public void DelBuff(List<Buff> buffs)
    {
        foreach (var buff in buffs)
        {
            buffList.Remove(buff);
            buff.delBuffAction?.Invoke();
        }
    }
    public void ClearBuff()
    {
        foreach (var buff in buffList)
        {
            buff.delBuffAction?.Invoke();
        }
        buffList.Clear();
    }

    public Blueprint GetBlueprintByProductInfo(string productInfo)
    {
        foreach (var bp in blueprints)
            if (bp.ProductInfo() == productInfo)
                return bp;
        return null;
    }
}
