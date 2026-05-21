using UnityEngine;

/// <summary>
/// Buff技能效果类,施加Buff效果到目标角色
/// </summary>
[CreateAssetMenu(menuName = "Game/Skills/Effects/Buff")]
public class BuffSkillEffect : SkillEffect
{
    //Buff定义的序列化字段
    [SerializeField] 
    private BuffDefinition buff;
    //是否对施法者自身施加Buff的布尔标志
    [SerializeField] 
    private bool applyToCaster;

    /// <summary>
    /// 执行Buff技能效果的方法
    /// </summary>
    public override void Execute(SkillContext context)
    {
        // 确定目标对象：如果设置为对施法者有效或没有目标，则目标为施法者；否则为目标
        var target = applyToCaster || context.Target == null
            ? context.Caster.gameObject
            : context.Target;

        // 检查目标是否拥有CharacterStats组件，如果没有则添加
        if (target.GetComponent<CharacterStats>() == null)
        {
            target.AddComponent<CharacterStats>();
        }

        // 获取或添加BuffController组件
        var controller = target.GetComponent<BuffController>();
        if (controller == null)
        {
            controller = target.AddComponent<BuffController>();
        }

        // 对目标应用Buff，传入Buff定义和施法者信息
        controller.ApplyBuff(buff, context.Caster);
    }
}
