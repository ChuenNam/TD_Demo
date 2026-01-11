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
    string RewriteBlueprintText(Blueprint blueprint);
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

    public virtual void Initialize(string id, GridManager manager)
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

    private void OnDisable()
    {
        // 结束时清除所有Buff
        ClearBuff();
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

    #region 生产
    // 回调事件
    public Action onStart;
    public Action onComplete;

    public virtual void ChangeProduction()
    {
        // 停止生产 || 未选择配方
        if (inProduction || CurrentBlueprint == null)
        {
            inProduction = false;
            CurrentBlueprint = null;
            return;
        }
        // 消耗资源检测
        foreach (var useItemGroup in CurrentBlueprint.useGroup)
        {
            if (useItemGroup.item.count - useItemGroup.count >= 0) 
                continue;
            
            Debug.Log("原料不足，结束生产");
            inProduction = false;
            CurrentBlueprint = null;
            return;
        }
        // 检测通过 - 开始生产
        inProduction = true;
        foreach (var useItemGroup in CurrentBlueprint.useGroup) 
            useItemGroup.item.count -= useItemGroup.count;  // 更新资源数据
        onStart?.Invoke();      //开始生产回调
    }
    public virtual void Product()
    {
        if (!(timeCounter >= CurrentBlueprint.Time)) 
            return;
        
        timeCounter = 0; // 重置计时器
        // 更新资源数据
        foreach (var productItemGroup in CurrentBlueprint.productGroup)
        {
            productItemGroup.item.count += productItemGroup.count;
        }
        onComplete?.Invoke();   // 完成生产回调
        inProduction = false;
        ChangeProduction();      // 循环生产
    }
    #endregion

    #region Buff处理

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

    #endregion
    
    public Blueprint GetBlueprintByProductInfo(string productInfo)
    {
        foreach (var bp in blueprints)
            if (bp.ProductInfo(this) == productInfo)
                return bp;
        return null;
    }
}
