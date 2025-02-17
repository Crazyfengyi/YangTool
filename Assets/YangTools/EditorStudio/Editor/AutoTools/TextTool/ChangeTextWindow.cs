#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace YangTools
{
    /// <summary>
    /// 替换字体
    /// </summary>
    public class ChangeTextWindow : EditorWindow
    {
        Font change;
        static Font changeFont;
        Font toChange;
        static Font toChangeFont;

        [MenuItem(SettingInfo.YongToolsFunctionPath + "替换字体")]
        public static void OpenWindow()
        {
            ChangeTextWindow changeText = EditorWindow.GetWindow<ChangeTextWindow>();
            changeText.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox("将会替换场景中第一个找到的Canvas下的所有Text的字体", MessageType.Info);
            EditorGUILayout.Space(60);
            change = (Font)EditorGUILayout.ObjectField("目标字体", change, typeof(Font), true, GUILayout.MinWidth(100f));
            changeFont = change;
            EditorGUILayout.Space(20);
            toChange = (Font)EditorGUILayout.ObjectField("替换为字体", toChange, typeof(Font), true, GUILayout.MinWidth(100f));
            toChangeFont = toChange;
            EditorGUILayout.Space(30);
            if (GUILayout.Button("替换"))
            {
                ChangeText();
            }
        }

        /// <summary>
        /// 切换字体
        /// </summary>
        public static void ChangeText()
        {
            Transform canvas = GameObject.Find("Canvas").transform;
            if (!canvas)
            {
                Debug.Log("Sence no canvas");
                return;
            }

            Text[] textArray = canvas.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < textArray.Length; i++)
            {
                Text text = textArray[i];
                Undo.RecordObject(text, text.gameObject.name);
                if (text.font == changeFont)
                {
                    text.font = toChangeFont;
                    EditorUtility.SetDirty(text);
                }
            }
        }
    }
}
#endif