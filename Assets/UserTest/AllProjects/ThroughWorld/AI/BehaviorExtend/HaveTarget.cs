using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("有无目标")]
[TaskCategory("RoleAI/Conditionals")]
//[TaskIcon("{SkinColor}StartBehaviorTreeIcon.png")]
public class HaveTarget : Conditional
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
            return role.Target ? TaskStatus.Success : TaskStatus.Failure;
        }

        return TaskStatus.Failure;
    }
}