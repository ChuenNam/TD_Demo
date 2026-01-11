using System.Collections.Generic;
using UnityEngine;

public class TradingPost : Building, ITrading
{
    [Header("专属信息")]
    public Blueprint order = new();

    public override void Initialize(string id, GridManager manager)
    {
        base.Initialize(id, manager);
    }

    public string RewriteBlueprintText(Blueprint blueprint)
    {
        return $"blueprint.blueprintName X{blueprint.useGroup[0].count}";
    }

    public void SetSellOrder(Blueprint bp, int sellCount)
    {
        //设置交易物品
        order.useGroup.Add(bp.useGroup[0]);
        order.productGroup.Add(bp.productGroup[0]);
        
        //设置交易数量和收入
        order.useGroup[0].count = sellCount;
        order.productGroup[0].count = order.useGroup[0].item.price * sellCount;
    }
    public void SetBuyOrder(Blueprint bp, int buyCount)
    {
        //设置交易物品
        order.useGroup.Add(bp.useGroup[0]);
        order.productGroup.Add(bp.productGroup[0]);
        
        //设置交易数量和支出
        order.useGroup[0].count = order.productGroup[0].item.price * buyCount;
        order.productGroup[0].count = buyCount;
    }
}