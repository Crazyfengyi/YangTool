using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using static DG.Tweening.DOTweenCYInstruction;
using UnityEngine.Playables;

[TaskDescription("开始技能使用,并等待完成")]
[TaskCategory("RoleAI/Motion")]
[TaskIcon("{SkinColor}StartBehaviorTreeIcon.png")]
public class SkillUseAction : Action
{
    private RoleBase role;
    private BehaviorTree skillTree;//技能行为树
    private PlayableDirector skillTimeLine;//技能时间线

    private bool isTimeLine;
    private bool behaviorComplete;
    public override void OnStart()
    {
        behaviorComplete = false;

        role = gameObject.GetComponent<RoleBase>();
        skillTree = gameObject.GetComponents<BehaviorTree>()[1];
        skillTimeLine = gameObject.GetComponent<PlayableDirector>();

        isTimeLine = role.SkillControl.IsTimeLine;

        if (isTimeLine)
        {
            isTimeLine = true;
            skillTimeLine.Play();
        }
        else
        {
            skillTree.OnBehaviorEnd += BehaviorEnded;
            skillTree.EnableBehavior();
        }

        if (role && role.SkillControl != null)
        {
            role.SkillControl.NeedUseSkill = false;
        }
    }
    public override void OnEnd()
    {
        if (skillTree != null)
        {
            skillTree.OnBehaviorEnd -= BehaviorEnded;
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (skillTree == null)
        {
            return TaskStatus.Failure;
        }

        if (!behaviorComplete)
        {
            if (isTimeLine)
            {
                if (Mathf.Abs((float)(skillTimeLine.time - skillTimeLine.duration)) < 0.01f)
                {
                    BehaviorEnded(null);
                    return TaskStatus.Success;
                }
            }
            return TaskStatus.Running;
        }

        return TaskStatus.Success;
    }

    void BehaviorEnded(Behavior behavior)
    {
        behaviorComplete = true;
    }
}