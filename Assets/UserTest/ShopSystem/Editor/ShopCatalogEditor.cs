using UnityEditor;
using UnityEngine;

namespace ShopSystem.EditorTools
{
    #region CatalogEditor
    /// <summary>
    /// 商店商品目录的中文 Inspector 绘制器
    /// </summary>
    [CustomEditor(typeof(ShopCatalog))]
    public sealed class ShopCatalogEditor : UnityEditor.Editor
    {
        // 商品列表序列化属性
        private SerializedProperty products;

        /// <summary>
        /// 查找商品列表序列化属性
        /// </summary>
        private void OnEnable()
        {
            products = serializedObject.FindProperty("products");
        }

        /// <summary>
        /// 绘制中文商品目录 Inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("商店商品目录", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (products == null)
            {
                EditorGUILayout.HelpBox("未找到商品列表序列化字段", MessageType.Error);
            }
            else
            {
                // 使用 Unity 原生列表绘制 保留原有折叠排序添加和删除行为
                EditorGUILayout.PropertyField(products, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endregion

    #region DrawerUtility
    /// <summary>
    /// 商品元素字段绘制辅助方法
    /// </summary>
    internal static class ShopCatalogEditorDrawerUtility
    {
        /// <summary>
        /// 绘制带中文标题的序列化字段
        /// </summary>
        /// <param name="position">当前绘制位置</param>
        /// <param name="property">字段序列化属性</param>
        /// <param name="label">字段显示标题</param>
        /// <param name="includeChildren">是否绘制子字段</param>
        public static void DrawField(
            ref Rect position,
            SerializedProperty property,
            string label,
            bool includeChildren = false)
        {
            if (property == null)
            {
                return;
            }

            float height = EditorGUI.GetPropertyHeight(property, includeChildren);
            Rect fieldPosition = new Rect(position.x, position.y, position.width, height);
            EditorGUI.PropertyField(fieldPosition, property, new GUIContent(label), includeChildren);
            position.y += height + EditorGUIUtility.standardVerticalSpacing;
        }

        /// <summary>
        /// 获取父属性及其字段的展开高度
        /// </summary>
        /// <param name="property">父序列化属性</param>
        /// <param name="fieldNames">要绘制的相对字段名称</param>
        /// <returns>属性展开时所需高度</returns>
        public static float GetExpandedHeight(SerializedProperty property, params string[] fieldNames)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
            {
                return height;
            }

            for (int i = 0; i < fieldNames.Length; i++)
            {
                SerializedProperty child = property.FindPropertyRelative(fieldNames[i]);
                if (child != null)
                {
                    height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }
    }
    #endregion

    #region ProductDrawer
    /// <summary>
    /// 商品数据元素的中文属性绘制器
    /// </summary>
    [CustomPropertyDrawer(typeof(ShopProductData))]
    internal sealed class ShopProductDataDrawer : PropertyDrawer
    {
        /// <summary>
        /// 绘制商品元素字段
        /// </summary>
        /// <param name="position">绘制区域</param>
        /// <param name="property">商品序列化属性</param>
        /// <param name="label">列表元素标题</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect current = position;
            property.isExpanded = EditorGUI.Foldout(
                new Rect(current.x, current.y, current.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded,
                label,
                true);

            if (property.isExpanded)
            {
                current.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.indentLevel++;
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("id"),
                    "商品 ID");
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("displayName"),
                    "显示名称");
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("icon"),
                    "图标");
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("rewards"),
                    "Rewards",
                    true);
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("costs"),
                    "Costs",
                    true);
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("purchaseMethod"),
                    "购买方式");
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("maxPurchaseCount"),
                    "最大购买次数");
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("requiredAdViews"),
                    "所需广告次数");
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 获取商品元素属性高度
        /// </summary>
        /// <param name="property">商品序列化属性</param>
        /// <param name="label">列表元素标题</param>
        /// <returns>属性绘制高度</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShopCatalogEditorDrawerUtility.GetExpandedHeight(
                property,
                "id",
                "displayName",
                "icon",
                "rewards",
                "costs",
                "purchaseMethod",
                "maxPurchaseCount",
                "requiredAdViews");
        }
    }
    #endregion

    #region RewardDrawer
    /// <summary>
    /// 奖励数据元素的中文属性绘制器
    /// </summary>
    [CustomPropertyDrawer(typeof(ShopRewardData))]
    internal sealed class ShopRewardDataDrawer : PropertyDrawer
    {
        /// <summary>
        /// 绘制奖励元素字段
        /// </summary>
        /// <param name="position">绘制区域</param>
        /// <param name="property">奖励序列化属性</param>
        /// <param name="label">列表元素标题</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawElement(position, property, label);
        }

        /// <summary>
        /// 获取奖励元素属性高度
        /// </summary>
        /// <param name="property">奖励序列化属性</param>
        /// <param name="label">列表元素标题</param>
        /// <returns>属性绘制高度</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShopCatalogEditorDrawerUtility.GetExpandedHeight(property, "itemId", "amount", "icon");
        }

        private static void DrawElement(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect current = position;
            property.isExpanded = EditorGUI.Foldout(
                new Rect(current.x, current.y, current.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded,
                label,
                true);

            if (property.isExpanded)
            {
                current.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.indentLevel++;
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("itemId"),
                    "道具 ID");
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("amount"),
                    "数量");
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("icon"),
                    "图标");
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
    #endregion

    #region CostDrawer
    /// <summary>
    /// 消耗数据元素的中文属性绘制器
    /// </summary>
    [CustomPropertyDrawer(typeof(ShopCostData))]
    internal sealed class ShopCostDataDrawer : PropertyDrawer
    {
        /// <summary>
        /// 绘制消耗元素字段
        /// </summary>
        /// <param name="position">绘制区域</param>
        /// <param name="property">消耗序列化属性</param>
        /// <param name="label">列表元素标题</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawElement(position, property, label);
        }

        /// <summary>
        /// 获取消耗元素属性高度
        /// </summary>
        /// <param name="property">消耗序列化属性</param>
        /// <param name="label">列表元素标题</param>
        /// <returns>属性绘制高度</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShopCatalogEditorDrawerUtility.GetExpandedHeight(property, "itemId", "amount", "icon");
        }

        private static void DrawElement(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect current = position;
            property.isExpanded = EditorGUI.Foldout(
                new Rect(current.x, current.y, current.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded,
                label,
                true);

            if (property.isExpanded)
            {
                current.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.indentLevel++;
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("itemId"),
                    "道具 ID");
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("amount"),
                    "数量");
                ShopCatalogEditorDrawerUtility.DrawField(
                    ref current,
                    property.FindPropertyRelative("icon"),
                    "图标");
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
    #endregion
}
