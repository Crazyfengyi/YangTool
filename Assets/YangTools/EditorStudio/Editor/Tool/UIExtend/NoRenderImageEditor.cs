using UnityEditor;
using UnityEngine;

namespace YangTools
{
    public class YImage_editor
    {
        [MenuItem(SettingInfo.YangToolUIPath + "NoRenderImage")]
        static void CreatImage()
        {
            if (Selection.activeTransform)
            {
                if (Selection.activeTransform.GetComponentInParent<Canvas>())
                {
                    GameObject go = new GameObject("NoRenderImage", typeof(NoRenderImage));
                    go.GetComponent<NoRenderImage>().raycastTarget = true;
                    go.transform.SetParent(Selection.activeTransform);
                    go.transform.localScale = Vector3.one;
                    go.transform.localPosition = Vector3.zero;
                }
            }
        }
    }
}