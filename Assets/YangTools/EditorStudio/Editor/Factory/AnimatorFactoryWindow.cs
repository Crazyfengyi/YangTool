using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor;
using UnityEngine;
using YangTools;
using System.IO;

public class AnimatorFactoryWindow : EditorWindow
{
    #region const
    //����������·��
    private const string AnimatorSavePath = "Assets/Animator/";
    //����·��
    private const string AnimationPath = "Assets/UserTest/MyTest/Axe_anim.fbx";
    //��׺
    private const string AnimatorControllerSuffix = "_AniController.controller";
    #endregion

    #region private members
    private string controllerName;
    private AnimatorController aniController;
    private AnimatorStateMachine baseLayerMachine;
    #endregion

    #region EditorWindow
    [MenuItem(SettingInfo.YongToolsFunctionPath + "����������")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow(typeof(AnimatorFactoryWindow));
    }
    void OnEnable()
    {
        controllerName = "defaultName";
    }
    void OnGUI()
    {
        GUILayout.Label("AnimationSettings", EditorStyles.boldLabel);
        controllerName = EditorGUILayout.TextField("����������:", controllerName);
        if (GUILayout.Button("���ɶ�����"))
        {
            GenerateController();
        }
    }
    #endregion EditorWindow

    /// <summary>
    /// ���ɶ�����
    /// </summary>
    public void GenerateController()
    {
        if (string.IsNullOrEmpty(controllerName))
        {
            CommonEditorTool.CommonTipsPanel("����ʧ��,�붯������������");
            return;
        }
        string path = AnimatorSavePath + controllerName + AnimatorControllerSuffix;
        if (File.Exists(path))
        {
            CommonEditorTool.CommonTipsPanel("����ʧ��,�Ѿ�����ͬ���������ļ�");
            return;
        }
        CreateController();
        AddParameters();
        CreateState();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        CommonEditorTool.CommonTipsPanel("�����ɹ�");
    }
    /// <summary>
	/// ��������״̬��
	/// </summary>
	private void CreateController()
    {
        aniController = AnimatorController.CreateAnimatorControllerAtPath(AnimatorSavePath + controllerName + AnimatorControllerSuffix);
        baseLayerMachine = aniController.layers[0].stateMachine;
        baseLayerMachine.entryPosition = Vector3.zero;
        baseLayerMachine.anyStatePosition = new Vector3(0f, 200f);
        baseLayerMachine.exitPosition = new Vector3(0f, 300f);

        //����һ��Null��State
        AnimatorState stateIdle = baseLayerMachine.AddState("Null", new Vector3(0, 100, 0));
        stateIdle.speedParameter = "speedControl";
        stateIdle.speedParameterActive = true;
        stateIdle.motion = null;
        baseLayerMachine.defaultState = stateIdle;

        EditorUtility.SetDirty(aniController);
        EditorUtility.SetDirty(baseLayerMachine);
        EditorUtility.SetDirty(stateIdle);
    }
    /// <summary>
	/// ���ö���״̬������
	/// </summary>
	private void AddParameters()
    {
        AnimatorControllerParameter playSpeed = new AnimatorControllerParameter();
        playSpeed.name = "speedControl";
        playSpeed.type = AnimatorControllerParameterType.Float;
        playSpeed.defaultFloat = 1.0f;
        aniController.AddParameter(playSpeed);
    }
    /// <summary>
	/// ���״̬
	/// </summary>
	private void CreateState()
    {
        AnimationClip[] clips = AnimatorFactoryTool.LoadAllAnimClip(AnimationPath);
        int rowCount = 8;//һ�м��� 
        int currentRow = 0;//��ǰ����
        int currentRowCount = 0;//��ǰ��clip����

        for (int i = 0; i < clips.Length; i++)
        {
            if (i % rowCount == 0)
            {
                currentRow += 1;
                currentRowCount = 0;
            }

            var x = currentRow * 300;
            var y = currentRowCount * 100;
            currentRowCount++;

            AnimatorState stateIdle = baseLayerMachine.AddState(clips[i].name, new Vector3(x, y, 0));
            stateIdle.speedParameter = "speedControl";
            stateIdle.speedParameterActive = true;
            stateIdle.motion = clips[i];
            EditorUtility.SetDirty(stateIdle);
        }
    }
}
