using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BaseItem))]
public class BaseItemPropertyDrawer : PropertyDrawer
{
    // 定义两列的宽度比例（可根据需要调整）
    private const float DragColumnWidth = 0.7f;   // 拖拽栏占70%宽度
    private const float InfoColumnWidth = 0.28f;  // 信息栏占28%宽度（留2%间距）

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 1. 开始绘制属性（保留Unity默认的标签样式）
        EditorGUI.BeginProperty(position, label, property);

        // 2. 拆分单行区域为两个独立的列（同一行）
        Rect totalRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        
        // 拖拽列：占左侧70%宽度
        Rect dragRect = new Rect(
            totalRect.x, 
            totalRect.y, 
            totalRect.width * DragColumnWidth, 
            totalRect.height
        );
        
        // 信息列：占右侧28%宽度（和拖拽列留少量间距）
        Rect infoRect = new Rect(
            totalRect.x + totalRect.width * (DragColumnWidth + 0.02f), 
            totalRect.y, 
            totalRect.width * InfoColumnWidth, 
            totalRect.height
        );

        // 3. 绘制第一列：拖拽选择栏（原有功能保留）
        EditorGUI.ObjectField(dragRect, property, typeof(BaseItem), GUIContent.none);

        // 4. 绘制第二列：单独的信息栏
        BaseItem item = property.objectReferenceValue as BaseItem;
        if (item != null)
        {
            // 定制信息栏样式（区分拖拽栏）
            GUIStyle infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft, // 信息左对齐
                normal = { textColor = Color.cyan }, // 蓝色文字
                padding = new RectOffset(5, 5, 0, 0) // 内边距，避免文字贴边
            };

            // 拼接要显示的信息
            string infoText = $"{item.itemName} | {item.count}";
            // 绘制信息文本（自动换行适配列宽）
            EditorGUI.LabelField(infoRect, infoText, infoStyle);
        }
        else
        {
            // 未选择Item时，信息栏显示提示文字
            GUIStyle emptyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.gray }
            };
            EditorGUI.LabelField(infoRect, "未选择物品", emptyStyle);
        }

        // 5. 结束属性绘制
        EditorGUI.EndProperty();
    }

    // 保持行高为默认值（确保和其他属性行对齐）
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }
}