#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using YangTools.Scripts.Core;

namespace YangTools.EditorStudio.Editor.AutoTools.TextTool
{
    /// <summary>
    /// 重命名 TMP 字体资源内嵌的 Atlas 贴图和材质子资源。
    /// </summary>
    internal sealed class TMPFontSubAssetRenameWindow : EditorWindow
    {
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private string atlasName = string.Empty;
        [SerializeField] private string materialName = string.Empty;
        private string lastKnownAtlasName = string.Empty;
        private string lastKnownMaterialName = string.Empty;

        /// <summary>
        /// 打开 TMP 字体子资源重命名窗口。
        /// </summary>
        [MenuItem(SettingInfo.YongToolsFunctionPath + "TMP字体子资源重命名")]
        private static void OpenWindow()
        {
            TMPFontSubAssetRenameWindow window = GetWindow<TMPFontSubAssetRenameWindow>("TMP字体子资源重命名");
            window.minSize = new Vector2(360f, 170f);
            TMP_FontAsset selectedFont = Selection.activeObject as TMP_FontAsset;
            if (selectedFont != null)
            {
                window.SetFontAsset(selectedFont);
            }

            window.Show();
        }

        /// <summary>
        /// 注册撤销与重做后的字体子资源保存回调。
        /// </summary>
        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        /// <summary>
        /// 注销撤销与重做回调，避免窗口关闭后保留事件订阅。
        /// </summary>
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        /// <summary>
        /// 绘制字体选择、子资源名称和应用按钮。
        /// </summary>
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            TMP_FontAsset selectedFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
                "TMP 字体资源", fontAsset, typeof(TMP_FontAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                SetFontAsset(selectedFont);
            }

            if (fontAsset == null)
            {
                EditorGUILayout.HelpBox("请选择一个 TMP 字体资源。", MessageType.Info);
                return;
            }

            if (!TryGetEmbeddedSubAssets(out Texture2D atlas, out Material material, out string errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Warning);
                return;
            }

            atlasName = EditorGUILayout.TextField("Atlas 贴图名称", atlasName);
            materialName = EditorGUILayout.TextField("Material 名称", materialName);

            bool hasValidNames = !string.IsNullOrWhiteSpace(atlasName)
                                 && !string.IsNullOrWhiteSpace(materialName);
            bool hasChanges = atlas.name != atlasName || material.name != materialName;
            using (new EditorGUI.DisabledScope(!hasValidNames || !hasChanges))
            {
                if (GUILayout.Button("应用名称", GUILayout.Height(28f)))
                {
                    RenameSubAssets(atlas, material);
                }
            }
        }

        /// <summary>
        /// 选择字体资源并同步读取其当前子资源名称。
        /// </summary>
        /// <param name="selectedFont">选中的 TMP 字体资源。</param>
        private void SetFontAsset(TMP_FontAsset selectedFont)
        {
            fontAsset = selectedFont;
            atlasName = string.Empty;
            materialName = string.Empty;

            if (fontAsset != null && TryGetEmbeddedSubAssets(out Texture2D atlas, out Material material, out _))
            {
                atlasName = atlas.name;
                materialName = material.name;
                lastKnownAtlasName = atlas.name;
                lastKnownMaterialName = material.name;
            }
        }

        /// <summary>
        /// 获取与所选字体同属一个资产文件的 Atlas 贴图和材质子资源。
        /// </summary>
        /// <param name="atlas">字体 Atlas 贴图。</param>
        /// <param name="material">字体 Atlas 材质。</param>
        /// <param name="errorMessage">验证失败时显示的说明。</param>
        /// <returns>两个对象都是所选字体资产的内嵌子资源时返回 true。</returns>
        private bool TryGetEmbeddedSubAssets(out Texture2D atlas, out Material material, out string errorMessage)
        {
            atlas = null;
            material = null;
            errorMessage = string.Empty;
            if (fontAsset == null)
            {
                errorMessage = "请选择一个 TMP 字体资源。";
                return false;
            }

            atlas = fontAsset.atlasTexture;
            material = fontAsset.material;
            string fontPath = AssetDatabase.GetAssetPath(fontAsset);
            bool atlasIsEmbedded = atlas != null
                                   && AssetDatabase.IsSubAsset(atlas)
                                   && string.Equals(AssetDatabase.GetAssetPath(atlas), fontPath, StringComparison.Ordinal);
            bool materialIsEmbedded = material != null
                                      && AssetDatabase.IsSubAsset(material)
                                      && string.Equals(AssetDatabase.GetAssetPath(material), fontPath, StringComparison.Ordinal);
            if (atlasIsEmbedded && materialIsEmbedded)
            {
                return true;
            }

            errorMessage = "所选字体缺少内嵌 Atlas 贴图或 Material，未启用重命名。";
            return false;
        }

        /// <summary>
        /// 记录撤销、更新两个子资源名称并保存字体资产。
        /// </summary>
        /// <param name="atlas">字体 Atlas 贴图。</param>
        /// <param name="material">字体 Atlas 材质。</param>
        private void RenameSubAssets(Texture2D atlas, Material material)
        {
            string newAtlasName = atlasName.Trim();
            string newMaterialName = materialName.Trim();
            Undo.RecordObjects(new UnityEngine.Object[] { atlas, material }, "重命名 TMP 字体子资源");

            atlas.name = newAtlasName;
            material.name = newMaterialName;
            EditorUtility.SetDirty(atlas);
            EditorUtility.SetDirty(material);
            EditorUtility.SetDirty(fontAsset);

            try
            {
                AssetDatabase.SaveAssetIfDirty(fontAsset);
                atlasName = newAtlasName;
                materialName = newMaterialName;
                lastKnownAtlasName = atlas.name;
                lastKnownMaterialName = material.name;
                ShowNotification(new GUIContent("TMP 字体子资源名称已保存"));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("TMP 字体子资源重命名失败", exception.Message, "确定");
            }
        }

        /// <summary>
        /// 撤销或重做后保存恢复后的子资源名称，并刷新窗口输入框。
        /// </summary>
        private void OnUndoRedoPerformed()
        {
            if (!TryGetEmbeddedSubAssets(out Texture2D atlas, out Material material, out _))
            {
                return;
            }

            if (atlas.name == lastKnownAtlasName && material.name == lastKnownMaterialName)
            {
                return;
            }

            EditorUtility.SetDirty(atlas);
            EditorUtility.SetDirty(material);
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssetIfDirty(fontAsset);
            atlasName = atlas.name;
            materialName = material.name;
            lastKnownAtlasName = atlas.name;
            lastKnownMaterialName = material.name;
            Repaint();
        }
    }
}
#endif
