using TMPro;
using UnityEditor;
using UnityEngine.UI;

namespace YangTool
{
    public static class RaycastEditorHide
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }
        
        private static void OnHierarchyChanged()
        {
            var activeGameObject = Selection.activeGameObject;
            if (activeGameObject != null)
            {
                if (activeGameObject.name.Contains("Text (TMP)"))
                {
                    var txtComponent = activeGameObject.GetComponent<TextMeshProUGUI>();
                    if (txtComponent != null)
                    {
                        txtComponent.raycastTarget = false;
                    }
                }
            }
        }
    }
}