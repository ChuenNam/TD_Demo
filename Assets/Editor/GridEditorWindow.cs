// GridObjectEditorWindow.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public enum EditMode
{
    Add,       // 添加单元格
    Remove,    // 删除单元格
    SetPivot,  // 设置枢轴点
    Move,      // 移动视图
    Rectangle, // 矩形绘制
    Fill       // 填充绘制
}

public class GridObjectEditorWindow : EditorWindow
{
    private GridObjectConfig currentConfig;
    private SerializedObject serializedConfig;
    
    // 网格设置
    private Vector2 gridScrollPosition = Vector2.zero;
    private float gridZoom = 20f;
    private Vector2Int gridDisplaySize = new Vector2Int(30, 30);
    private bool showGridCoordinates = true;
    private bool snapToGrid = true;
    
    // 编辑设置
    private EditMode currentEditMode = EditMode.Add;
    private Vector2Int currentBrushSize = Vector2Int.one;
    private bool isDragging = false;
    private Vector2Int dragStartCell;
    private List<Vector2Int> dragPreviewCells = new List<Vector2Int>();
    
    // 引用GridManager（可选）
    private GridManager gridManagerRef;
    
    // 样式
    private GUIStyle cellStyle;
    private GUIStyle pivotStyle;
    private GUIStyle gridLineStyle;
    
    [MenuItem("Tools/Grid Object Editor")]
    public static void ShowWindow()
    {
        GetWindow<GridObjectEditorWindow>("网格物体编辑器");
    }
    
    private void OnEnable()
    {
        // 查找场景中的GridManager
        FindGridManager();
        
        // 初始化样式
        //InitializeStyles();
        
        // 监听撤销操作
        Undo.undoRedoPerformed += Repaint;
        
        // 监听资产变化
        EditorApplication.projectChanged += Repaint;
    }
    
    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Repaint;
        EditorApplication.projectChanged -= Repaint;
    }
    
    private void FindGridManager()
    {
        GridManager manager = FindObjectOfType<GridManager>();
        if (manager != null)
        {
            gridManagerRef = manager;
        }
    }
    
    private void InitializeStyles()
    {
        cellStyle = new GUIStyle(GUI.skin.box)
        {
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(0, 0, 0, 0),
            border = new RectOffset(0, 0, 0, 0)
        };
        
        pivotStyle = new GUIStyle(GUI.skin.box)
        {
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(0, 0, 0, 0),
            border = new RectOffset(0, 0, 0, 0)
        };
    }
    
    private void OnGUI()
    {
        InitializeStyles();
        
        // 顶部工具栏
        DrawToolbar();
        
        // 主区域
        EditorGUILayout.BeginHorizontal();
        
        // 左侧面板（属性编辑）
        DrawLeftPanel();
        
        // 分隔线
        GUILayout.Box("", GUILayout.Width(1), GUILayout.ExpandHeight(true));
        
        // 右侧面板（网格绘制）
        DrawRightPanel();
        
        EditorGUILayout.EndHorizontal();
        
        // 底部状态栏
        DrawStatusBar();
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("新建", EditorStyles.toolbarButton))
        {
            CreateNewConfig();
        }
        
        if (GUILayout.Button("打开", EditorStyles.toolbarButton))
        {
            OpenConfig();
        }
        
        if (currentConfig != null)
        {
            if (GUILayout.Button("保存", EditorStyles.toolbarButton))
            {
                SaveConfig();
            }
            
            if (GUILayout.Button("另存为", EditorStyles.toolbarButton))
            {
                SaveConfigAs();
            }
        }
        
        GUILayout.FlexibleSpace();
        
        // 显示当前编辑的对象
        if (currentConfig != null)
        {
            EditorGUILayout.LabelField($"正在编辑: {currentConfig.objectName}", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        
        EditorGUILayout.Space(5);
        
        // 配置文件选择
        DrawConfigSelection();
        
        if (currentConfig != null)
        {
            EditorGUILayout.Space(10);
            
            // 基础属性
            DrawBasicProperties();
            
            EditorGUILayout.Space(10);
            
            // 网格属性
            DrawGridProperties();
            
            EditorGUILayout.Space(10);
            
            // 编辑工具
            DrawEditTools();
            
            EditorGUILayout.Space(10);
            
            // 形状操作
            DrawShapeOperations();
            
            EditorGUILayout.Space(10);
            
            // 统计信息
            DrawStatistics();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawConfigSelection()
    {
        EditorGUILayout.LabelField("配置文件", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        currentConfig = (GridObjectConfig)EditorGUILayout.ObjectField(
            "配置", currentConfig, typeof(GridObjectConfig), false);
        
        if (EditorGUI.EndChangeCheck() && currentConfig != null)
        {
            serializedConfig = new SerializedObject(currentConfig);
        }
    }
    
    private void DrawBasicProperties()
    {
        EditorGUILayout.LabelField("基础属性", EditorStyles.boldLabel);
        
        if (serializedConfig != null)
        {
            serializedConfig.Update();
            
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("objectName"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("prefab"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("description"));
            
            serializedConfig.ApplyModifiedProperties();
        }
    }
    
    private void DrawGridProperties()
    {
        EditorGUILayout.LabelField("网格属性", EditorStyles.boldLabel);
        
        if (serializedConfig != null)
        {
            serializedConfig.Update();
            
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("pivotOffset"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("canRotate"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("rotationSnap"));
            
            // 编辑器颜色
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("editorColor"), 
                new GUIContent("占用颜色"));
            EditorGUILayout.PropertyField(serializedConfig.FindProperty("pivotColor"), 
                new GUIContent("枢轴颜色"));
            
            serializedConfig.ApplyModifiedProperties();
        }
    }
    
    private void DrawEditTools()
    {
        EditorGUILayout.LabelField("编辑工具", EditorStyles.boldLabel);
        
        // 编辑模式选择
        EditorGUILayout.BeginHorizontal();
        
        GUIContent[] modeContents = {
            new GUIContent("添加", "添加占用单元格(A)"),
            new GUIContent("删除", "删除占用单元格(R)"),
            new GUIContent("枢轴", "设置枢轴点(P)"),
            new GUIContent("移动", "移动视图(M)"),
            new GUIContent("矩形", "矩形绘制"),
            new GUIContent("填充", "填充绘制")
        };
        
        for (int i = 0; i < modeContents.Length; i++)
        {
            if (GUILayout.Toggle((int)currentEditMode == i, modeContents[i], "Button"))
            {
                currentEditMode = (EditMode)i;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 笔刷大小
        if (currentEditMode == EditMode.Add || currentEditMode == EditMode.Remove || 
            currentEditMode == EditMode.Rectangle)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("笔刷设置", EditorStyles.boldLabel);
            
            // 笔刷大小控制
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("尺寸", GUILayout.Width(40));
            
            // 宽度输入
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("宽度", EditorStyles.miniLabel, GUILayout.Width(40));
            currentBrushSize.x = EditorGUILayout.IntField(currentBrushSize.x, GUILayout.Width(50));
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // 高度输入
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("高度", EditorStyles.miniLabel, GUILayout.Width(40));
            currentBrushSize.y = EditorGUILayout.IntField(currentBrushSize.y, GUILayout.Width(50));
            EditorGUILayout.EndVertical();
            
            // 快速设置按钮
            EditorGUILayout.BeginVertical(GUILayout.Width(60));
            EditorGUILayout.LabelField("预设", EditorStyles.miniLabel, GUILayout.Width(40));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("1×1", EditorStyles.miniButton, GUILayout.Width(30)))
            {
                currentBrushSize = Vector2Int.one;
            }
            if (GUILayout.Button("3×3", EditorStyles.miniButton, GUILayout.Width(30)))
            {
                currentBrushSize = new Vector2Int(3, 3);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            // 确保最小值
            currentBrushSize = new Vector2Int(
                Mathf.Max(1, currentBrushSize.x),
                Mathf.Max(1, currentBrushSize.y)
            );
            
            // 显示当前笔刷预览
            if (currentBrushSize.x > 1 || currentBrushSize.y > 1)
            {
                EditorGUILayout.BeginVertical("Box", GUILayout.Height(60));
                EditorGUILayout.LabelField($"笔刷预览: {currentBrushSize.x} × {currentBrushSize.y}", 
                    EditorStyles.miniLabel);
                
                // 绘制简单的笔刷预览
                Rect previewRect = GUILayoutUtility.GetRect(50, 50);
                DrawBrushPreview(previewRect);
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        // 网格设置
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("网格:", GUILayout.Width(40));
        snapToGrid = EditorGUILayout.Toggle(snapToGrid, GUILayout.Width(20));
        EditorGUILayout.LabelField("对齐", GUILayout.Width(30));
        showGridCoordinates = EditorGUILayout.Toggle(showGridCoordinates, GUILayout.Width(20));
        EditorGUILayout.LabelField("坐标", GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();
    }
    
    //笔刷辅助函数
    private void DrawBrushPreview(Rect rect)
    {
        if (Event.current.type != EventType.Repaint) return;
    
        // 绘制背景
        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
    
        // 计算网格大小
        float cellSize = Mathf.Min(rect.width / currentBrushSize.x, rect.height / currentBrushSize.y);
        float totalWidth = cellSize * currentBrushSize.x;
        float totalHeight = cellSize * currentBrushSize.y;
    
        // 居中绘制
        float startX = rect.x + (rect.width - totalWidth) * 0.5f;
        float startY = rect.y + (rect.height - totalHeight) * 0.5f;
    
        // 绘制网格线
        Handles.BeginGUI();
        Handles.color = new Color(1f, 1f, 1f);
    
        // 垂直线
        for (int x = 0; x <= currentBrushSize.x; x++)
        {
            float posX = startX + x * cellSize;
            Handles.DrawLine(new Vector3(posX, startY), new Vector3(posX, startY + totalHeight));
        }
    
        // 水平线
        for (int y = 0; y <= currentBrushSize.y; y++)
        {
            float posY = startY + y * cellSize;
            Handles.DrawLine(new Vector3(startX, posY), new Vector3(startX + totalWidth, posY));
        }
    
        // 绘制填充的中心区域（如果笔刷尺寸是奇数）
        if (currentBrushSize.x % 2 == 1 && currentBrushSize.y % 2 == 1)
        {
            int centerX = currentBrushSize.x / 2;
            int centerY = currentBrushSize.y / 2;
            float cellX = startX + centerX * cellSize;
            float cellY = startY + centerY * cellSize;
        
            Rect centerCellRect = new Rect(cellX + 1, cellY + 1, cellSize - 2, cellSize - 2);
            EditorGUI.DrawRect(centerCellRect, new Color(1f, 0.5f, 0.5f, 0.3f));
        }
    
        Handles.EndGUI();
    
        // 绘制外框
        GUI.Box(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), "");
    }
    
    private void DrawShapeOperations()
    {
        EditorGUILayout.LabelField("形状操作", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("清空"))
        {
            if (EditorUtility.DisplayDialog("清空形状", "确定要清空所有单元格吗？", "确定", "取消"))
            {
                Undo.RecordObject(currentConfig, "清空形状");
                currentConfig.ClearCells();
                EditorUtility.SetDirty(currentConfig);
            }
        }
        
        if (GUILayout.Button("居中"))
        {
            Undo.RecordObject(currentConfig, "居中形状");
            CenterShape();
            EditorUtility.SetDirty(currentConfig);
        }
        
        if (GUILayout.Button("水平镜像"))
        {
            Undo.RecordObject(currentConfig, "水平镜像");
            MirrorShape(true);
            EditorUtility.SetDirty(currentConfig);
        }
        
        if (GUILayout.Button("垂直镜像"))
        {
            Undo.RecordObject(currentConfig, "垂直镜像");
            MirrorShape(false);
            EditorUtility.SetDirty(currentConfig);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("旋转90°"))
        {
            Undo.RecordObject(currentConfig, "旋转90度");
            RotateShape(90);
            EditorUtility.SetDirty(currentConfig);
        }
        
        if (GUILayout.Button("旋转-90°"))
        {
            Undo.RecordObject(currentConfig, "旋转-90度");
            RotateShape(-90);
            EditorUtility.SetDirty(currentConfig);
        }
        
        /*if (GUILayout.Button("导入Prefab"))
        {
            ImportFromPrefab();
        }*/
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawStatistics()
    {
        EditorGUILayout.LabelField("统计信息", EditorStyles.boldLabel);
        
        if (currentConfig != null)
        {
            EditorGUILayout.LabelField($"占用单元格: {currentConfig.occupiedCells.Count}");
            
            if (currentConfig.occupiedCells.Count > 0)
            {
                var bounds = currentConfig.GetBounds();
                EditorGUILayout.LabelField($"尺寸: {bounds.size.x} × {bounds.size.y}");
                EditorGUILayout.LabelField($"中心: ({bounds.center.x}, {bounds.center.y})");
            }
        }
    }
    
    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        
        // 网格控制栏
        DrawGridControls();
        
        // 网格绘制区域
        DrawGridArea();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawGridControls()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("重置视图", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            gridScrollPosition = Vector2.zero;
            gridZoom = 20f;
        }
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.LabelField("缩放:", GUILayout.Width(30));
        gridZoom = EditorGUILayout.Slider(gridZoom, 10f, 50f, GUILayout.Width(100));
        
        EditorGUILayout.LabelField("网格尺寸:", GUILayout.Width(50));
        gridDisplaySize.x = EditorGUILayout.IntField(gridDisplaySize.x, GUILayout.Width(40));
        gridDisplaySize.y = EditorGUILayout.IntField(gridDisplaySize.y, GUILayout.Width(40));
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawGridArea()
    {
        // 获取绘制区域
        Rect gridRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    
        // 处理网格事件
        HandleGridEvents(gridRect);
    
        // 绘制背景
        EditorGUI.DrawRect(gridRect, new Color(0.15f, 0.15f, 0.15f));
    
        // 计算网格参数
        Vector2 gridCenter = gridRect.center;
    
        // 绘制网格线
        DrawGridLines(gridRect, gridCenter);
    
        // 绘制原点标记
        DrawOrigin(gridRect, gridCenter);
    
        // 绘制占用的单元格
        DrawOccupiedCells(gridRect, gridCenter);
    
        // 绘制枢轴点
        DrawPivotPoint(gridRect, gridCenter);
    
        // 绘制拖拽预览
        DrawDragPreview(gridRect, gridCenter);
    
        // 绘制网格坐标
        if (showGridCoordinates)
        {
            DrawGridCoordinates(gridRect, gridCenter);
        }
    }
    
    private void HandleGridEvents(Rect gridRect)
    {
        Event e = Event.current;
        Vector2Int mouseGridPos = GetGridPosition(e.mousePosition, gridRect);
        
        switch (e.type)
        {
            case EventType.MouseDown:
                if (gridRect.Contains(e.mousePosition))
                {
                    HandleMouseDown(mouseGridPos, e);
                    e.Use();
                }
                break;
                
            case EventType.MouseDrag:
                if (gridRect.Contains(e.mousePosition))
                {
                    HandleMouseDrag(mouseGridPos, e);
                    e.Use();
                }
                break;
                
            case EventType.MouseUp:
                HandleMouseUp(e);
                e.Use();
                break;
                
            case EventType.ScrollWheel:
                if (gridRect.Contains(e.mousePosition))
                {
                    HandleScrollWheel(e);
                    e.Use();
                }
                break;
                
            case EventType.KeyDown:
                HandleKeyDown(e);
                break;
        }
        
        // 显示鼠标位置的坐标
        if (gridRect.Contains(e.mousePosition))
        {
            ShowMouseCoordinates(e.mousePosition, mouseGridPos);
        }
    }
    
    private void HandleMouseDown(Vector2Int gridPos, Event e)
    {
        dragStartCell = gridPos;
        
        switch (currentEditMode)
        {
            case EditMode.Add:
                if (e.button == 0) // 左键
                {
                    AddCellsAt(gridPos);
                }
                break;
                
            case EditMode.Remove:
                if (e.button == 0) // 左键
                {
                    RemoveCellsAt(gridPos);
                }
                break;
                
            case EditMode.SetPivot:
                if (e.button == 0) // 左键
                {
                    SetPivotAt(gridPos);
                }
                break;
                
            case EditMode.Move:
                if (e.button == 2) // 中键
                {
                    isDragging = true;
                }
                break;
                
            case EditMode.Rectangle:
            case EditMode.Fill:
                if (e.button == 0) // 左键
                {
                    isDragging = true;
                    UpdateDragPreview(gridPos);
                }
                break;
        }
    }
    
    private void HandleMouseDrag(Vector2Int gridPos, Event e)
    {
        switch (currentEditMode)
        {
            case EditMode.Add:
                if (e.button == 0)
                {
                    AddCellsAt(gridPos);
                }
                break;
                
            case EditMode.Remove:
                if (e.button == 0)
                {
                    RemoveCellsAt(gridPos);
                }
                break;
                
            case EditMode.Move:
                if (e.button == 2 && isDragging)
                {
                    gridScrollPosition += e.delta;
                    Repaint();
                }
                break;
                
            case EditMode.Rectangle:
                if (e.button == 0 && isDragging)
                {
                    UpdateDragPreview(gridPos);
                }
                break;
        }
    }
    
    private void HandleMouseUp(Event e)
    {
        if (isDragging)
        {
            if (currentEditMode == EditMode.Rectangle)
            {
                ApplyRectangle();
            }
            else if (currentEditMode == EditMode.Fill)
            {
                ApplyFill();
            }
            
            isDragging = false;
            dragPreviewCells.Clear();
            Repaint();
        }
    }
    
    private void HandleScrollWheel(Event e)
    {
        float oldZoom = gridZoom;
        gridZoom = Mathf.Clamp(gridZoom - e.delta.y * 0.5f, 10f, 50f);
        
        // 以鼠标为中心缩放
        Vector2 mouseOffset = e.mousePosition - new Vector2(position.width * 0.5f, position.height * 0.5f);
        gridScrollPosition += mouseOffset * (oldZoom - gridZoom) * 0.01f;
        
        Repaint();
    }
    
    private void HandleKeyDown(Event e)
    {
        switch (e.keyCode)
        {
            case KeyCode.A:
                currentEditMode = EditMode.Add;
                Repaint();
                break;
                
            case KeyCode.R:
                currentEditMode = EditMode.Remove;
                Repaint();
                break;
                
            case KeyCode.P:
                currentEditMode = EditMode.SetPivot;
                Repaint();
                break;
                
            case KeyCode.M:
                currentEditMode = EditMode.Move;
                Repaint();
                break;
                
            case KeyCode.Escape:
                dragPreviewCells.Clear();
                Repaint();
                break;
        }
    }
    
    private void AddCellsAt(Vector2Int gridPos)
    {
        if (currentConfig == null) return;
        
        Undo.RecordObject(currentConfig, "添加单元格");
        
        for (int x = 0; x < currentBrushSize.x; x++)
        {
            for (int y = 0; y < currentBrushSize.y; y++)
            {
                Vector2Int cell = new Vector2Int(
                    gridPos.x + x - currentBrushSize.x / 2,
                    gridPos.y + y - currentBrushSize.y / 2
                );
                
                if (!currentConfig.ContainsCell(cell))
                {
                    currentConfig.AddCell(cell);
                }
            }
        }
        
        EditorUtility.SetDirty(currentConfig);
        Repaint();
    }
    
    private void RemoveCellsAt(Vector2Int gridPos)
    {
        if (currentConfig == null) return;
        
        Undo.RecordObject(currentConfig, "删除单元格");
        
        for (int x = 0; x < currentBrushSize.x; x++)
        {
            for (int y = 0; y < currentBrushSize.y; y++)
            {
                Vector2Int cell = new Vector2Int(
                    gridPos.x + x - currentBrushSize.x / 2,
                    gridPos.y + y - currentBrushSize.y / 2
                );
                
                if (currentConfig.ContainsCell(cell))
                {
                    currentConfig.RemoveCell(cell);
                }
            }
        }
        
        EditorUtility.SetDirty(currentConfig);
        Repaint();
    }
    
    private void SetPivotAt(Vector2Int gridPos)
    {
        if (currentConfig == null) return;
        
        Undo.RecordObject(currentConfig, "设置枢轴点");
        currentConfig.pivotOffset = gridPos;
        EditorUtility.SetDirty(currentConfig);
        Repaint();
    }
    
    private void UpdateDragPreview(Vector2Int currentPos)
    {
        dragPreviewCells.Clear();
        
        if (currentEditMode == EditMode.Rectangle)
        {
            int minX = Mathf.Min(dragStartCell.x, currentPos.x);
            int maxX = Mathf.Max(dragStartCell.x, currentPos.x);
            int minY = Mathf.Min(dragStartCell.y, currentPos.y);
            int maxY = Mathf.Max(dragStartCell.y, currentPos.y);
            
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    dragPreviewCells.Add(new Vector2Int(x, y));
                }
            }
        }
        else if (currentEditMode == EditMode.Fill)
        {
            // 简单的洪水填充算法预览
            // 这里只预览，实际应用在ApplyFill中
        }
        
        Repaint();
    }
    
    private void ApplyRectangle()
    {
        if (currentConfig == null || dragPreviewCells.Count == 0) return;
        
        Undo.RecordObject(currentConfig, "绘制矩形");
        
        foreach (var cell in dragPreviewCells)
        {
            if (currentEditMode == EditMode.Rectangle)
            {
                if (!currentConfig.ContainsCell(cell))
                {
                    currentConfig.AddCell(cell);
                }
            }
        }
        
        EditorUtility.SetDirty(currentConfig);
    }
    
    private void ApplyFill()
    {
        // 实现洪水填充算法
        // 这里需要更复杂的实现，暂不展开
    }
    
    private void DrawGridLines(Rect gridRect, Vector2 gridCenter)
    {
        Handles.BeginGUI();
        Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
        // 计算网格范围（以格子中心为基准）
        int halfGridX = gridDisplaySize.x / 2;
        int halfGridY = gridDisplaySize.y / 2;
    
        // 垂直线 - 绘制在格子边界上
        for (int x = -halfGridX; x <= halfGridX; x++)
        {
            // 网格线在格子边界，所以从 -0.5 开始
            float lineX = gridCenter.x + (x - 0.5f) * gridZoom + gridScrollPosition.x;
        
            if (lineX >= gridRect.x && lineX <= gridRect.x + gridRect.width)
            {
                Handles.DrawLine(
                    new Vector3(lineX, gridRect.y),
                    new Vector3(lineX, gridRect.y + gridRect.height)
                );
            }
        }
    
        // 水平线 - 绘制在格子边界上
        for (int y = -halfGridY; y <= halfGridY; y++)
        {
            // 网格线在格子边界，所以从 -0.5 开始
            float lineY = gridCenter.y + (y - 0.5f) * gridZoom + gridScrollPosition.y;
        
            if (lineY >= gridRect.y && lineY <= gridRect.y + gridRect.height)
            {
                Handles.DrawLine(
                    new Vector3(gridRect.x, lineY),
                    new Vector3(gridRect.x + gridRect.width, lineY)
                );
            }
        }
    
        Handles.EndGUI();
    }
    
    private void DrawGridAxes(Rect gridRect, Vector2 gridCenter)
    {
        Handles.BeginGUI();
        
        // 绘制坐标轴
        Vector2 origin = gridCenter + gridScrollPosition;
        
        // X轴
        Handles.color = new Color(1f, 0.5f, 0.5f, 0.8f);
        Handles.DrawLine(
            new Vector3(gridRect.x, origin.y),
            new Vector3(gridRect.x + gridRect.width, origin.y)
        );
        
        // Y轴
        Handles.color = new Color(0.5f, 0.5f, 1f, 0.8f);
        Handles.DrawLine(
            new Vector3(origin.x, gridRect.y),
            new Vector3(origin.x, gridRect.y + gridRect.height)
        );
        
        Handles.EndGUI();
    }
    
    private void DrawOccupiedCells(Rect gridRect, Vector2 gridCenter)
    {
        if (currentConfig == null || currentConfig.occupiedCells.Count == 0) return;
    
        Handles.BeginGUI();
    
        // 为每个单元格创建颜色纹理
        Texture2D cellTex = MakeTex(2, 2, currentConfig.editorColor);
    
        foreach (var cell in currentConfig.occupiedCells)
        {
            // 现在格子中心在整数坐标上，所以直接使用
            Vector2 cellCenter = gridCenter + gridScrollPosition + 
                                 new Vector2(cell.x * gridZoom, cell.y * gridZoom);
        
            Rect cellRect = new Rect(
                cellCenter.x - gridZoom * 0.5f,  // 从中心向两边扩展
                cellCenter.y - gridZoom * 0.5f,
                gridZoom,
                gridZoom
            );
        
            // 只在可见区域绘制
            if (gridRect.Overlaps(cellRect))
            {
                GUI.DrawTexture(cellRect, cellTex);
            
                // 绘制单元格边框
                Handles.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
                Handles.DrawSolidRectangleWithOutline(cellRect, 
                    Color.clear, 
                    new Color(0.1f, 0.1f, 0.1f, 0.5f));
            }
        }
    
        Handles.EndGUI();
    }
    
    private void DrawPivotPoint(Rect gridRect, Vector2 gridCenter)
    {
        if (currentConfig == null) return;
    
        Handles.BeginGUI();
    
        Vector2 pivotCenter = gridCenter + gridScrollPosition + 
                              new Vector2(currentConfig.pivotOffset.x * gridZoom, 
                                  currentConfig.pivotOffset.y * gridZoom);
    
        Rect pivotRect = new Rect(
            pivotCenter.x - gridZoom * 0.5f,
            pivotCenter.y - gridZoom * 0.5f,
            gridZoom,
            gridZoom
        );
    
        if (gridRect.Overlaps(pivotRect))
        {
            // 绘制枢轴点所在的格子
            Texture2D pivotTex = MakeTex(2, 2, currentConfig.pivotColor);
            GUI.DrawTexture(pivotRect, pivotTex);
        
            // 绘制十字线
            Handles.color = Color.white;
            Handles.DrawLine(
                new Vector3(pivotCenter.x - gridZoom * 0.3f, pivotCenter.y),
                new Vector3(pivotCenter.x + gridZoom * 0.3f, pivotCenter.y)
            );
            Handles.DrawLine(
                new Vector3(pivotCenter.x, pivotCenter.y - gridZoom * 0.3f),
                new Vector3(pivotCenter.x, pivotCenter.y + gridZoom * 0.3f)
            );
        
            // 绘制圆圈
            Handles.DrawWireArc(pivotCenter, Vector3.forward, Vector3.up, 360, gridZoom * 0.2f);
        
            // 绘制"P"标记
            GUIStyle pivotLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(gridZoom * 0.4f),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        
            GUI.Label(pivotRect, "P", pivotLabelStyle);
        }
    
        Handles.EndGUI();
    }
    
    private void DrawDragPreview(Rect gridRect, Vector2 gridCenter)
    {
        if (!isDragging || dragPreviewCells.Count == 0) return;
        
        Handles.BeginGUI();
        Handles.color = new Color(1f, 1f, 0f, 0.3f);
        
        foreach (var cell in dragPreviewCells)
        {
            Vector2 cellCenter = gridCenter + gridScrollPosition + 
                new Vector2(cell.x * gridZoom, cell.y * gridZoom);
            
            Rect cellRect = new Rect(
                cellCenter.x - gridZoom * 0.45f,
                cellCenter.y - gridZoom * 0.45f,
                gridZoom * 0.9f,
                gridZoom * 0.9f
            );
            
            if (gridRect.Overlaps(cellRect))
            {
                GUI.DrawTexture(cellRect, MakeTex(2, 2, new Color(1f, 1f, 0f, 0.3f)));
            }
        }
        
        Handles.EndGUI();
    }
    
    private void DrawGridCoordinates(Rect gridRect, Vector2 gridCenter)
    {
        Handles.BeginGUI();
    
        GUIStyle coordStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 9,
            normal = { textColor = Color.gray },
            alignment = TextAnchor.MiddleCenter
        };
    
        // 在格子中心显示坐标
        for (int x = -gridDisplaySize.x / 2; x <= gridDisplaySize.x / 2; x++)
        {
            for (int y = -gridDisplaySize.y / 2; y <= gridDisplaySize.y / 2; y++)
            {
                // 只在特定的格子显示坐标（避免过于密集）
                if (Mathf.Abs(x) % 2 == 0 && Mathf.Abs(y) % 2 == 0)
                {
                    Vector2 point = gridCenter + gridScrollPosition + 
                                    new Vector2(x * gridZoom, y * gridZoom);
                
                    if (gridRect.Contains(point))
                    {
                        Rect labelRect = new Rect(
                            point.x - 25,
                            point.y - 10,
                            50,
                            20
                        );
                    
                        GUI.Label(labelRect, $"({x},{y})", coordStyle);
                    }
                }
            }
        }
    
        Handles.EndGUI();
    }
    
    private void DrawOrigin(Rect gridRect, Vector2 gridCenter)
    {
        Handles.BeginGUI();
    
        Vector2 origin = gridCenter + gridScrollPosition;
    
        // 绘制原点标记
        Handles.color = Color.yellow;
        Handles.DrawWireArc(origin, Vector3.forward, Vector3.up, 360, 3);
    
        // 绘制坐标轴
        float axisLength = 20f;
    
        // X轴
        Handles.color = Color.red;
        Handles.DrawLine(origin, new Vector3(origin.x + axisLength, origin.y));
        Handles.DrawLine(new Vector3(origin.x + axisLength - 5, origin.y - 3), 
            new Vector3(origin.x + axisLength, origin.y));
        Handles.DrawLine(new Vector3(origin.x + axisLength - 5, origin.y + 3), 
            new Vector3(origin.x + axisLength, origin.y));
    
        // Y轴
        Handles.color = Color.green;
        Handles.DrawLine(origin, new Vector3(origin.x, origin.y + axisLength));
        Handles.DrawLine(new Vector3(origin.x - 3, origin.y + axisLength - 5), 
            new Vector3(origin.x, origin.y + axisLength));
        Handles.DrawLine(new Vector3(origin.x + 3, origin.y + axisLength - 5), 
            new Vector3(origin.x, origin.y + axisLength));
    
        // 原点标签
        GUIStyle originStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            normal = { textColor = Color.yellow },
            fontStyle = FontStyle.Bold
        };
    
        GUI.Label(new Rect(origin.x + 5, origin.y - 15, 40, 20), "原点", originStyle);
    
        Handles.EndGUI();
    }
    
    private void ShowMouseCoordinates(Vector2 mousePos, Vector2Int gridPos)
    {
        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTex(2, 2, new Color(0, 0, 0, 0.7f)) },
            padding = new RectOffset(5, 5, 2, 2)
        };
    
        Rect infoRect = new Rect(mousePos.x + 15, mousePos.y + 15, 120, 50);
    
        GUI.Box(infoRect, "", style);
    
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            normal = { textColor = Color.white }
        };
    
        // 显示格子坐标
        GUI.Label(new Rect(infoRect.x + 5, infoRect.y + 5, 110, 15), 
            $"格子: ({gridPos.x}, {gridPos.y})", labelStyle);
    
        if (currentConfig != null)
        {
            bool isOccupied = currentConfig.ContainsCell(gridPos);
            GUI.Label(new Rect(infoRect.x + 5, infoRect.y + 20, 110, 15), 
                $"状态: {(isOccupied ? "占用" : "空闲")}", labelStyle);
        
            // 显示世界坐标（如果需要）
            Vector2 worldPos = new Vector2(
                gridPos.x * (gridManagerRef?.cellSize ?? 1f),
                gridPos.y * (gridManagerRef?.cellSize ?? 1f)
            );
            GUI.Label(new Rect(infoRect.x + 5, infoRect.y + 35, 110, 15), 
                $"世界: ({worldPos.x:F1}, {worldPos.y:F1})", labelStyle);
        }
    }
    
    private Vector2Int GetGridPosition(Vector2 mousePos, Rect gridRect)
    {
        // 计算相对于网格中心的坐标
        Vector2 relativePos = mousePos - gridRect.center - gridScrollPosition;
    
        // 将鼠标坐标转换为网格坐标
        // 现在网格中心在格子中心，所以我们需要调整计算方法
        Vector2 gridPos = relativePos / gridZoom;
    
        if (snapToGrid)
        {
            // 使用四舍五入获取最近的格子
            return new Vector2Int(
                Mathf.RoundToInt(gridPos.x),
                Mathf.RoundToInt(gridPos.y)
            );
        }
        else
        {
            // 使用向下取整
            return new Vector2Int(
                Mathf.FloorToInt(gridPos.x),
                Mathf.FloorToInt(gridPos.y)
            );
        }
    }
    
    private void DrawStatusBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        string modeText = "";
        switch (currentEditMode)
        {
            case EditMode.Add: modeText = "添加模式 (A)"; break;
            case EditMode.Remove: modeText = "删除模式 (R)"; break;
            case EditMode.SetPivot: modeText = "设置枢轴 (P)"; break;
            case EditMode.Move: modeText = "移动视图 (M)"; break;
            case EditMode.Rectangle: modeText = "矩形绘制"; break;
            case EditMode.Fill: modeText = "填充绘制"; break;
        }
        
        EditorGUILayout.LabelField(modeText, EditorStyles.miniLabel, GUILayout.Width(100));
        
        GUILayout.FlexibleSpace();
        
        if (currentConfig != null)
        {
            EditorGUILayout.LabelField($"占用单元格: {currentConfig.occupiedCells.Count}", 
                EditorStyles.miniLabel, GUILayout.Width(100));
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    // 文件操作
    private void CreateNewConfig()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "创建新配置",
            "NewGridObject",
            "asset",
            "选择保存位置"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            GridObjectConfig newConfig = CreateInstance<GridObjectConfig>();
            newConfig.objectName = "新网格物体";
            
            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            
            currentConfig = newConfig;
            serializedConfig = new SerializedObject(currentConfig);
            
            Selection.activeObject = newConfig;
            EditorGUIUtility.PingObject(newConfig);
        }
    }
    
    private void OpenConfig()
    {
        string path = EditorUtility.OpenFilePanel("打开配置", "Assets", "asset");
        if (!string.IsNullOrEmpty(path))
        {
            // 转换为相对路径
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }
            
            GridObjectConfig config = AssetDatabase.LoadAssetAtPath<GridObjectConfig>(path);
            if (config != null)
            {
                currentConfig = config;
                serializedConfig = new SerializedObject(currentConfig);
            }
        }
    }
    
    private void SaveConfig()
    {
        if (currentConfig != null)
        {
            EditorUtility.SetDirty(currentConfig);
            AssetDatabase.SaveAssets();
            Debug.Log($"配置已保存: {currentConfig.name}");
        }
    }
    
    private void SaveConfigAs()
    {
        if (currentConfig == null) return;
        
        string path = EditorUtility.SaveFilePanelInProject(
            "另存为",
            currentConfig.objectName,
            "asset",
            "选择保存位置"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            GridObjectConfig newConfig = Instantiate(currentConfig);
            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            
            currentConfig = newConfig;
            serializedConfig = new SerializedObject(currentConfig);
        }
    }
    
    // 形状操作
    private void CenterShape()
    {
        if (currentConfig == null || currentConfig.occupiedCells.Count == 0) return;
        
        var bounds = currentConfig.GetBounds();
        Vector2Int centerOffset = new Vector2Int((int)bounds.center.x, (int)bounds.center.y);
        
        // 移动所有单元格
        for (int i = 0; i < currentConfig.occupiedCells.Count; i++)
        {
            currentConfig.occupiedCells[i] -= centerOffset;
        }
        
        // 调整枢轴点
        currentConfig.pivotOffset -= centerOffset;
    }
    
    private void MirrorShape(bool horizontal)
    {
        if (currentConfig == null) return;
        
        List<Vector2Int> newCells = new List<Vector2Int>();
        
        foreach (var cell in currentConfig.occupiedCells)
        {
            Vector2Int mirrored = horizontal ?
                new Vector2Int(-cell.x, cell.y) :
                new Vector2Int(cell.x, -cell.y);
            
            if (!newCells.Contains(mirrored))
            {
                newCells.Add(mirrored);
            }
        }
        
        // 镜像枢轴点
        currentConfig.pivotOffset = horizontal ?
            new Vector2(-currentConfig.pivotOffset.x, currentConfig.pivotOffset.y) :
            new Vector2(currentConfig.pivotOffset.x, -currentConfig.pivotOffset.y);
        
        currentConfig.occupiedCells = newCells;
    }
    
    private void RotateShape(float angle)
    {
        if (currentConfig == null) return;
        
        List<Vector2Int> newCells = new List<Vector2Int>();
        float radians = angle * Mathf.Deg2Rad;
        
        foreach (var cell in currentConfig.occupiedCells)
        {
            Vector2 rotated = RotatePoint(cell, radians);
            Vector2Int rounded = new Vector2Int(
                Mathf.RoundToInt(rotated.x),
                Mathf.RoundToInt(rotated.y)
            );
            
            if (!newCells.Contains(rounded))
            {
                newCells.Add(rounded);
            }
        }
        
        // 旋转枢轴点
        Vector2 rotatedPivot = RotatePoint(currentConfig.pivotOffset, radians);
        currentConfig.pivotOffset = rotatedPivot;
        
        currentConfig.occupiedCells = newCells;
    }
    
    private Vector2 RotatePoint(Vector2 point, float radians)
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        
        return new Vector2(
            point.x * cos - point.y * sin,
            point.x * sin + point.y * cos
        );
    }
    
    private void ImportFromPrefab()
    {
        if (currentConfig == null || currentConfig.prefab == null) return;
        
        GameObject prefab = currentConfig.prefab;
        MeshFilter meshFilter = prefab.GetComponentInChildren<MeshFilter>();
        
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            EditorUtility.DisplayDialog("导入失败", "Prefab中没有找到Mesh", "确定");
            return;
        }
        
        // 这里可以实现从Mesh生成占位网格的逻辑
        // 由于复杂度较高，这里只提供框架
        EditorUtility.DisplayDialog("导入功能", "从Prefab导入网格功能待实现", "确定");
    }
    
    // 辅助函数
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}