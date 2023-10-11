using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using static DG.Tweening.DOTweenCYInstruction;
using UnityEngine.Playables;

[TaskDescription("��ʼ����ʹ��,���ȴ����")]
[TaskCategory("RoleAI/Motion")]
[TaskIcon("{SkinColor}StartBehaviorTreeIcon.png")]
public class SkillUseAction : Action
{
    private RoleBase role;
    private BehaviorTree skillTree;//������Ϊ��
    private PlayableDirector skillTimeLine;//����ʱ����

    private bool isTimeLine;
    private float timer;
    private bool behaviorComplete;
    public override void OnStart()
    {
        behaviorComplete = false;
        timer = 0;

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
                timer += Time.deltaTime;
                if (timer >= skillTimeLine.duration)
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
        if (role && role.SkillControl != null)
        {
            role.SkillControl.SkillEnd();
            if (role is Monster) ((Monster)role).StartCommonSkillCD();
        }
    }
}