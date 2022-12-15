using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using YangTools;

public class AnimatorFactoryWindow : EditorWindow
{
    #region const
    //动画机保存路径
    private const string AnimatorSavePath = "Assets/Animator/";
    //动画路径
    private const string AnimationPath = "Assets/UserTest/MyTest/Axe_anim.fbx";
    //后缀
    private const string AnimatorControllerSuffix = "_AniController.controller";
    #endregion

    #region private members
    private string controllerName;
    private AnimatorController aniController;
    private AnimatorStateMachine baseLayerMachine;
    #endregion

    #region EditorWindow
    [MenuItem(SettingInfo.YongToolsFunctionPath + "动画机工厂")]
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
        controllerName = EditorGUILayout.TextField("动画机名称:", controllerName);
        if (GUILayout.Button("生成动画机"))
        {
            GenerateController();
        }
    }
    #endregion EditorWindow

    /// <summary>
    /// 生成动画机
    /// </summary>
    public void GenerateController()
    {
        if (string.IsNullOrEmpty(controllerName))
        {
            CommonEditorTool.CommonTipsPanel("创建失败,请动画机输入名称");
            return;
        }
        string path = AnimatorSavePath + controllerName + AnimatorControllerSuffix;
        if (File.Exists(path))
        {
            CommonEditorTool.CommonTipsPanel("创建失败,已经包含同名动画机文件");
            return;
        }
        CreateController();
        AddParameters();
        CreateState();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        CommonEditorTool.CommonTipsPanel("创建成功");
    }
    /// <summary>
	/// 创建动画状态机
	/// </summary>
	private void CreateController()
    {
        aniController = AnimatorController.CreateAnimatorControllerAtPath(AnimatorSavePath + controllerName + AnimatorControllerSuffix);
        baseLayerMachine = aniController.layers[0].stateMachine;
        baseLayerMachine.entryPosition = Vector3.zero;
        baseLayerMachine.anyStatePosition = new Vector3(0f, 200f);
        baseLayerMachine.exitPosition = new Vector3(0f, 300f);

        //创建一个Null的State
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
	/// 设置动画状态机参数
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
	/// 添加状态
	/// </summary>
	private void CreateState()
    {
        AnimationClip[] clips = AnimatorFactoryTool.LoadAllAnimClip(AnimationPath);
        int rowCount = 8;//一列几个 
        int currentRow = 0;//当前列数
        int currentRowCount = 0;//当前列clip个数

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
