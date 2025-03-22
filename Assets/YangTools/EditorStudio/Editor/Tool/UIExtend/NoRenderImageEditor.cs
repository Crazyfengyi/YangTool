#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using YangTools.Scripts.Core;

namespace YangTools
{
    public static class YImageEditor
    {
        [MenuItem(SettingInfo.YangToolUIPath + "NoRenderImage")]
        private static void CreatImage()
        {
            if (!Selection.activeTransform) return;
            if (!Selection.activeTransform.GetComponentInParent<Canvas>()) return;
            
            var go = new GameObject("NoRenderImage", typeof(NoRenderImage));
            go.GetComponent<NoRenderImage>().raycastTarget = true;
            go.transform.SetParent(Selection.activeTransform);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
        }
    }
}
#endif