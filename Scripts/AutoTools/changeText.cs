using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

namespace YangTools
{
    public class changeText : EditorWindow
    {
        [MenuItem("YangTools/替换字体")]
        public static void OpenWindow()
        {
            changeText changeText = EditorWindow.GetWindow<changeText>();
            changeText.Show();
        }

        Font change;
        static Font changeFont;
        Font toChange;
        static Font toChangeFont;

        void OnGUI()
        {
            EditorGUILayout.HelpBox("将会替换场景中Canvas名称下的所有Text的字体", MessageType.Info);
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
        public static void ChangeText()
        {
            Transform canvas = GameObject.Find("Canvas").transform;
            if (!canvas)
            {
                Debug.Log("Sence no canvas");
                return;
            }
            Transform[] tArray = canvas.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < tArray.Length; i++)
            {
                Text t = tArray[i].GetComponent<Text>();
                if (t)
                {
                    Undo.RecordObject(t, t.gameObject.name);
                    if (t.font == changeFont)
                    {
                        t.font = toChangeFont;
                        EditorUtility.SetDirty(t);
                    }
                }
            }
        }
    }
}