using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemManager : MonoBehaviour
{
    #region 单例 instance
        public static ItemManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }
    #endregion 
    
    public List<BaseItem> possesItem = new();

    private void OnEnable()
    {
        foreach (var item in possesItem)
        {
            item.count = 0;
        }
        GetItemByName("金币").count = 10;
    }

    public BaseItem GetItemByName(string itemName)
    {
        return possesItem.FirstOrDefault(item => item.itemName == itemName);
    }

    public int GetItemCount(BaseItem targetItem)
    {
        return possesItem.FirstOrDefault(item => item == targetItem)!.count;
    }


}
