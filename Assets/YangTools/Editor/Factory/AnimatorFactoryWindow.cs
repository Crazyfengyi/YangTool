using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor;
using UnityEngine;

public class AnimatorFactoryWindow : EditorWindow
{
    #region const
    private const string AnimatorSavePath = "Assets/Standard Assets/Characters/ThirdPersonCharacter/Animator/";
    private const string PrefabSavePath = "Assets/Standard Assets/Characters/ThirdPersonCharacter/Prefabs/";
    private const string ModelPath = "Assets/Standard Assets/Characters/ThirdPersonCharacter/Models/";
    private const string AnimationPath = "Assets/Standard Assets/Characters/ThirdPersonCharacter/Animation/Humanoid";
    private const string AnimatorControllerSuffix = "AnimatorController.controller";
    #endregion const

    #region private members
    private string controllerName;
    private AnimatorController aniController;
    private AnimatorStateMachine baseLayerMachine;
    #endregion private members

    #region EditorWindow
    [MenuItem("Window/AnimatorFactory")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow(typeof(AnimatorFactoryWindow));
    }
    void OnEnable()
    {
        controllerName = "default name";
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
            return;
        CreateController();
        AddParameters();
        CreateState();
    }
    /// <summary>
	/// ��������״̬��
	/// </summary>
	private void CreateController()
    {
        aniController = AnimatorController.CreateAnimatorControllerAtPath(AnimatorSavePath + controllerName + AnimatorControllerSuffix);
        baseLayerMachine = aniController.layers[0].stateMachine;
        baseLayerMachine.entryPosition = Vector3.zero;
        baseLayerMachine.exitPosition = new Vector3(400f, 200f);
        baseLayerMachine.anyStatePosition = new Vector3(0f, 200f);
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
        AnimationClip idleClip = AnimatorFactoryTool.LoadAnimClip(AnimationPath + "Idle.FBX");
        AnimatorState stateIdle = baseLayerMachine.AddState("Idle", new Vector3(300f, 0f));
        stateIdle.motion = idleClip;
        baseLayerMachine.defaultState = stateIdle;
    }
}
