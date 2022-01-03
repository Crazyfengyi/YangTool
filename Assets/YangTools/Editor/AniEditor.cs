using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]
[CustomEditor(typeof(Animator), true)]//bool 子类是否可用
public class AniEditor : Editor
{
    //目标组件
    private Animator animator;
    //运行时状态机控制器
    private RuntimeAnimatorController controller;
    //动画片段
    private AnimationClip[] clips;
    //选中的片段下标
    private int curIndex;
    //模拟时间
    private float timer;
    //上一帧时间
    private float lastFrameTime;
    //是在播放中
    private bool isPlaying;

    //收折栏
    static bool _Foldout = true;

    public void OnEnable()
    {
        Init();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        isPlaying = false;
        curIndex = 0;
        timer = 0f;
        animator = target as Animator;
        controller = animator.runtimeAnimatorController;

        if (controller)
        {
            clips = controller.animationClips;
        }
        else
        {
            clips = new AnimationClip[0];
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (clips.Length == 0)
        {
            EditorGUILayout.HelpBox("没有动画片段!", MessageType.Warning);
            return;
        };

        //检查代码块中是否有任何控件被更改
        EditorGUI.BeginChangeCheck();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("===========");
        GUILayout.Label("<color=#00F5FF>动画预览</color>", new GUIStyle() { richText = true, fontStyle = FontStyle.Normal, fontSize = 16 });
        GUILayout.Label("===========");
        GUILayout.Space(10);
        GUILayout.EndHorizontal();


        GUILayout.Label("当前片段：");
        //创建一个通用的弹出式选择字段。
        curIndex = EditorGUILayout.Popup(curIndex, clips.Select(p => p.name).ToArray()); //还原clip状态
        AnimationClip clip = clips[curIndex];

        GUILayout.Label("播放长度：");
        timer = EditorGUILayout.Slider(timer, 0, clip.length);
        string playAniBtnStr = isPlaying ? "暂停" : "播放动画";

        if (GUILayout.Button(playAniBtnStr))
        {
            SetPlayAni(timer >= clip.length);
        }

        if (GUILayout.Button("返回到当前片段第一帧"))
        {
            isPlaying = false;
            timer = 0;
            clip.SampleAnimation(animator.gameObject, timer);
        }

        if (isPlaying)
        {
            timer += Time.realtimeSinceStartup - lastFrameTime;
            if (timer >= clip.length)
            {
                if (clip.isLooping)
                {
                    timer = 0;
                }
                else
                {
                    isPlaying = false;
                    timer = clip.length;
                }
            }
        }

        //重新绘制显示此编辑器的检查器。
        Repaint();
        lastFrameTime = Time.realtimeSinceStartup;
        if (EditorGUI.EndChangeCheck() || isPlaying)
        {
            clip.SampleAnimation(animator.gameObject, timer);
        }

        _Foldout = CommonEditorTool.Foldout(_Foldout, "测试折叠栏");
        if (_Foldout)
        {

        }

        //EditorGUI.Foldout(new Rect(100, 100, 10, 10), true, new GUIContent("Test"));
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="rePlay"></param>
    private void SetPlayAni(bool rePlay)
    {
        if (rePlay)
        {
            timer = 0f;
            isPlaying = true;
        }
        else
        {
            isPlaying = !isPlaying;
        }
    }
}
