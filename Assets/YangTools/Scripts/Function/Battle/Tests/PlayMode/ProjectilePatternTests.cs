using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

/// <summary>
/// 弹幕排列 轨迹与生命周期回归验证
/// </summary>
public class ProjectilePatternTests
{
    private readonly List<Object> created = new List<Object>(); //测试资源
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic; //反射范围

    /// <summary>
    /// 枚举所有排列和轨迹组合
    /// </summary>
    public static IEnumerable Combinations()
    {
        foreach (ProjectileEmissionMode e in Enum.GetValues(typeof(ProjectileEmissionMode)))
        foreach (ProjectileFlightMode f in Enum.GetValues(typeof(ProjectileFlightMode))) yield return new TestCaseData(e, f);
    }

    /// <summary>
    /// 清理测试实例与派生弹体
    /// </summary>
    [TearDown]
    public void Cleanup()
    {
        foreach (var p in Object.FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None))
            if (p.name.StartsWith("ProjectileTest")) Object.DestroyImmediate(p.gameObject);
        foreach (var o in created) if (o != null) Object.DestroyImmediate(o);
        created.Clear();
    }

    /// <summary>
    /// 创建不依赖正式预制体的测试效果
    /// </summary>
    private ProjectileSkillEffect Effect()
    {
        var go = new GameObject("ProjectileTest");
        go.transform.position = new Vector3(1000, 1000, 1000);
        var collider = go.AddComponent<SphereCollider>(); collider.isTrigger = true;
        var p = go.AddComponent<SkillProjectile>();
        var effect = ScriptableObject.CreateInstance<ProjectileSkillEffect>();
        created.Add(effect);
        Set(effect, "projectilePrefab", p); Set(effect, "targetMask", (LayerMask)0);
        return effect;
    }

    /// <summary>
    /// 读写私有配置以验证既有公开施放入口
    /// </summary>
    private static void Set(object value, string name, object field) => value.GetType().GetField(name, Flags).SetValue(value, field);
    private static T Get<T>(object value, string name) => (T)value.GetType().GetField(name, Flags).GetValue(value);

    /// <summary>
    /// 所有组合均能产生合法轨迹并保持配置不变
    /// </summary>
    [TestCaseSource(nameof(Combinations))]
    public void AllCombinationsHaveFiniteMotion(ProjectileEmissionMode emission, ProjectileFlightMode flight)
    {
        var effect = Effect();
        var e = Get<ProjectileEmissionSettings>(effect, "emission"); e.mode = emission; e.count = 3; e.rows = 2;
        Get<ProjectileFlightSettings>(effect, "flight").mode = flight;
        string before = JsonUtility.ToJson(effect);
        effect.Execute(new SkillContext { OriginPoint = Vector3.zero, Point = Vector3.forward * 8, Direction = Vector3.forward });
        int count = 0;
        foreach (var p in Object.FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None))
        {
            if (!p.name.EndsWith("(Clone)")) continue;
            count++;
            object motion = Get<object>(p, "motion");
            var step = motion.GetType().GetMethod("Step", Flags);
            var pos = motion.GetType().GetProperty("Position", Flags);
            for (int i = 1; i <= 180; i++)
            {
                step.Invoke(motion, new object[] { i / 60f, 1 / 60f });
                Vector3 v = (Vector3)pos.GetValue(motion);
                Assert.IsFalse(float.IsNaN(v.x) || float.IsInfinity(v.x) || float.IsNaN(v.y) || float.IsInfinity(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.z));
            }
        }
        Assert.AreEqual(emission == ProjectileEmissionMode.CircleRow || emission == ProjectileEmissionMode.LaunchCircleRow ? 6 : 3, count);
        Assert.AreEqual(before, JsonUtility.ToJson(effect));
    }

    /// <summary>
    /// 自引用消亡链受生成预算限制
    /// </summary>
    [UnityTest]
    public IEnumerator DeathChainHonorsBudget()
    {
        var effect = Effect(); Set(effect, "lifeSeconds", 0.01f);
        var life = Get<ProjectileLifecycleSettings>(effect, "lifecycle");
        life.onDeath.Add(effect); life.maxDepth = 20; life.maxSpawnCount = 3;
        effect.Execute(new SkillContext { Direction = Vector3.forward });
        yield return new WaitForSeconds(0.2f);
        int clones = 0;
        foreach (var p in Object.FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None)) if(p.name.EndsWith("(Clone)")) clones++;
        Assert.AreEqual(0, clones);
    }

    /// <summary>
    /// 飞行节点在有限寿命中均匀触发
    /// </summary>
    [UnityTest]
    public IEnumerator NodeEffectsRunOncePerInternalBoundary()
    {
        var effect = Effect(); Set(effect, "lifeSeconds", 0.12f);
        var counter = ScriptableObject.CreateInstance<ProjectileCounterEffect>(); created.Add(counter);
        var life = Get<ProjectileLifecycleSettings>(effect, "lifecycle"); life.segments = 4; life.onNode.Add(counter); life.onDeath.Add(counter);
        effect.Execute(new SkillContext { Direction = Vector3.forward });
        yield return new WaitForSeconds(0.25f);
        Assert.AreEqual(4, counter.Count);
    }

    /// <summary>
    /// 无限寿命不运行节点 外部销毁不触发消亡
    /// </summary>
    [UnityTest]
    public IEnumerator ExternalDestructionDoesNotRunLifecycle()
    {
        var effect = Effect(); Set(effect, "lifeSeconds", 0f);
        var counter = ScriptableObject.CreateInstance<ProjectileCounterEffect>(); created.Add(counter);
        var life = Get<ProjectileLifecycleSettings>(effect, "lifecycle"); life.segments = 4; life.onNode.Add(counter); life.onDeath.Add(counter);
        effect.Execute(new SkillContext { Direction = Vector3.forward });
        yield return null;
        foreach (var p in Object.FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None)) if(p.name.EndsWith("(Clone)")) Object.Destroy(p.gameObject);
        yield return null;
        Assert.AreEqual(0, counter.Count);
    }
    /// <summary>
    /// 验证帧率变化下解析轨迹一致且不发生首帧跳位
    /// </summary>
    [TestCase(ProjectileFlightMode.Linear)]
    [TestCase(ProjectileFlightMode.Sine)]
    [TestCase(ProjectileFlightMode.Circle)]
    [TestCase(ProjectileFlightMode.Orbit)]
    [TestCase(ProjectileFlightMode.Lobbed)]
    [TestCase(ProjectileFlightMode.FlyAwayAndBack)]
    [TestCase(ProjectileFlightMode.LaunchCircle)]
    public void AnalyticPathsAreFrameRateIndependent(ProjectileFlightMode flight)
    {
        var effect = Effect(); Get<ProjectileFlightSettings>(effect, "flight").mode = flight;
        effect.Execute(new SkillContext { Direction = Vector3.forward, Point = Vector3.forward * 10 });
        object motion = null;
        foreach(var p in Object.FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None))
            if(p.name.EndsWith("(Clone)")) motion = Get<object>(p, "motion");
        Assert.NotNull(motion);
        var method = motion.GetType().GetMethod("Step", Flags);
        var property = motion.GetType().GetProperty("Position", Flags);
        method.Invoke(motion, new object[]{0f, 0f});
        Assert.Less(((Vector3)property.GetValue(motion)).magnitude, 0.0001f);
        for(int i=1;i<=30;i++) method.Invoke(motion,new object[]{i/30f,1/30f});
        Vector3 first=(Vector3)property.GetValue(motion);
        method.Invoke(motion,new object[]{0f,0f});
        for(int i=1;i<=120;i++) method.Invoke(motion,new object[]{i/120f,1/120f});
        Assert.Less(Vector3.Distance(first,(Vector3)property.GetValue(motion)),0.001f);
    }

    /// <summary>
    /// 多碰撞体只伤害一次 穿透耗尽触发一次消亡
    /// </summary>
    [Test]
    public void HitDeduplicationAndPiercingHaveCorrectOrder()
    {
        var effect=Effect(); Set(effect,"targetMask",(LayerMask)(~0)); Set(effect,"piercing",1);
        var counter=ScriptableObject.CreateInstance<ProjectileCounterEffect>();created.Add(counter);
        var life=Get<ProjectileLifecycleSettings>(effect,"lifecycle");life.onHit.Add(counter);life.onDeath.Add(counter);
        effect.Execute(new SkillContext{Direction=Vector3.forward});
        SkillProjectile bullet=null;
        foreach(var p in Object.FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None))if(p.name.EndsWith("(Clone)"))bullet=p;
        var a=new GameObject("TargetA");created.Add(a);var health=a.AddComponent<SimpleHealth>();var c=a.AddComponent<BoxCollider>();
        var child=new GameObject("ExtraCollider");child.transform.parent=a.transform;var c2=child.AddComponent<SphereCollider>();
        var method=typeof(SkillProjectile).GetMethod("ProcessHit",Flags);
        method.Invoke(bullet,new object[]{c,Vector3.zero});method.Invoke(bullet,new object[]{c2,Vector3.zero});
        Assert.AreEqual(90,health.Current);Assert.AreEqual(1,counter.Count);
        var b=new GameObject("TargetB");created.Add(b);var healthB=b.AddComponent<SimpleHealth>();var cb=b.AddComponent<BoxCollider>();
        method.Invoke(bullet,new object[]{cb,Vector3.zero});
        Assert.AreEqual(90,healthB.Current);Assert.AreEqual(3,counter.Count);
        method.Invoke(bullet,new object[]{cb,Vector3.zero});Assert.AreEqual(3,counter.Count);
    }

    /// <summary>
    /// 匀减速到零后停留而不倒飞
    /// </summary>
    [Test]
    public void NegativeAccelerationStopsWithoutReversing()
    {
        var effect=Effect();Set(effect,"speed",10f);Get<ProjectileFlightSettings>(effect,"flight").acceleration=-10;
        effect.Execute(new SkillContext{Direction=Vector3.forward});
        foreach(var p in Object.FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None))if(p.name.EndsWith("(Clone)"))
        {
            var motion=Get<object>(p,"motion");motion.GetType().GetMethod("Step",Flags).Invoke(motion,new object[]{5f,5f});
            var position=(Vector3)motion.GetType().GetProperty("Position",Flags).GetValue(motion);
            Assert.AreEqual(5f,position.z,0.001f);
        }
    }

    /// <summary>
    /// 派生子弹继承会话并避开本链历史目标
    /// </summary>
    [Test]
    public void HitChainRetargetsAndPreservesSourceContext()
    {
        var effect=Effect();Set(effect,"targetMask",(LayerMask)(~0));
        Get<ProjectileLifecycleSettings>(effect,"lifecycle").onHit.Add(effect);
        var skill=ScriptableObject.CreateInstance<SkillDefinition>();created.Add(skill);
        var session=(SkillCastSession)Activator.CreateInstance(typeof(SkillCastSession),Flags,null,new object[]{7,skill},null);
        var a=new GameObject("TargetA");created.Add(a);a.transform.position=Vector3.forward*2;
        var healthA=a.AddComponent<SimpleHealth>();var colliderA=a.AddComponent<BoxCollider>();
        var b=new GameObject("TargetB");created.Add(b);b.transform.position=Vector3.forward*5;
        var healthB=b.AddComponent<SimpleHealth>();var colliderB=b.AddComponent<BoxCollider>();
        Physics.SyncTransforms();
        effect.Execute(new SkillContext{Skill=skill,Session=session,SourceInstanceId=123,Direction=Vector3.forward,Target=a});
        var hit=typeof(SkillProjectile).GetMethod("ProcessHit",Flags);
        SkillProjectile parent=null;
        foreach(var p in Object.FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None))if(p.name.EndsWith("(Clone)"))parent=p;
        hit.Invoke(parent,new object[]{colliderA,a.transform.position});
        SkillProjectile child=null;
        foreach(var p in Object.FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None))
        {
            var runtime=Get<object>(p,"data");
            if(runtime!=null && Get<int>(runtime,"depth")==1)child=p;
        }
        Assert.NotNull(child);
        var context=Get<SkillContext>(Get<object>(child,"data"),"context");
        Assert.AreSame(session,context.Session);Assert.AreSame(skill,context.Skill);Assert.AreEqual(123,context.SourceInstanceId);
        Assert.AreSame(b,context.Target);
        hit.Invoke(child,new object[]{colliderA,a.transform.position});Assert.AreEqual(90,healthA.Current);
        hit.Invoke(child,new object[]{colliderB,b.transform.position});Assert.AreEqual(90,healthB.Current);
    }

}

/// <summary>
/// 记录测试生命周期次数
/// </summary>
public sealed class ProjectileCounterEffect : SkillEffect
{
    public int Count; //执行次数
    /// <summary>
    /// 累计执行次数
    /// </summary>
    public override void Execute(SkillContext context) { Count++; }
}
