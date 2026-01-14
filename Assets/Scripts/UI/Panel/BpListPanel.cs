using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BpListPanel : BasePanel
{
    public GridObjectData data;
    public Building building;
    public List<Blueprint> blueprintList;
    
    [Header("UI控件")]
    public Dropdown bpDropdown;
    public Button productButton;
    public Text bpText;
    public Text timeText;
    public RectTransform useIconRect;
    public RectTransform productIconRect;
    public GameObject iconPrefab;
    public Sprite noneSprite;
    
    protected override void Init()
    {
        base.Init();
        
        if (bpDropdown == null)
            return;
        // 绑定选择变更事件（先移除原有监听，避免重复绑定）
        bpDropdown.onValueChanged.RemoveAllListeners();
        bpDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        productButton.onClick.AddListener(() =>
        {
            building.ChangeProduction();
            UIManager.instance.objectInfoPanel.WriteInfo(data); //物件面板的信息
            //蓝图面板按钮切换
            productButton.GetComponent<Image>().color = building.inProduction ? 
                new Color(1f,.7f,.7f) : new Color(.7f,1f,.7f);
            productButton.GetComponentInChildren<Text>().text = building.inProduction ? "停止制造" : "开始制造";        
        });
    }
    
    // Dropdown选择变更回调方法（参数为当前选中的索引）
    private void OnDropdownValueChanged(int index)
    {
        string selectedText = bpDropdown.options[index].text;
        building.CurrentBlueprint = building.GetBlueprintByProductInfo(selectedText);
        // 绘制蓝图面板内容
        bpText.text = building.CurrentBlueprint.Info();
        timeText.text = $"{building.CurrentBlueprint.baseTime:F1}秒";
        // 清除icon
        ClearIcon(useIconRect);
        ClearIcon(productIconRect);
        // icon绘制
        CreateIcons(building.CurrentBlueprint.productGroup, productIconRect);
        if (building.CurrentBlueprint.useGroup.Count == 0)
        {
            var icon = Instantiate(iconPrefab, useIconRect);
            icon.GetComponent<Image>().sprite = noneSprite;
            icon.GetComponentInChildren<Text>().text = "";
        }
        else
            CreateIcons(building.CurrentBlueprint.useGroup, useIconRect);
        
        // 绘制物体面板内容
        UIManager.instance.objectInfoPanel.WriteInfo(data);
    }

    public void InitBpInfo(GridObjectData d,Building b)
    {
        data = d;
        building = b;
        blueprintList = building.blueprints;
        
        //蓝图面板按钮切换
        productButton.GetComponent<Image>().color = building.inProduction ? 
            new Color(1f,.7f,.7f) : new Color(.7f,1f,.7f);
        productButton.GetComponentInChildren<Text>().text = building.inProduction ? "停止制造" : "开始制造";   
        
        // 菜单更新逻辑
        bpDropdown.ClearOptions();
        List<string> optionTexts = new();
        foreach (var bp in blueprintList)
        {
            if (bp.isLocked)    continue;       // 未解锁则跳过
            optionTexts.Add(bp.ProductInfo(b));
        }
        var optionList = new List<Dropdown.OptionData>();
        foreach (string text in optionTexts)
        {
            // 创建OptionData实例，设置选项文本（还可设置图标：new Dropdown.OptionData(sprite, text)）
            var option = new Dropdown.OptionData(text);
            optionList.Add(option);
        }
        bpDropdown.options = optionList;    // 赋值给Dropdown的options集合
        bpDropdown.value = 0;               // 设置默认选中第0个选项（初始化默认值）
        bpDropdown.RefreshShownValue();     // 强制触发一次值变更事件（避免默认值未触发回调）
        OnDropdownValueChanged(bpDropdown.value);   // 再次调用
    }

    public void CreateIcons(List<ItemGroup> itemGroups, RectTransform rect)
    {
        foreach (var group in itemGroups)
        {
            var icon = Instantiate(iconPrefab, rect);
            icon.GetComponent<Image>().sprite = group.item.icon;
            icon.GetComponentInChildren<Text>().text = $"X{group.count}";
        }
    }
    public void ClearIcon(RectTransform rect)
    {
        for (int i = 0; i < rect.childCount; i++)
        {
            Destroy(rect.GetChild(i).gameObject);
        }
    }
    
}
