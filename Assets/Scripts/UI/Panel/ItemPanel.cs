using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconUI
{
    private Image image;
    private Text count;

    public IconUI(Image img, Text txt)
    {
        this.image = img;
        this.count = txt;
    }
    
    public void SetData(Sprite s, string c)
    {
        this.image.sprite = s;
        this.count.text = c;
    }
}

public class ItemPanel : BasePanel
{
    public ItemManager itemManager;
    public List<BaseItem> items;
    private List<IconUI> iconsUI = new();

    public RectTransform panel;
    public GameObject iconPrefab;

    protected override void Init()
    {
        base.Init();
        itemManager = ItemManager.instance;
        items = itemManager.possesItem;

        foreach (var item in items)
        {
            var icon = Instantiate(iconPrefab, panel);
            var image = icon.GetComponent<Image>();
            var text = icon.transform.GetChild(0).GetChild(0)
                .GetComponent<Text>();

            var iconUI = new IconUI(image,text);
            iconUI.SetData(item.icon,item.count.ToString());
            iconsUI.Add(iconUI);
        }
    }

    private void Update()
    {
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            
            if (i >= iconsUI.Count)
            {
                var icon = Instantiate(iconPrefab, panel);
                var image = icon.GetComponent<Image>();
                var text = icon.transform.GetChild(0).GetChild(0)
                    .GetComponent<Text>();  
                
                var newIconUI = new IconUI(image,text);
                newIconUI.SetData(item.icon,item.count.ToString());
                iconsUI.Add(newIconUI);
            }
           
            iconsUI[i].SetData(item.icon,item.count.ToString());
        }
    }
}

