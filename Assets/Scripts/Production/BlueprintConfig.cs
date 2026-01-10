using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class ItemGroup
{
    public BaseItem item;
    public int count;
}

[Serializable]
public class Blueprint
{
    public bool isLocked;
    public float baseTime = 1;
    public float timeMultiplier = 1;
    public float Time => baseTime/timeMultiplier;
    
    public List<ItemGroup> useGroup;
    public List<ItemGroup> productGroup;

    public string Info()
    {
        var usetxt = GetItemGroupInfo(useGroup);
        var useInfo = usetxt == "" ? $"{Time:F}秒" : usetxt;
        var productInfo = GetItemGroupInfo(productGroup);
        return  $"{useInfo} → {productInfo}";
    }
    public string ProductInfo() => GetItemGroupInfo(productGroup);

    private string GetItemGroupInfo(List<ItemGroup> itemGroups)
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
}

[CreateAssetMenu(fileName = "New BlueprintConfig", menuName = "Blueprint/New BlueprintConfig")]
public class BlueprintConfig : ScriptableObject
{
    public List<Blueprint> blueprints;
}