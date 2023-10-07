using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("是否需要使用技能")]
[TaskCategory("RoleAI/Conditionals")]
//[TaskIcon("{SkinColor}StartBehaviorTreeIcon.png")]
public class NeedUseSkill : Conditional
{
    private RoleBase role;
    public override void OnStart()
    {
        role = GetComponent<RoleBase>();
    }
    public override TaskStatus OnUpdate()
    {
        if (role && role.SkillControl != null)
        {
            return role.SkillControl.SkillTreeNeedUseSkill ? TaskStatus.Success : TaskStatus.Failure;
        }

        return TaskStatus.Failure;
    }
}