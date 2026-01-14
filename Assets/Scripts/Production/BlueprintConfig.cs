using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class ItemGroup
{
    public BaseItem item;
    public int count;

    public ItemGroup(ItemGroup other)
    {
        this.count = other.count;
        this.item = other.item;
    }

    public ItemGroup() { }
}

[Serializable]
public class Blueprint
{
    public string blueprintName;
    public Sprite icon;
    public bool isLocked;
    public float baseTime = 1;
    public float timeMultiplier = 1;
    public float Time => baseTime/timeMultiplier;
    
    public List<ItemGroup> useGroup = new();
    public List<ItemGroup> productGroup = new();

    public string Info()
    {
        var usetxt = GetItemGroupInfo(useGroup);
        var useInfo = $"{usetxt}({Time:F1}秒)";
        //var useInfo = usetxt == "" ? $"{Time:F}秒" : usetxt;
        var productInfo = GetItemGroupInfo(productGroup);
        return  $"{useInfo} → {productInfo}";
    }
    public string Info(Building building)      //贸易类建筑:转换蓝图信息 -> 商品信息
    {
        if (building is ITrading tradeBuilding)
        {
            return tradeBuilding.RewriteBlueprintText(this);
        }
        
        var usetxt = GetItemGroupInfo(useGroup);
        var useInfo = $"{usetxt}({Time:F1}秒)";
       
        var productInfo = GetItemGroupInfo(productGroup);
        return  $"{useInfo} → {productInfo}";
    }
    public string ProductInfo(Building building)
    {
        if (building is ITrading tradeBuilding)
        {
            return tradeBuilding.RewriteBlueprintText(this);
        }
        return GetItemGroupInfo(productGroup);
    }

    public string GetItemGroupInfo(List<ItemGroup> itemGroups)
    {
        string txt = "";
        for (var i = 0; i < itemGroups.Count; i++)
        {
            var group = itemGroups[i];
            if (group.item is null)
                txt += "空";
            else
                txt += group.count + group.item.itemName;
            if (i == itemGroups.Count-1) break;
            txt += "+";
        }
        return txt;
    }

    public void CopyBlueprintValuesFrom(Blueprint other)
    {
        // 健壮性判断：避免空引用异常
        if (other == null)
            throw new ArgumentNullException(nameof(other), "源对象不能为 null");
        
        blueprintName = other.blueprintName;
        isLocked = other.isLocked;
        baseTime = other.baseTime;
        timeMultiplier = other.timeMultiplier;
        // 复制集合（深度值复制，创建新集合+新元素）
        useGroup?.Clear();     // 先清空 a 的原有集合（避免残留旧数据）
        foreach (var itemGroup in other.useGroup)
        {
            // 利用 ItemGroup 的拷贝构造函数，创建新实例
            var newItemGroup = new ItemGroup(itemGroup);
            useGroup.Add(newItemGroup);
        }
        productGroup?.Clear();
        foreach (var itemGroup in other.productGroup)
        {
            var newItemGroup = new ItemGroup(itemGroup);
            productGroup.Add(newItemGroup);
        }
    }
    
}

[CreateAssetMenu(fileName = "New BlueprintConfig", menuName = "Blueprint/New BlueprintConfig")]
public class BlueprintConfig : ScriptableObject
{
    public List<Blueprint> blueprints;
    
    public static List<Blueprint> GetCopyBlueprints(List<Blueprint> blueprints)
    {
        var copy = new List<Blueprint>();
        foreach (var bp in blueprints)
        {
            var newBp = new Blueprint();
            newBp.CopyBlueprintValuesFrom(bp);
            copy.Add(newBp);
        }
        return copy;
    }
}