using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectInfoPanel : BasePanel
{
    [SerializeField]private GridManager gridManager;
    public GridObjectData data;
    
    public Text nameInfo;
    public Text descriptionText;
    public Text productInfoText;
    public Text buffInfoText;
    public Button lvlUpButton;
    public Image lvlUpButtonImage;
    public Text lvlUpButtonText;
    public Button chooseBPButton;
    
    [Header("面板类别")]
    public BpListPanel blueprintListPanel;
    
    protected override void Init()
    {
        base.Init();
        closeButton.onClick.AddListener(()=> gridManager.currentObject = null);
        lvlUpButton.onClick.AddListener(() =>
        {
            var building = data.instance.GetComponent<Building>();
            if (building is ILevelUp lb)
            {
                UIManager.instance.restaurantPanel.ClosePanel();
                UIManager.instance.bpListPanel.ClosePanel();
                UIManager.instance.tradePanel.ClosePanel();
                lb.LevelUp(building);
                lvlUpButtonImage.color = lb.Level == lb.LimitLevel ? Color.gray : Color.green;
                nameInfo.text = $"{data.name} Lv.{lb.Level}";

                // 根据等级状况显示信息
                if (lb.Level == lb.LimitLevel)
                {
                    lvlUpButtonText.text = lb.Level == lb.MaxLevel ? "已满级" : "升级餐厅提升等级上限";
                    return;
                }
                lvlUpButtonText.text = $"升级:{lb.GetCostInfo()}";
            }
            else
            {
                UIManager.instance.helpPanel.Show("建筑不可升级");
            }
        });
        chooseBPButton.onClick.AddListener(() =>
        {
            TimeLogic.instance.timeSpeed = 0;
            var building = data.instance.GetComponent<Building>();
            switch (building)
            {
                case Restaurant:
                {
                    var restaurantPanel = UIManager.instance.restaurantPanel;
                    if (!restaurantPanel.isActive)
                    {
                        restaurantPanel.ShowPanel();
                        restaurantPanel.InitRestaurantPanel();
                    }
                    else
                    {
                        //TimeLogic.instance.timeSpeed = 1;
                        restaurantPanel.ClosePanel();
                    }
                    break;
                }
                case TradingPost:
                {
                    var tradePanel = UIManager.instance.tradePanel;
                    if (!tradePanel.isActive)
                    {
                        tradePanel.ShowPanel();
                        tradePanel.InitTradePanel(data.instance.GetComponent<TradingPost>());
                    }
                    else
                    {
                        //TimeLogic.instance.timeSpeed = 1;
                        tradePanel.ClosePanel();
                    }
                    break;
                }
                default:
                {
                    if (!blueprintListPanel.isActive)
                    {
                        blueprintListPanel.ShowPanel();
                        blueprintListPanel.InitBpInfo(data,data.instance.GetComponent<Building>());
                    }
                    else
                    {
                        //TimeLogic.instance.timeSpeed = 1;
                        blueprintListPanel.ClosePanel();
                        blueprintListPanel.building = null;
                    }
                    break;
                }
            }
        });
    }

    public void WriteInfo(GridObjectData objectData)
    {
        this.data = objectData;
        nameInfo.text = objectData.name;
        descriptionText.text = data.description;
        
        var building = data.instance.GetComponent<Building>();
        WriteBuffInfo(building);        // 更新buff信息
        productInfoText.text = building.inProduction ? "进行中：\n" + building.CurrentBlueprint.Info(building): "休息中";

        if (building is ILevelUp lb)
        {
            lvlUpButtonImage.color = lb.Level == lb.LimitLevel ? Color.gray : Color.green;
            nameInfo.text += $" Lv.{lb.Level}";

            // 根据等级状况显示信息
            if (lb.Level == lb.LimitLevel)
            {
                lvlUpButtonText.text = lb.Level == lb.MaxLevel ? "已满级" : "升级餐厅提升等级上限";
                return;
            }
            lvlUpButtonText.text = $"升级:{lb.GetCostInfo()}";
        }
        else
        {
            lvlUpButtonImage.color = Color.gray;
            lvlUpButtonText.text = "无法升级";
        }
    }

    public void WriteBuffInfo(Building building)
    {
        if (!isActive) return;

        var info = "";
        foreach (var buff in building.buffList)
        {
            if (buff.buffName is "" or "L")
                continue;
            var time = Mathf.Approximately(buff.remainDuration, -1) ? "永久" : $"{buff.remainDuration:F1}";
            info += $"{buff.buffName}:  {time}\n";
        }
        buffInfoText.text = info;
    }
}
