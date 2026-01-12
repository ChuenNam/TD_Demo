using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RestaurantPanel : BasePanel
{
    [Header("UI控件")]
    public RectTransform menuRect;
    public RectTransform todayMenuRect;
    public RectTransform totalInfoRect;
    public Button businessButton;
    public Text totalMoneyText;
    public Text totalTimeText;
    
    public GameObject iconPrefab;
    public GameObject menuDishPrefab;
    public GameObject todayDishPrefab;

    [Header("数据信息")] 
    public bool allowProduction;
    public Restaurant restaurant;
    
    protected override void Init()
    {
        base.Init();
        closeButton.onClick.AddListener(() => TimeLogic.instance.timeSpeed = 1);
        
        businessButton.onClick.AddListener(() =>
        {
            // 屋物资检查
            if (!allowProduction)
            {
                UIManager.instance.helpPanel.ShowPanel();
                UIManager.instance.helpPanel.Write("缺少食材");
                return;
            }
            // 开始经营
            if (restaurant.todayMenu.Count > 0)
            {
                TimeLogic.instance.timeSpeed = 1;
                //restaurant.inProduction = true;
                //restaurant.CurrentBlueprint = restaurant.todayMenu[0];
                restaurant.SetDishToQueue();
                restaurant.ChangeProduction();
            }
            ClosePanel();
        });
    }
    
    public void InitRestaurantPanel()
    {
        // 清空原有信息
        for (var i = 0; i < menuRect.childCount; i++)
            Destroy(menuRect.GetChild(i).gameObject);
        // 填写菜单信息
        var menuDishList = new List<GameObject>();
        foreach (var bp in restaurant.blueprints)
        {
            if (bp.isLocked)
                continue;
            
            var panel = Instantiate(menuDishPrefab, menuRect);
            menuDishList.Add(panel);
            InitMenuDishPanel(panel, bp);
        }
    }

    private void InitMenuDishPanel(GameObject menuDish, Blueprint bp)
    {
        // 填写信息
        menuDish.transform.GetChild(0).GetComponent<Image>().sprite = bp.icon;
        menuDish.transform.GetChild(1).GetComponent<Text>().text = bp.blueprintName;
        menuDish.transform.GetChild(2).GetComponent<Text>().text = $"食材:{bp.GetItemGroupInfo(bp.useGroup)}";
        menuDish.transform.GetChild(3).GetComponent<Text>().text = $"售价:${bp.productGroup[0].count}";
        // 绑定选择按钮
        menuDish.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (restaurant.todayMenu.Count == 5)
                return;
            restaurant.todayMenu.Add(bp);
            InitTodayDishPanel(bp);
            totalMoneyText.text = $"${CalculateTotalMoney()}";
            allowProduction = CalculateTotalInfo();
        });
    }

    private void InitTodayDishPanel(Blueprint bp)
    {
        var ui = Instantiate(todayDishPrefab, todayMenuRect);
        // 填写信息
        ui.transform.GetChild(0).GetComponent<Text>().text = bp.blueprintName;
        ui.transform.GetChild(1).GetComponent<Image>().sprite = bp.icon;
        ui.transform.GetChild(2).GetComponent<Text>().text = $"${bp.productGroup[0].count}";
        // 绑定按钮
        ui.GetComponent<Button>().onClick.AddListener(() =>
        {
            restaurant.todayMenu.Remove(bp);
            Destroy(ui);
            totalMoneyText.text = $"${CalculateTotalMoney()}";
            allowProduction = CalculateTotalInfo();
        });
    }

    private int CalculateTotalMoney() => restaurant.todayMenu.Sum(bp => bp.productGroup[0].count);

    private bool CalculateTotalInfo()
    {
        // 清空原有信息
        for (var i = 0; i < totalInfoRect.childCount; i++)
            Destroy(totalInfoRect.GetChild(i).gameObject);
        // 获取数据
        var finalItemCountDict = new Dictionary<BaseItem, int>();
        float totalTime = 0;
        foreach (Blueprint bp in restaurant.todayMenu)
        {
            totalTime += bp.baseTime;
            foreach (ItemGroup group in bp.useGroup)
            {
                if (finalItemCountDict.ContainsKey(group.item))
                    finalItemCountDict[group.item] += group.count;
                else
                    finalItemCountDict.Add(group.item, group.count);
            }
        }

        // 更新显示
        var enough = true;
        foreach (var kv in finalItemCountDict)
        {
            totalTimeText.text = $"时间:{totalTime}s";
            var icon = Instantiate(iconPrefab, totalInfoRect);
            icon.transform.localScale = Vector3.one * .5f;
            icon.GetComponent<Image>().sprite = kv.Key.icon;
            icon.GetComponentInChildren<Text>().text = $"X{kv.Value}";
            if (kv.Key.count < kv.Value)
            {
                icon.GetComponentInChildren<Text>().color = Color.red;
                enough = false;
            }
            else
                icon.GetComponentInChildren<Text>().color = Color.black;
        }
        return enough;
    }
}
