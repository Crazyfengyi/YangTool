using UnityEditor;
using UnityEngine;

namespace ES3Internal
{
    [CustomEditor(typeof(ES3AutoSave))]
    public class ES3AutoSaveEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (target == null)
                return;

            DrawDefaultInspector();

            if (GUILayout.Button("Manage Auto Save Settings"))
                ES3Editor.ES3Window.InitAndShowAutoSave();
        }
    }
}