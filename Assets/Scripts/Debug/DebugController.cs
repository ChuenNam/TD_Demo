using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    [Header("控制台设置")]
    private string input;
    public bool showConsole = false;
    public Color textColor = Color.yellow; // 文本颜色
    [Tooltip("控制台左上角坐标")] public Vector2 consolePos = new Vector2(10, 10);
    [Tooltip("控制台宽度")] public float consoleWidth = 600f;
    [Tooltip("显示框高度")] public float displayBoxHeight = 300f;
    [Tooltip("输入框高度")] public float inputBoxHeight = 30f;

    private List<object> commandList = new();
    
    [Header("Debug语句")]
    public static DebugCommand HELP;
    public static DebugCommand CLEAR_LOG;
    public static DebugCommand<string, int> SET_ITEM_COUNT;
    public static DebugCommand<string, int> SET_ITEM_PRICE;
    public static DebugCommand<int> SET_MONEY;
    public static DebugCommand<int> SET_ALL_ITEM_COUNT;
    public static DebugCommand CLEAR_ALL_ITEM_COUNT;
    public static DebugCommand<int> SET_TIME_SPEED;
    public static DebugCommand SET_DAY;
    public static DebugCommand SET_NIGHT;

    // 核心显示相关变量
    private Vector2 displayScrollPos;
    private List<string> displayContentList = new();
    private bool isHelpContentAdded = false;

    private void Awake()
    {
        #region 添加控制台语句

            HELP = new DebugCommand("HELP", "查找所有指令", "HELP", AddHelpContentToDisplayList);
        
            CLEAR_LOG = new DebugCommand("CLEAR_LOG", "清除所有记录", "CLEAR_LOG",
            () => displayContentList.Clear());
            
            SET_ITEM_PRICE = new DebugCommand<string, int>("SET_ITEM_PRICE", "设置物品价格", "SET_ITEM_PRICE <NAME> <PRICE>",
            (str, newPrice) =>
            {
                if (str == "金币")
                {
                    AddContentToDisplayList($"错误：金币无法设置价格");
                    return;
                }
                var item = ItemManager.instance?.GetItemByName(str);
                if (item is null) return;
                item.price = newPrice;
                AddContentToDisplayList($"执行结果：{str}价格已设置为 {newPrice}");
            });
            SET_ITEM_COUNT = new DebugCommand<string, int>("SET_ITEM_COUNT", "设置物品数量", "SET_ITEM_COUNT <NAME> <COUNT>",
            (str, newCount) =>
            {
                var item = ItemManager.instance?.GetItemByName(str);
                if (item is null) return;
                item.count = newCount;
                AddContentToDisplayList($"执行结果：{str}数量已设置为 {newCount}");
            });
            SET_MONEY = new DebugCommand<int>("SET_MONEY", "设置金币数量", "SET_MONEY <MONEY>", 
                (x) =>
            {
                var money = ItemManager.instance?.GetItemByName("金币");
                if (money is null) return;
                money.count = x;
                AddContentToDisplayList($"执行结果：金币已设置为 {x}");
            });
            SET_ALL_ITEM_COUNT = new DebugCommand<int>("SET_ALL_ITEM", "设置所有物品数量", "SET_ALL_ITEM <COUNT>", 
            (x) =>
            {
                if (ItemManager.instance?.possesItem == null) return;
                foreach (var item in ItemManager.instance.possesItem)
                {
                    item.count = x;
                }
                AddContentToDisplayList($"执行结果：所有物品数量已设置为 {x}");
            });
            CLEAR_ALL_ITEM_COUNT = new DebugCommand("CLEAR_ALL_ITEM", "清空所有物品数量", "CLEAR_ALL_ITEM", 
            () =>
            {
                if (ItemManager.instance?.possesItem == null) return;
                foreach (var item in ItemManager.instance.possesItem)
                {
                    item.count = 0;
                }
                AddContentToDisplayList("执行结果：所有物品数量已清空");
            });
            SET_TIME_SPEED = new DebugCommand<int>("SET_TIME_SPEED", "设置时间倍速", "SET_TIME_SPEED <SPEED>", 
            (x) =>
            {
                if (Logic.instance?.timeSpeed == null) return;
                Logic.instance.timeSpeed = x;
                AddContentToDisplayList($"执行结果：时间流速已设置为{x}");
            });
            SET_DAY = new DebugCommand("SET_DAY", "设置时间为白天", "SET_DAY", 
            () =>
            {
                if (Logic.instance?.timeSpeed == null) return;
                var ins = Logic.instance;
                ins.dayTime = 0;
                ins.globalTime = (ins.day-1) * ins.timeConfig.secondsPerDay;            // 需要同步重置总时间
                AddContentToDisplayList("执行结果：时间已设置为白天开始");
            });
            SET_NIGHT = new DebugCommand("SET_NIGHT", "设置时间为夜晚", "SET_NIGHT", 
            () =>
            {
                if (Logic.instance?.timeSpeed == null) return;
                var ins = Logic.instance;
                ins.dayTime = ins.timeConfig.nightChangePoint * ins.timeConfig.secondsPerDay;
                ins.globalTime = (ins.day-1) * ins.timeConfig.secondsPerDay + ins.dayTime;  // 需要同步重置总时间
                AddContentToDisplayList("执行结果：时间已设置为夜晚开始");
            });
            
            commandList = new List<object>()
            {
                HELP,
                CLEAR_LOG,
                SET_ITEM_COUNT,
                SET_ITEM_PRICE,
                SET_MONEY,
                CLEAR_ALL_ITEM_COUNT,
                SET_ALL_ITEM_COUNT,
                SET_TIME_SPEED,
                SET_DAY,
                SET_NIGHT,
            };
        #endregion
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote)) showConsole = !showConsole;
        if (Input.GetKeyDown(KeyCode.Return) && showConsole) HandleInput();
    }
    
    private void OnGUI()
    {
        if (!showConsole) return;

        // 计算控制台各区域Rect（基于自定义坐标和尺寸，避免挤出屏幕）
        Rect displayBoxRect = new Rect(consolePos.x, consolePos.y, consoleWidth, displayBoxHeight);
        Rect inputBoxRect = new Rect(consolePos.x, consolePos.y + displayBoxHeight + 5f, consoleWidth, inputBoxHeight);

        // ========== 1. 绘制显示框（Help内容+输入记录） ==========
        GUI.backgroundColor = Color.gray;
        GUI.Box(displayBoxRect, "控制台信息", new GUIStyle(GUI.skin.textField)
        {
            fontSize = 20,
            normal = { textColor = Color.white },
            alignment = TextAnchor.UpperCenter,
        });

        // 滚动视图内容区域
        Rect viewportRect = new Rect(0, 0, displayBoxRect.width - 20f, (inputBoxHeight) * displayContentList.Count);
        displayScrollPos = GUI.BeginScrollView(
            new Rect(displayBoxRect.x + 5f, displayBoxRect.y + 25f, displayBoxRect.width - 10f, displayBoxRect.height - 30f),
            displayScrollPos,
            viewportRect
        );

        // 绘制显示内容
        for (int i = 0; i < displayContentList.Count; i++)
        {
            Rect textRect = new Rect(5f, i * (inputBoxHeight - 5f), viewportRect.width - 10f, inputBoxHeight - 5f);
            GUI.Label(textRect, displayContentList[i], new GUIStyle()
            {
                fontSize = 20,
                normal = { textColor = textColor },
                alignment = TextAnchor.MiddleLeft
            });
        }
        GUI.EndScrollView();

        // ========== 2. 绘制输入框（确保可见） ==========
        GUI.backgroundColor = Color.gray;
        GUI.Box(inputBoxRect, "输入指令", new GUIStyle(GUI.skin.textField)
        {
            fontSize = 20,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter,
        });

        // 输入框文本样式
        GUIStyle inputStyle = new GUIStyle(GUI.skin.textField)
        {
            fontSize = 20,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(10, 0, 0, 0) // 文本左内边距
        };

        // 绘制输入框并获取输入内容
        input = GUI.TextField(
            new Rect(inputBoxRect.x + 10f, inputBoxRect.y + 5f, inputBoxRect.width - 20f, inputBoxRect.height - 10f),
            input,
            inputStyle
        );
    }

    private void HandleInput()
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        AddContentToDisplayList($"用户输入：{input}");

        string[] properties = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (properties.Length == 0)
        {
            input = "";
            return;
        }

        bool commandFound = false;
        string commandID = properties[0].ToUpper();

        foreach (var cmdObj in commandList)
        {
            if (cmdObj is not DebugCommandBase commandBase) continue;
            
            if (commandBase.CommandID.Equals(commandID, StringComparison.OrdinalIgnoreCase))
            {
                commandFound = true;
                try
                {
                    switch (cmdObj)
                    {
                        case DebugCommand cmd:
                            cmd.Invoke();
                            break;
                        
                        case DebugCommand<int> cmdInt 
                            when properties.Length >= 2 && int.TryParse(properties[1], out int param):
                                cmdInt.Invoke(param);
                            break;
                        
                        case DebugCommand<string, int> cmdStringInt 
                            when properties.Length >= 2 && int.TryParse(properties[2], out int param):
                                cmdStringInt.Invoke(properties[1],param);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AddContentToDisplayList($"执行异常：{ex.Message}");
                }
                break;
            }
        }

        if (!commandFound)
        {
            AddContentToDisplayList($"错误：未找到指令 {commandID}，输入HELP查看所有指令");
        }

        input = "";
    }

    private void AddContentToDisplayList(string content)
    {
        displayContentList.Add(content);
        if (displayContentList.Count > 100) displayContentList.RemoveAt(0);
    }

    private void AddHelpContentToDisplayList()
    {
        if (isHelpContentAdded) return;

        AddContentToDisplayList("=============== 可用指令列表 ===============");
        foreach (var cmdObj in commandList)
        {
            if (cmdObj is DebugCommandBase cmd)
            {
                AddContentToDisplayList($"{cmd.CommandFormat} - {cmd.CommandDescription}");
            }
        }
        AddContentToDisplayList("===========================================");
        isHelpContentAdded = true;
    }
}
