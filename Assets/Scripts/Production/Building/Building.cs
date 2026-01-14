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

public interface ILevelUp
{
    public int Level { get; set; }
    public int LimitLevel { get; set; }
    int MaxLevel { get; set; }
    List<Level> GetEachLevelBuff();
    SetLevelBuffMode GetLevelBuffMode();

    string GetCostInfo()
    {
        var cost = "";
        for (var i = 0; i < GetEachLevelBuff()[Level].levelUpCost.Count; i++)
        {
            var group = GetEachLevelBuff()[Level].levelUpCost[i];
            cost += $"{group.count}{group.item.itemName}";
            if (i == GetEachLevelBuff()[Level].levelUpCost.Count - 1) 
                return cost;
            cost += "+";
        }
        return cost;
    }

    void OnLevelUpCompleted(){}
    void LevelUp(Building building)
    {
        // 非餐厅建筑
        if (building is not Restaurant)
        {
            if (LimitLevel != MaxLevel)     // 未达到最高等级
            {
                if (TimeLogic.instance.buildings.FirstOrDefault(r => r.GetType() == typeof(Restaurant)) 
                    is Restaurant restaurant)           // 若有场上餐厅
                    LimitLevel = restaurant.Level;      // 升级时将等级上限设为餐厅等级
                else
                    LimitLevel = 1;     // 场上没餐厅则保持等级上限为 1 (放置建筑时默认调用 触发)
            }
        }
        
        if (Level >= LimitLevel)
        {
            Level = LimitLevel;
            UIManager.instance.helpPanel.Show("已经到达等级上限");
            return;
        }
        // 检查材料
        var cost = GetEachLevelBuff()[Level].levelUpCost;
        if (cost.Any(group => group.count > ItemManager.instance.GetItemCount(group.item)))
        {
            UIManager.instance.helpPanel.Show("材料不足");
            return;
        }
        
        // 扣除材料
        foreach (var group in cost) 
            group.item.count -= group.count;
        
        // 添加buff
        var levelUpBuff = GetEachLevelBuff()[Level];
        var buffsToAdd = new List<Buff>();
        foreach (var config in levelUpBuff.buffConfigs)
        {
            switch (config.buffType)
            {
                case BuffType.Productivity:
                    var buff = BuffManager.CreatProductivityBuff(building, config);
                    buff.buffName += 'L';
                    buffsToAdd.Add(buff);
                    break;
                case BuffType.ExtraOutput:
                    buff = BuffManager.CreatExtraOutputBuff(building, config);
                    buff.buffName += 'L';
                    buffsToAdd.Add(buff);
                    break;
                case BuffType.UnlockBlueprint:
                    buff = BuffManager.CreateUnLockBpBuff(building, config);
                    buff.buffName += 'L';
                    buffsToAdd.Add(buff);
                    break;
            }
        }
        // 根据添加方式添加
        if (Level == 0)
            building.AddBuff(buffsToAdd);
        else
        {
            switch (GetLevelBuffMode())
            {
                case SetLevelBuffMode.Add:
                    building.AddBuff(buffsToAdd);
                    break;
                case SetLevelBuffMode.Replace:
                    foreach (var buff in building.buffList.ToList())
                    {
                        if (buff.buffName[^1] == 'L')
                        {
                            building.DelBuff(buff);
                        }
                    }
                    building.AddBuff(buffsToAdd);
                    break;
            }
        }
        Level++;        // 升级
        OnLevelUpCompleted();
    }
}

public class Building : MonoBehaviour
{
    public GridObjectData objectData;
    
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
        blueprints = BlueprintConfig.GetCopyBlueprints(objectData.blueprintConfig.blueprints);
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
            UIManager.instance.objectInfoPanel.WriteInfo(objectData);
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
        UIManager.instance.objectInfoPanel.WriteBuffInfo(this);
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
