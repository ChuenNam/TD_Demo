// BlueprintEditorWindow.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class BlueprintEditorWindow : EditorWindow
{
    private GridObjectConfig selectedConfig;
    private Vector2 scrollPosition;
    private Vector2 blueprintScrollPosition;
    private int selectedBlueprintIndex = -1;
    
    // 添加：记录前一个配置，用于检测变化
    private GridObjectConfig previousConfig;
    
    [MenuItem("Tools/蓝图编辑器")]
    public static void ShowWindow()
    {
        var window = GetWindow<BlueprintEditorWindow>("蓝图编辑器");
        window.minSize = new Vector2(680, 600);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        
        // 左侧面板 - 配置选择
        DrawLeftPanel();
        
        // 中间分隔线
        GUILayout.Box("", GUILayout.Width(1), GUILayout.ExpandHeight(true));
        
        // 右侧面板 - 蓝图编辑
        DrawRightPanel();
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(300));
        
        EditorGUILayout.LabelField("配置选择", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // 方法1：使用 BeginChangeCheck/EndChangeCheck
        EditorGUI.BeginChangeCheck();
        selectedConfig = EditorGUILayout.ObjectField("物件配置:", 
            selectedConfig, typeof(GridObjectConfig), false) as GridObjectConfig;
        
        if (EditorGUI.EndChangeCheck())
        {
            OnConfigChanged();
        }
        
        EditorGUILayout.Space(20);
        
        if (selectedConfig != null)
        {
            DrawConfigInfo();
        }
        else
        {
            // 添加拖拽区域
            Rect dropRect = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            // 样式：文字水平 + 垂直居中
            var centeredHelpBoxStyle = new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter };  
            GUI.Box(dropRect, "拖放 GridObjectConfig 到这里", centeredHelpBoxStyle);
            
            HandleDragAndDrop(dropRect);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void HandleDragAndDrop(Rect dropRect)
    {
        Event evt = Event.current;
        
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (dropRect.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GridObjectConfig config)
                        {
                            selectedConfig = config;
                            OnConfigChanged();
                            break;
                        }
                    }
                    
                    evt.Use();
                }
            }
        }
    }
    
    private void OnConfigChanged()
    {
        selectedBlueprintIndex = -1;
        
        if (selectedConfig != null)
        {
            // 刷新数据
            EditorUtility.SetDirty(selectedConfig);
            
            // 如果配置文件没有蓝图配置，询问是否创建
            if (selectedConfig.blueprintConfig == null)
            {
                if (EditorUtility.DisplayDialog("创建蓝图配置", 
                    "该配置没有关联的蓝图配置，是否创建新的蓝图配置？", "创建", "取消"))
                {
                    CreateNewBlueprintConfig();
                }
            }
        }
        
        // 强制重新绘制窗口
        Repaint();
    }
    
    private void DrawConfigInfo()
    {
        EditorGUILayout.LabelField("已选建筑配置:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("名称: " + selectedConfig.objectName);
        
        if (selectedConfig.icon != null)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label(selectedConfig.icon.texture, GUILayout.Width(64), GUILayout.Height(64));
        }
        
        EditorGUILayout.Space(10);
        
        // 蓝图配置部分
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        EditorGUILayout.LabelField("蓝图配置:", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        selectedConfig.blueprintConfig = EditorGUILayout.ObjectField(
            "蓝图文件:", 
            selectedConfig.blueprintConfig, 
            typeof(BlueprintConfig), 
            false) as BlueprintConfig;
        
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(selectedConfig);
            AssetDatabase.SaveAssets();
        }
        
        if (selectedConfig.blueprintConfig == null)
        {
            EditorGUILayout.HelpBox("没有蓝图配置!", MessageType.Warning);
            if (GUILayout.Button("创建新的蓝图配置"))
            {
                CreateNewBlueprintConfig();
            }
        }
        else
        {
            EditorGUILayout.Space(10);
            
            int blueprintCount = selectedConfig.blueprintConfig.blueprints?.Count ?? 0;
            EditorGUILayout.LabelField($"蓝图数量: {blueprintCount}");
            
            if (GUILayout.Button("新建蓝图", GUILayout.Height(30)))
            {
                CreateNewBlueprint();
            }
            
            if (GUILayout.Button("在项目中打开", GUILayout.Height(25)))
            {
                Selection.activeObject = selectedConfig.blueprintConfig;
                EditorGUIUtility.PingObject(selectedConfig.blueprintConfig);
            }
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 保存按钮
        if (GUILayout.Button("保存所有修改", GUILayout.Height(30)))
        {
            SaveAllChanges();
        }
    }
    
    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        
        if (selectedConfig == null)
        {
            EditorGUILayout.HelpBox("请先选择一个建筑配置", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }
        
        if (selectedConfig.blueprintConfig == null)
        {
            EditorGUILayout.HelpBox("该配置没有蓝图配置", MessageType.Warning);
            if (GUILayout.Button("创建蓝图配置"))
            {
                CreateNewBlueprintConfig();
            }
            EditorGUILayout.EndVertical();
            return;
        }
        
        var blueprintConfig = selectedConfig.blueprintConfig;
        
        // 确保蓝图列表不为null
        if (blueprintConfig.blueprints == null)
        {
            blueprintConfig.blueprints = new List<Blueprint>();
            EditorUtility.SetDirty(blueprintConfig);
        }
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"蓝图列表 ({blueprintConfig.blueprints.Count})", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("新建蓝图", GUILayout.Width(80)))
        {
            CreateNewBlueprint();
        }
        
        if (GUILayout.Button("刷新", GUILayout.Width(60)))
        {
            RefreshData();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // 蓝图列表
        blueprintScrollPosition = EditorGUILayout.BeginScrollView(blueprintScrollPosition);
        
        for (int i = 0; i < blueprintConfig.blueprints.Count; i++)
        {
            DrawBlueprintItem(i, blueprintConfig.blueprints[i]);
        }
        
        if (blueprintConfig.blueprints.Count == 0)
        {
            EditorGUILayout.HelpBox("还没有蓝图，点击'新建蓝图'按钮创建一个", MessageType.Info);
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawBlueprintItem(int index, Blueprint blueprint)
    {
        bool isSelected = (selectedBlueprintIndex == index);
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        // 使用自定义的背景色
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = isSelected ? 
            Color.white : 
            Color.cyan;
        
        // 标题行
        EditorGUILayout.BeginHorizontal(GUI.skin.box);
        
        string blueprintName = $"蓝图 {index + 1}";
        if (blueprint.useGroup != null && blueprint.useGroup.Count > 0 && 
            blueprint.productGroup != null && blueprint.productGroup.Count > 0)
        {
            string inputName = GetFirstItemName(blueprint.useGroup);
            string outputName = GetFirstItemName(blueprint.productGroup);
            blueprintName = $"{inputName} → {outputName}";
        }
        
        // 选择切换
        bool newSelected = EditorGUILayout.Foldout(isSelected, blueprintName, true);
        if (newSelected != isSelected)
        {
            selectedBlueprintIndex = newSelected ? index : -1;
            Repaint();
        }
        
        GUILayout.FlexibleSpace();
        
        // 锁定状态
        EditorGUI.BeginChangeCheck();
        blueprint.isLocked = EditorGUILayout.ToggleLeft("锁定", blueprint.isLocked, GUILayout.Width(50));
        if (EditorGUI.EndChangeCheck())
        {
            MarkBlueprintDirty(index);
        }
        
        // 删除按钮
        if (GUILayout.Button("删除", GUILayout.Width(50)))
        {
            if (EditorUtility.DisplayDialog("删除蓝图", 
                $"确定要删除蓝图 {index + 1} 吗？", "删除", "取消"))
            {
                DeleteBlueprint(index);
                return;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 蓝图详情（如果选中）
        if (isSelected)
        {
            EditorGUILayout.Space(10);
            
            // 生产时间
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            
            EditorGUIUtility.labelWidth = 60; // 设置合适的宽度
            blueprint.time = EditorGUILayout.FloatField("生产时间",blueprint.time);
            EditorGUILayout.LabelField("秒", GUILayout.Width(200));
            
            if (EditorGUI.EndChangeCheck())
            {
                MarkBlueprintDirty(index);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(15);
            
            // 原料组
            EditorGUILayout.LabelField("原料:", EditorStyles.boldLabel);
            DrawItemGroupList(blueprint.useGroup, "原料", index, true);
            
            EditorGUILayout.Space(15);
            
            // 产物组
            EditorGUILayout.LabelField("产物:", EditorStyles.boldLabel);
            DrawItemGroupList(blueprint.productGroup, "产物", index, false);
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("保存修改", GUILayout.Height(25)))
            {
                SaveBlueprint(index);
            }
        }
        
        GUI.backgroundColor = originalColor;
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
    }
    
    private void DrawItemGroupList(List<ItemGroup> itemGroups, string label, int blueprintIndex, bool isInput)
    {
        // 确保列表不为null
        if (itemGroups == null)
        {
            itemGroups = new List<ItemGroup>();
            if (isInput)
                selectedConfig.blueprintConfig.blueprints[blueprintIndex].useGroup = itemGroups;
            else
                selectedConfig.blueprintConfig.blueprints[blueprintIndex].productGroup = itemGroups;
        }
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        for (int i = 0; i < itemGroups.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Item选择
            EditorGUI.BeginChangeCheck();
            itemGroups[i].item = EditorGUILayout.ObjectField(
                itemGroups[i].item, typeof(BaseItem), false, GUILayout.Width(200)) as BaseItem;
            
            if (EditorGUI.EndChangeCheck())
            {
                MarkBlueprintDirty(blueprintIndex);
            }
            
            EditorGUILayout.LabelField("X", GUILayout.Width(10));
            
            EditorGUI.BeginChangeCheck();
            itemGroups[i].count = EditorGUILayout.IntField(itemGroups[i].count, GUILayout.Width(60));
            
            if (EditorGUI.EndChangeCheck())
            {
                MarkBlueprintDirty(blueprintIndex);
            }
            
            // 删除按钮
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                itemGroups.RemoveAt(i);
                MarkBlueprintDirty(blueprintIndex);
                break;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 显示物品信息
            if (itemGroups[i].item != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.LabelField(itemGroups[i].item.itemName, GUILayout.Width(150));
                EditorGUILayout.LabelField($"库存: {itemGroups[i].item.count}", GUILayout.Width(80));
                EditorGUILayout.LabelField($"价格: ${itemGroups[i].item.price}", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space(5);
        }
        
        // 添加新物品按钮
        if (GUILayout.Button($"添加{label}", GUILayout.Height(25)))
        {
            itemGroups.Add(new ItemGroup { count = 1 });
            MarkBlueprintDirty(blueprintIndex);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void MarkBlueprintDirty(int index)
    {
        if (selectedConfig != null && selectedConfig.blueprintConfig != null)
        {
            EditorUtility.SetDirty(selectedConfig.blueprintConfig);
        }
    }
    
    private void CreateNewBlueprintConfig()
    {
        if (selectedConfig == null) return;
        
        string defaultName = $"{selectedConfig.objectName}_Blueprints";
        string path = EditorUtility.SaveFilePanelInProject(
            "创建蓝图配置",
            defaultName,
            "asset",
            "保存蓝图配置文件");
        
        if (!string.IsNullOrEmpty(path))
        {
            var newConfig = CreateInstance<BlueprintConfig>();
            newConfig.blueprints = new List<Blueprint>();
            
            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            
            selectedConfig.blueprintConfig = newConfig;
            EditorUtility.SetDirty(selectedConfig);
            AssetDatabase.SaveAssets();
            
            // 创建第一个蓝图
            CreateNewBlueprint();
            
            Repaint();
        }
    }
    
    private void CreateNewBlueprint()
    {
        if (selectedConfig == null || selectedConfig.blueprintConfig == null) return;
        
        var newBlueprint = new Blueprint
        {
            isLocked = false,
            time = 5.0f,
            useGroup = new List<ItemGroup>(),
            productGroup = new List<ItemGroup>()
        };
        
        selectedConfig.blueprintConfig.blueprints.Add(newBlueprint);
        selectedBlueprintIndex = selectedConfig.blueprintConfig.blueprints.Count - 1;
        
        EditorUtility.SetDirty(selectedConfig.blueprintConfig);
        AssetDatabase.SaveAssets();
        
        Repaint();
    }
    
    private void DeleteBlueprint(int index)
    {
        if (selectedConfig == null || selectedConfig.blueprintConfig == null) return;
        
        selectedConfig.blueprintConfig.blueprints.RemoveAt(index);
        
        if (selectedBlueprintIndex >= selectedConfig.blueprintConfig.blueprints.Count)
        {
            selectedBlueprintIndex = selectedConfig.blueprintConfig.blueprints.Count - 1;
        }
        
        EditorUtility.SetDirty(selectedConfig.blueprintConfig);
        AssetDatabase.SaveAssets();
        
        Repaint();
    }
    
    private void SaveBlueprint(int index)
    {
        if (selectedConfig == null || selectedConfig.blueprintConfig == null) return;
        
        EditorUtility.SetDirty(selectedConfig.blueprintConfig);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"蓝图 {index + 1} 保存成功!");
    }
    
    private void SaveAllChanges()
    {
        if (selectedConfig != null)
        {
            EditorUtility.SetDirty(selectedConfig);
            
            if (selectedConfig.blueprintConfig != null)
            {
                EditorUtility.SetDirty(selectedConfig.blueprintConfig);
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log("所有修改已保存!");
        }
    }
    
    private void RefreshData()
    {
        if (selectedConfig != null && selectedConfig.blueprintConfig != null)
        {
            // 重新导入资产
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(selectedConfig.blueprintConfig);
            Repaint();
        }
    }
    
    private string GetFirstItemName(List<ItemGroup> itemGroups)
    {
        if (itemGroups == null || itemGroups.Count == 0 || itemGroups[0].item == null)
            return "空";

        string txt = "";
        for (var i = 0; i < itemGroups.Count; i++)
        {
            var group = itemGroups[i];
            if (group.item == null)
                txt += "空";
            else
                txt += group.count + group.item.itemName;
            if (i == itemGroups.Count-1) break;
            txt += "+";
        }
        return txt;
    }
    
    // 添加：定期检查配置变化
    private void Update()
    {
        // 如果配置在外部被修改，刷新窗口
        if (selectedConfig != previousConfig)
        {
            previousConfig = selectedConfig;
            Repaint();
        }
    }
}
