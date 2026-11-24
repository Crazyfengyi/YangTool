using UnityEngine;

/// <summary>
/// 弹幕模式演示 使用独立运行副本避免改写 SO
/// </summary>
public sealed class ProjectilePatternDemoController : MonoBehaviour
{
    [SerializeField] private SkillCaster caster; //施法者
    [SerializeField] private Transform origin; //发射点
    [SerializeField] private Transform[] targets; //演示目标
    [SerializeField] private ProjectileSkillEffect[] presets; //四种示例预设
    [SerializeField] private Font panelFont; //中文面板字体
    [SerializeField] private Camera demoCamera; //为控制面板预留视口
    private ProjectileSkillEffect[] working; //运行配置副本
    private Vector3[] targetStarts; //目标原位
    private int preset, emission, flight; //界面选择
    private bool moving; //目标移动开关
    private readonly string[] emissions = { "平行排列", "全周等分", "扇形", "范围随机", "由外向内", "多排圆环", "发射到环停留" }; //排列名称
    private readonly string[] flights = { "直线", "定点抛物线", "螺旋", "正弦", "追踪", "重力抛射", "圆周", "直飞转圆周", "直去直回", "发射时瞄准" }; //轨迹名称
    private readonly string[] names = { "普通弹幕", "命中换目标", "消亡散射", "沿途派生" }; //预设名称

    /// <summary>
    /// 创建运行配置副本并记录目标位置
    /// </summary>
    private void Start()
    {
        working = new ProjectileSkillEffect[presets.Length];
        for (int i = 0; i < presets.Length; i++) if (presets[i] != null) working[i] = Instantiate(presets[i]);
        targetStarts = new Vector3[targets.Length];
        for (int i = 0; i < targets.Length; i++) if (targets[i] != null) targetStarts[i] = targets[i].position;
    }

    /// <summary>
    /// 按世界时间移动目标便于观察追踪
    /// </summary>
    private void Update()
    {
        if (demoCamera != null) demoCamera.rect = new Rect(PanelWidth / Screen.width + 0.02f, 0,
            0.98f - PanelWidth / Screen.width, 1);
        for (int i = 0; i < targets.Length; i++) if (targets[i] != null)
            targets[i].position = targetStarts[i] + (moving ? Vector3.right * Mathf.Sin(Time.time + i) * 2 : Vector3.zero);
    }

    /// <summary>
    /// 绘制模式切换与发射面板
    /// </summary>
    private void OnGUI()
    {
        if (panelFont != null) GUI.skin.font = panelFont;
        GUILayout.BeginArea(new Rect(8, 16, PanelWidth - 12, 450), GUI.skin.box);
        GUILayout.Label("投射物模式演示");
        if (GUILayout.Button("预设：" + names[preset])) preset = (preset + 1) % 4;
        if (GUILayout.Button("发射：" + emissions[emission])) emission = (emission + 1) % 7;
        if (GUILayout.Button("轨迹：" + flights[flight])) flight = (flight + 1) % 10;
        GUILayout.Label("点击以上按钮循环切换");
        if (emission == 6) GUILayout.Label("到环后停止 优先于飞行轨迹");
        moving = GUILayout.Toggle(moving, "目标移动");
        if (GUILayout.Button("发射", GUILayout.Height(40))) Fire();
        if (GUILayout.Button("清空演示子弹")) Clear();
        for (int i = 0; i < targets.Length; i++) if (targets[i] != null)
        {
            var health = targets[i].GetComponent<SimpleHealth>();
            if (health != null) GUILayout.Label("目标 " + (i + 1) + " 生命：" + health.Current.ToString("0"));
        }
        GUILayout.EndArea();
    }

    private float PanelWidth => Mathf.Min(300, Screen.width * 0.4f);

    /// <summary>
    /// 从场景绑定的发射点执行选定预设
    /// </summary>
    public void Fire()
    {
        if (working == null || preset >= working.Length || working[preset] == null || origin == null) return;
        var effect = working[preset];
        effect.Emission.mode = (ProjectileEmissionMode)emission;
        effect.Flight.mode = (ProjectileFlightMode)flight;
        GameObject target = targets.Length > 0 && targets[0] != null ? targets[0].gameObject : null;
        effect.Execute(new SkillContext { Caster = caster, OriginPoint = origin.position, Direction = origin.forward,
            Target = target, Point = target != null ? target.transform.position : origin.position + origin.forward * 8 });
    }

    /// <summary>
    /// 只清理本演示施法者发射的弹体
    /// </summary>
    private void Clear()
    {
        foreach (var p in FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None)) if (p.BelongsTo(caster)) Destroy(p.gameObject);
    }

    /// <summary>
    /// 释放运行副本
    /// </summary>
    private void OnDestroy()
    {
        if (working != null) foreach (var effect in working) if (effect != null) Destroy(effect);
    }
}
