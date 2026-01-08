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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Next");
            idx = (idx + 1)%itemManager.possesItem.Count;
            
            item = itemManager.possesItem[idx];
            itemName = item.name;
            count = item.count;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            count++;
            item.count = count;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            count--;
            item.count = count;
        }
    }
}
