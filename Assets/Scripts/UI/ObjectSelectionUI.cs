using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ObjectSelectionUI : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    public Text objectInfoText;
    public Text actionInfoText;
    
    private Dictionary<Button, GridObjectConfig> buttonConfigMap = new();
    private Restaurant restaurant;
    
    void Start()
    {
        // 创建可建造建筑的按钮
        CreateObjectButtons(0);
        
        // 显示操作提示
        if (gridManager != null)
        {
            string hint = "操作提示:\n";
            hint += "左键 - 放置建筑\n";
            hint += "右键 - 取消放置\n";
            hint += "R - 旋转物体\n";
            hint += "\n从左侧选择建筑开始放置";
            
            // 更新UI提示
            actionInfoText.text = hint;
        }
    }
    
    public void CreateObjectButtons(int level)
    {
        if (gridManager == null || gridManager.availableObjects.Count == 0) return;

        for (var i = 0; i < gridManager.availableObjects[level].configs.Count; i++)
        {
            var config = gridManager.availableObjects[level].configs[i];
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            Button button = buttonObj.GetComponent<Button>();

            // 设置按钮文本
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = config.objectName;
            }

            // 添加点击事件
            button.onClick.AddListener(() => OnObjectSelected(config));

            // 存储映射
            buttonConfigMap.Add(button, config);
        }
    }
    
    public void OnObjectSelected(GridObjectConfig config)
    {
        gridManager.StartPlacingObject(config);
        UIManager.instance.objectInfoPanel.ClosePanel();
        
        // 显示物体信息
        if (objectInfoText != null)
        {
            string info = $"{config.objectName}\n";
            info += $"{config.description}\n" + "\n";
            info += $"占用单元格: {config.occupiedCells.Count}\n";
            info += $"可旋转: {config.canRotate}";
            objectInfoText.text = info;
        }
    }
    
    void Update()
    {
        if (!gridManager.isPlacing)
        {
            objectInfoText.text = "";
        }
        
        if (restaurant is null)
        {
            foreach (var b in TimeLogic.instance.buildings)
            {
                if (b is not Restaurant r) continue;
                restaurant = r;
                restaurant.onLevelUpCompleted += () => CreateObjectButtons(restaurant.Level-1);     //升级时解锁建筑
                // 放置后删除餐厅按钮
                var restaurantBtn = buttonContainer.GetChild(0).gameObject;
                buttonConfigMap.Remove(restaurantBtn.GetComponent<Button>());
                Destroy(restaurantBtn);
                break;
            }
        }
    }
}