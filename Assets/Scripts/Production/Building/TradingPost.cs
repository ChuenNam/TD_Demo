using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradingPost : Building, ITrading, ILevelUp
{
    [Header("专属信息")] 
    [SerializeField]private bool isSell = true;
    public Blueprint order = new();
    
    [Header("等级数据")] 
    public int level;
    public int maxLevel = 2;
    public SetLevelBuffMode setLevelBuffMode;
    public List<Level> eachLevelBuff = new(); 
    public int Level
    {
        get => level;
        set => level = value;
    }
    public int MaxLevel
    {
        get => maxLevel;
        set => maxLevel = value;
    }
    public List<Level> GetEachLevelBuff() => eachLevelBuff;
    public SetLevelBuffMode GetLevelBuffMode() => setLevelBuffMode;

    public override void Initialize(string id, GridManager manager)
    {
        base.Initialize(id, manager);
        ((ILevelUp)this).LevelUp(this);

        onComplete += () =>
        {
            ChangeProduction();     //结束生产
            
            objectData.UpdateDataUI();
            Debug.Log("贸易完毕");
        };
    }

    public string RewriteBlueprintText(Blueprint blueprint)
    {
        var sellOrBuy = isSell ? "出售" : "进购";
        return $"{sellOrBuy} {blueprint.blueprintName} X{blueprint.useGroup[0].count}";
    }

    public Blueprint SetGetCurrentBpByItem(BaseItem item)
    {
        foreach (var bp in blueprints.Where(bp => bp.useGroup[0].item == item))
        {
            CurrentBlueprint = bp;
            return CurrentBlueprint;
        }
        return null;
    }

    public void SetSellOrder(Blueprint bp, int sellCount)
    {
        isSell = true;
        order.CopyBlueprintValuesFrom(bp);    //获取基础交易内容
        
        //设置交易数量和收入
        order.useGroup[0].count = sellCount;
        order.productGroup[0].count += order.useGroup[0].item.price * sellCount;
        
        //返回给生产蓝图
        CurrentBlueprint = order;
    }
    public void SetBuyOrder(Blueprint bp, int buyCount)
    {
        isSell = false;
        order.CopyBlueprintValuesFrom(bp);
        //转换交易物品
        (order.useGroup[0].item, order.productGroup[0].item) = (order.productGroup[0].item, order.useGroup[0].item);
        //设置交易数量和支出
        order.useGroup[0].count = order.productGroup[0].item.price * buyCount;
        order.productGroup[0].count += buyCount + order.useGroup[0].count;
        //返回给生产蓝图
        CurrentBlueprint = order;
    }
}