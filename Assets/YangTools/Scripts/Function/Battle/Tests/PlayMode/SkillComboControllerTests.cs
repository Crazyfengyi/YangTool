using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// 技能施放会话和线性连击控制器的运行时测试。
/// </summary>
public class SkillComboControllerTests
{
    private readonly List<Object> createdObjects = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            if (createdObjects[i] != null)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }
        }
        createdObjects.Clear();
    }

    /// <summary>
    /// 验证即时技能不会让施法器残留在施法状态。
    /// </summary>
    [Test]
    public void InstantSkillDoesNotRemainCasting()
    {
        SkillCaster caster = CreateCaster(out _);
        SkillDefinition skill = CreateSkill("Instant", 0f);
        caster.AddSkill(skill);

        bool result = caster.TryCast(CreateSelfRequest(skill), out SkillCastSession session);

        Assert.IsTrue(result);
        Assert.IsNotNull(session);
        Assert.IsTrue(session.IsCompleted);
        Assert.IsFalse(caster.IsCasting);
    }

    /// <summary>
    /// 验证连击按顺序执行，并在末段恢复窗口结束后允许重新起手。
    /// </summary>
    [UnityTest]
    public IEnumerator ComboRunsInOrderAndHonorsFinalRecovery()
    {
        CreateComboController(
            CreateCombo(0.25f,
                CreateStep(CreateSkill("Step1", 0f), 0.2f),
                CreateStep(CreateSkill("Step2", 0f), 0.05f)),
            out SkillComboController controller);

        var startedSteps = new List<int>();
        int completedCount = 0;
        controller.OnStepStarted += (index, _) => startedSteps.Add(index);
        controller.OnComboCompleted += _ => completedCount++;

        Assert.IsTrue(controller.TryTrigger());
        Assert.IsTrue(controller.TryTrigger());
        CollectionAssert.AreEqual(new[] { 0, 1 }, startedSteps);
        Assert.AreEqual(1, completedCount);
        Assert.IsFalse(controller.TryTrigger());

        yield return new WaitForSeconds(0.07f);

        Assert.IsFalse(controller.IsComboActive);
        Assert.IsTrue(controller.TryTrigger());
        Assert.AreEqual(0, controller.CurrentStepIndex);
    }

    /// <summary>
    /// 验证施法期间的短时输入会在技能完成后自动消费。
    /// </summary>
    [UnityTest]
    public IEnumerator BufferedInputIsConsumedAfterCast()
    {
        CreateComboController(
            CreateCombo(0.25f,
                CreateStep(CreateSkill("SlowStep", 0.05f), 0.2f),
                CreateStep(CreateSkill("FollowStep", 0f), 0.1f)),
            out SkillComboController controller);

        Assert.IsTrue(controller.TryTrigger());
        Assert.IsTrue(controller.TryTrigger());
        Assert.IsTrue(controller.HasBufferedInput);

        yield return new WaitForSeconds(0.07f);

        Assert.AreEqual(1, controller.CurrentStepIndex);
        Assert.IsFalse(controller.HasBufferedInput);
    }

    /// <summary>
    /// 验证过期缓存不会在技能完成后自动接续。
    /// </summary>
    [UnityTest]
    public IEnumerator ExpiredBufferedInputIsDiscarded()
    {
        CreateComboController(
            CreateCombo(0.01f,
                CreateStep(CreateSkill("SlowStep", 0.05f), 0.2f),
                CreateStep(CreateSkill("FollowStep", 0f), 0.1f)),
            out SkillComboController controller);

        Assert.IsTrue(controller.TryTrigger());
        Assert.IsTrue(controller.TryTrigger());

        yield return new WaitForSeconds(0.07f);

        Assert.AreEqual(0, controller.CurrentStepIndex);
        Assert.IsFalse(controller.HasBufferedInput);
    }

    /// <summary>
    /// 验证要求命中的步骤会等待异步命中，并在窗口内消费缓存输入。
    /// </summary>
    [UnityTest]
    public IEnumerator RequiredHitAllowsContinuationAfterDelayedReport()
    {
        GameObject target = Track(new GameObject("Target"));
        SkillDefinition hitSkill = CreateSkill("HitStep", 0f, SkillTargetType.Unit);
        hitSkill.Range = 10f;
        var delayedHit = Track(ScriptableObject.CreateInstance<DelayedHitSkillEffect>());
        delayedHit.Target = target;
        delayedHit.Delay = 0.04f;
        hitSkill.Effects.Add(delayedHit);

        CreateComboController(
            CreateCombo(0.2f,
                CreateStep(hitSkill, 0.2f, true),
                CreateStep(CreateSkill("FollowStep", 0f), 0.1f)),
            out SkillComboController controller);

        Assert.IsTrue(controller.TryTrigger(target));
        Assert.IsTrue(controller.TryTrigger(target));
        Assert.AreEqual(0, controller.CurrentStepIndex);

        yield return new WaitForSeconds(0.07f);

        Assert.AreEqual(1, controller.CurrentStepIndex);
    }

    /// <summary>
    /// 验证下一段因冷却施放失败时会重置连击。
    /// </summary>
    [Test]
    public void FailedNextStepResetsCombo()
    {
        SkillDefinition sharedSkill = CreateSkill("Shared", 0f);
        sharedSkill.Cooldown = 1f;
        CreateComboController(
            CreateCombo(0.25f,
                CreateStep(sharedSkill, 0.2f),
                CreateStep(sharedSkill, 0.2f)),
            out SkillComboController controller);

        int resetCount = 0;
        controller.OnComboReset += _ => resetCount++;

        Assert.IsTrue(controller.TryTrigger());
        Assert.IsFalse(controller.TryTrigger());
        Assert.IsFalse(controller.IsComboActive);
        Assert.AreEqual(1, resetCount);
    }

    /// <summary>
    /// 验证连击窗口中的外部技能施放会重置当前连击。
    /// </summary>
    [Test]
    public void ExternalCastResetsActiveCombo()
    {
        SkillCaster caster = CreateComboController(
            CreateCombo(0.25f, CreateStep(CreateSkill("ComboStep", 0f), 0.2f)),
            out SkillComboController controller);
        SkillDefinition externalSkill = CreateSkill("External", 0f);
        caster.AddSkill(externalSkill);

        Assert.IsTrue(controller.TryTrigger());
        Assert.IsTrue(controller.IsComboActive);
        Assert.IsTrue(caster.TryCast(CreateSelfRequest(externalSkill)));
        Assert.IsFalse(controller.IsComboActive);
    }

    private SkillCaster CreateComboController(SkillComboDefinition combo, out SkillComboController controller)
    {
        SkillCaster caster = CreateCaster(out GameObject owner);
        controller = owner.AddComponent<SkillComboController>(); 
        controller.SetCombo(combo);
        return caster;
    }

    private SkillCaster CreateCaster(out GameObject owner)
    {
        owner = Track(new GameObject("SkillCasterTest"));
        return owner.AddComponent<SkillCaster>();
    }

    private SkillDefinition CreateSkill(string id, float castTime, SkillTargetType targetType = SkillTargetType.Self)
    {
        SkillDefinition skill = Track(ScriptableObject.CreateInstance<SkillDefinition>());
        skill.Id = id;
        skill.TargetType = targetType;
        skill.CastTime = castTime;
        skill.Cooldown = 0f;
        return skill;
    }

    private SkillComboDefinition CreateCombo(float inputBufferDuration, params SkillComboStep[] steps)
    {
        SkillComboDefinition combo = Track(ScriptableObject.CreateInstance<SkillComboDefinition>());
        SetPrivateField(combo, "inputBufferDuration", inputBufferDuration);
        SetPrivateField(combo, "steps", new List<SkillComboStep>(steps));
        return combo;
    }

    private static SkillComboStep CreateStep(SkillDefinition skill, float window, bool requireHit = false)
    {
        var step = new SkillComboStep();
        SetPrivateField(step, "skill", skill);
        SetPrivateField(step, "nextStepWindow", window);
        SetPrivateField(step, "requireHit", requireHit);
        return step;
    }

    private static SkillCastRequest CreateSelfRequest(SkillDefinition skill)
    {
        return new SkillCastRequest
        {
            Skill = skill,
            Direction = Vector3.forward
        };
    }

    private T Track<T>(T instance) where T : Object
    {
        createdObjects.Add(instance);
        return instance;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, $"Missing field: {fieldName}");
        field.SetValue(target, value);
    }

    /// <summary>
    /// 在指定延迟后模拟投射物一类的异步命中回报。
    /// </summary>
    private sealed class DelayedHitSkillEffect : SkillEffect
    {
        public GameObject Target;
        public float Delay;

        public override void Execute(SkillContext context)
        {
            context.Caster.StartCoroutine(ReportHit(context));
        }

        private IEnumerator ReportHit(SkillContext context)
        {
            yield return new WaitForSeconds(Delay);
            context.ReportHit(Target);
        }
    }
}
