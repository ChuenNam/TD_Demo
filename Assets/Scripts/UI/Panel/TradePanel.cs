using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class TradePanel : BasePanel
{
    [Header("数据信息")]
    public bool isSell = true;
    public BaseItem chosenItem;
    public int dealAmount;
    public TradingPost tradingPost;
    
    [Header("UI控件")] 
    public Dropdown choseItemDropdown;
    public Dropdown sellOrBuyDropdown;
    
    public Image icon;
    public Text itemInfo;

    public Text amountText;
    public Button addButton;
    public Button subButton;
    public Slider amountSlider;
    public Text timeText;

    public Text finalMoney;
    public Button dealButton;


    protected override void Init()
    {
        base.Init();
        closeButton.onClick.AddListener(() => TimeLogic.instance.timeSpeed = 1);
        
        choseItemDropdown.onValueChanged.RemoveAllListeners();
        sellOrBuyDropdown.onValueChanged.RemoveAllListeners();
        choseItemDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        sellOrBuyDropdown.onValueChanged.AddListener(OnSellBuyDropdownValueChanged);
    }
    

    public void InitTradePanel(TradingPost tradingPost)
    {
        this.tradingPost = tradingPost;
        // 菜单更新逻辑
        choseItemDropdown.ClearOptions();
        List<string> optionTexts = new();
        foreach (var item in ItemManager.instance.possesItem)
        {
            optionTexts.Add(item.itemName);
        }
        var optionList = new List<Dropdown.OptionData>();
        foreach (var text in optionTexts)
        {
            // 创建OptionData实例，设置选项文本（还可设置图标：new Dropdown.OptionData(sprite, text)）
            var option = new Dropdown.OptionData(text);
            optionList.Add(option);
        }
        choseItemDropdown.options = optionList;    // 赋值给Dropdown的options集合
        choseItemDropdown.value = 0;               // 设置默认选中第0个选项（初始化默认值）
        choseItemDropdown.RefreshShownValue();     // 强制触发一次值变更事件（避免默认值未触发回调）
        OnDropdownValueChanged(choseItemDropdown.value);   // 再次调用
        
        // 买卖选项菜单
        sellOrBuyDropdown.options = new List<Dropdown.OptionData>
        {
            new("售出"),
            new("进购"),
        };
        OnDropdownValueChanged(sellOrBuyDropdown.value);   // 调用买卖选项
        
        // 开始贸易按钮
        dealButton.onClick.AddListener(() =>
        {
            // 状态检查
            if (tradingPost.inProduction)
            {
                UIManager.instance.helpPanel.Show("贸易中");
                return;
            }
            // 开始经营
            if(isSell)
                tradingPost.SetSellOrder(tradingPost.CurrentBlueprint, dealAmount);
            else
                tradingPost.SetBuyOrder(tradingPost.CurrentBlueprint, dealAmount);
            
            TimeLogic.instance.timeSpeed = 1;
            tradingPost.ChangeProduction();
            tradingPost.objectData.UpdateDataUI();

            ResetData();
            ClosePanel();
        });
    }
    

    private void OnDropdownValueChanged(int index)
    {
        Debug.Log($"选中项变更，当前索引：{index}");
        var selectedText = choseItemDropdown.options[index].text;
        chosenItem = ItemManager.instance.GetItemByName(selectedText);
        if (tradingPost.SetGetCurrentBpByItem(chosenItem) == null)
        {
            UIManager.instance.helpPanel.Show("未开放贸易");
            UIManager.instance.helpPanel.AddCloseAction(() =>
            {
                OnDropdownValueChanged(0);
            });
        }
        // 绘制物品信息
        ResetData();
        itemInfo.text = ItemDealInfo(chosenItem);
        icon.sprite = chosenItem.icon;
        timeText.text = $"{tradingPost.CurrentBlueprint.baseTime:F1}秒";
        InitSlider();       // 变更滑动条对象
    }
    private void OnSellBuyDropdownValueChanged(int index)
    {
        ResetData();
        var selectedText =  sellOrBuyDropdown.options[index].text;
        isSell = selectedText switch
        {
            "售出" => true,
            "进购" => false,
            _ => isSell
        };
        InitSlider();
    }

    private void InitSlider()
    {
        // 清空所有监听事件
        addButton.onClick.RemoveAllListeners();
        subButton.onClick.RemoveAllListeners();
        amountSlider.onValueChanged.RemoveAllListeners();
        // 匹配物品数量
        amountSlider.minValue = 1;
        if (isSell)
        {
            amountSlider.maxValue = chosenItem.count;
        }
        else
        {
            amountSlider.maxValue = ItemManager.instance.GetItemByName("金币").count / chosenItem.price;
        }
        // 添加增减按钮监听事件
        addButton.onClick.AddListener(() =>
        {
            if (amountSlider.value >= amountSlider.maxValue) return;
            amountSlider.value += 1;
        });
        subButton.onClick.AddListener(() =>
        {
            if(amountSlider.value <= amountSlider.minValue) return;
            amountSlider.value -= 1;
        });
        // 添加值变动监听事件
        amountSlider.onValueChanged.AddListener((num) =>
        {
            dealAmount = (int)amountSlider.value;
            amountText.text = $"选择数量:{dealAmount}";
            finalMoney.text = $"{chosenItem.price * dealAmount}$";
        });
    }
    
    private string ItemDealInfo(BaseItem item)
    {
        var info = 
            $"{item.itemName}\n" +
            $"拥有数量: {item.count}\n" +
            $"单价: {item.price}\n" +
            "\n" +
            $"{item.description}";
        return info;
    }

    private void ResetData()
    {
        // 重置数据
        //chosenItem = null;
        dealAmount = 1;
        amountSlider.value = dealAmount;

        if (chosenItem)
        {
            amountText.text = $"选择数量:{dealAmount}";
            finalMoney.text = $"{chosenItem.price * dealAmount}$";
        }
    }
    
}
