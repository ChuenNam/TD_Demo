using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Item", menuName = "Item/New Item")]
[Serializable]
public class BaseItem : ScriptableObject
{
    [Header("基础信息")]
    public string itemName;
    public Sprite icon;
    [TextArea]
    public string description;
        
    [Header("数据信息")]
    public int price;
    public int count;
}
