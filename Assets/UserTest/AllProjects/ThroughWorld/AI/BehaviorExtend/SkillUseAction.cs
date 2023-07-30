using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using static DG.Tweening.DOTweenCYInstruction;


[TaskDescription("��ʼ����ʹ��,���ȴ����")]
[TaskIcon("{SkinColor}StartBehaviorTreeIcon.png")]
public class SkillUseAction : Action
{
    private bool behaviorComplete;
    private Behavior behavior;

    private RoleBase role;
    public override void OnStart()
    {
        role = gameObject.GetComponent<RoleBase>();
        if (role && role.SkillControl != null) role.SkillControl.SkillTreeNeedUseSkill = false;
        //TODO:��Դ����
        behavior = gameObject.GetComponents<Behavior>()[1];
        behaviorComplete = false;

        behavior.OnBehaviorEnd += BehaviorEnded;
        behavior.EnableBehavior();
    }
    public override void OnEnd()
    {
        if (behavior != null)
        {
            behavior.OnBehaviorEnd -= BehaviorEnded;
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (behavior == null)
        {
            return TaskStatus.Failure;
        }

        if (!behaviorComplete)
        {
            return TaskStatus.Running;
        }

        return TaskStatus.Success;
    }

    void BehaviorEnded(Behavior behavior)
    {
        behaviorComplete = true;
    }
}