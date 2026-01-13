using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    public BaseItem item;
    public string itemName;
    public int count;
    private int idx;
    
    public ItemManager itemManager;

    private void Start()
    {
        itemManager = ItemManager.instance;
        idx = 0;
        
        item = itemManager.possesItem[idx];
        itemName = item.name;
        count = item.count;
    }
}
