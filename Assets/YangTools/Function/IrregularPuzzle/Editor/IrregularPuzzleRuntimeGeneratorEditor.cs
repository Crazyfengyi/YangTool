#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YangTools.Function.IrregularPuzzle.Editor
{
    /// <summary>运行时拼图生成器的 Inspector 测试入口。</summary>
    [CustomEditor(typeof(IrregularPuzzleRuntimeGenerator))]
    public sealed class IrregularPuzzleRuntimeGeneratorEditor : UnityEditor.Editor
    {
        /// <summary>绘制默认配置和运行时测试按钮。</summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            IrregularPuzzleRuntimeGenerator generator = (IrregularPuzzleRuntimeGenerator)target;
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("编辑器测试", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("进入 Play Mode 后，点击按钮会使用“Editor Test Texture”动态生成一局拼图。",
                MessageType.Info);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || generator.EditorTestTexture == null))
            {
                if (GUILayout.Button("生成测试关卡", GUILayout.Height(28f)))
                {
                    generator.Generate(generator.EditorTestTexture);
                }
            }
        }
    }
}
#endif
