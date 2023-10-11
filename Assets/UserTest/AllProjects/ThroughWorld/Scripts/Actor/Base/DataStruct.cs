/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2021-12-23 
*/
using System;
using DataStruct;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YangTools.Extend;
using cfg.skill;
using static YangTools.Extend.YangExtend;

/// <summary>
/// 阵营
/// </summary>
[Flags]
public enum ActorCampType
{
    [LabelText("未知的")]
    None = 0,
    [LabelText("玩家")]
    Player = 1 << 1,
    [LabelText("怪物")]
    Monster = 1 << 2,
    [LabelText("NPC")]
    Npc = 1 << 3,
    [LabelText("建筑")]
    Building = 1 << 4,
    [LabelText("怪物+建筑")]
    MonsterAndBuilding = Monster + Building,
    [LabelText("玩家+建筑")]
    PlayerAndBuilding = Player + Building,
}
/// <summary>
/// 攻击信息
/// </summary>
public class AtkInfo
{
    public GameActor sourceActor;//攻击来源
    public GameActor targetActor;//攻击目标
    public DamageInfo damageInfo;//伤害信息
    public EffectInfo atkEffectInfo;
}
/// <summary>
/// 击中伤害信息
/// </summary>
public class DamageInfo
{
    public float damage;
    public Vector3 atkPos;//攻击起点
    public Vector3 behitPos;//受击点
    public EffectInfo beHitEffectInfo;//击中特效
}
/// <summary>
/// 特效信息
/// </summary>
public class EffectInfo
{

}

/// <summary>
/// 运行时技能数据
/// </summary>
public class RunTimeSkillData : IWeight<RunTimeSkillData>
{
    public Skill skill;
    public int weight;
    public float currentCD;

    public void ToCD()
    {
        currentCD = skill.Cd;
    }

    public void UpdateCD(float timer)
    {
        currentCD -= timer;
        currentCD = Mathf.Max(currentCD, 0);
    }

    public int GetWeight()
    {
        return weight;
    }

    public RunTimeSkillData GetItem()
    {
        return this;
    }
}

/// <summary>
/// 伤害类型
/// </summary>
public enum DamageType
{
    None,
    物理伤害 = 1,
    魔法伤害,
    精神伤害,
    真实伤害,

    金属性伤害 = 1000,
    木属性伤害,
    水属性伤害,
    火属性伤害,
    土属性伤害,

    普通治疗 = 2000,
    护盾减少,
    攻击吸血,
}

/// <summary>
/// 子弹发射类型
/// </summary>
public enum BulletShootType
{
    None,
    Circle,
    Throw,
    Parabola,
    Physics,
    Bomb
}

/// <summary>
/// 子弹类型
/// </summary>
public enum BulletType
{
    None,
    Circle,
    Throw,
    Parabola,
    Physics,
    Bomb
}

/// <summary>
/// 交互类型
/// </summary>
public enum InterActiveType
{
    /// <summary>
    /// 默认
    /// </summary>
    None,
    /// <summary>
    /// 对话
    /// </summary>
    Dialogue,
    /// <summary>
    /// 触发
    /// </summary>
    Trigger,
}
/// <summary>
/// 角色大小类型
/// </summary>
public enum RoleSizeType
{
    None,
    ColliderSize,
    MeshSize,
}
/// <summary>
/// 角色属性
/// </summary>
public enum RoleAttribute
{
    None,
    HP,
    MP,
    Atk,
    Def,
    GuardRang
}
/// <summary>
/// 角色标记
/// </summary>
public enum RoleFlag
{
    None,
    标记1,
}
/// <summary>
/// 子弹死亡类型
/// </summary>
public enum BulletDieType
{
    None,
    /// <summary>
    /// 生存时间结束
    /// </summary>
    TimeEnd,
    /// <summary>
    /// 场景切换
    /// </summary>
    SceneChange,
    /// <summary>
    /// 检测到攻击目标
    /// </summary>
    HaveAtk,
    /// <summary>
    /// 碰撞到建筑物和地面
    /// </summary>
    Collision,
    /// <summary>
    /// 到达目标点
    /// </summary>
    EndPoint,
}

/// <summary>
/// 检测类型
/// </summary>
public enum HitCheckType
{
    None = 0,
    射线 = 1,
    矩形 = 2,
    扇形 = 3,
    圆形 = 4,
    圆环 = 5,
    球形射线 = 6,
}

[Serializable]
/// <summary>
/// 检测信息
/// </summary>
public class CheckConfig
{
    [LabelText("检测类型")]
    public HitCheckType checkType = HitCheckType.圆形;
    //射线
    [ShowIf("@(checkType == HitCheckType.射线)"), LabelText("距离")]
    public float distanceOfLine = 1;

    //矩形
    [ShowIf("@(checkType == HitCheckType.矩形)"), LabelText("长")]
    public float Long = 2;
    [ShowIf("@(checkType == HitCheckType.矩形)"), LabelText("宽")]
    public float Width = 1;
    [ShowIf("@(checkType == HitCheckType.矩形)"), LabelText("高")]
    public float Hight = 1;

    //扇形
    [ShowIf("@(checkType == HitCheckType.扇形)"), LabelText("直径")]
    public float diameter = 1;//直径
    [ShowIf("@(checkType == HitCheckType.扇形)"), LabelText("角度")]
    public float Angle = 30;//角度

    //圆形
    [ShowIf("@(checkType == HitCheckType.圆形)"), LabelText("半径")]
    public float radius = 1;

    //圆环
    [ShowIf("@(checkType == HitCheckType.圆环)"), LabelText("内半径")]
    public float insideRadius = 1;
    [ShowIf("@(checkType == HitCheckType.圆环)"), LabelText("外半径")]
    public float outsideRadius = 2;

    //球形射线
    [ShowIf("@(checkType == HitCheckType.球形射线)"), LabelText("距离")]
    public float distance = 1;
    [ShowIf("@(checkType == HitCheckType.球形射线)"), LabelText("半径")]
    public float radiusOfBallLine = 1;


    [LabelText("偏移量:左右,上下,前后")]
    public Vector3 offsetVector;
    [HideInInspector]
    [LabelText("旋转")]
    public Vector3 rotate;
    /// <summary>
    /// 直线用
    /// </summary>
    [HideInInspector]
    public List<Vector3> vector3List = new List<Vector3>();

    public CheckConfig Clone()
    {
        CheckConfig temp = (CheckConfig)this.MemberwiseClone();
        temp.vector3List = this.vector3List.GetClone();
        return temp;
    }
}



namespace DataStruct
{
    #region 枚举
    /// <summary>
    /// 角色阵营
    /// </summary>
    [Flags]
    public enum RoleCampType
    {
        None = 0,
        /// <summary>
        /// 英雄
        /// </summary>
        Hero = 1 << 0,
        /// <summary>
        /// 怪物
        /// </summary>
        Monster = 1 << 1,
        /// <summary>
        /// 宠物
        /// </summary>
        Pet = 1 << 2,
        /// <summary>
        /// 中立建筑
        /// </summary>
        MiddleBuild = 1 << 3,
        /// <summary>
        /// 所有角色
        /// </summary>
        AllRole = Hero + Monster + Pet,
    }
    /// <summary>
    /// 角色职业
    /// </summary>
    public enum RoleJob
    {
        /// <summary>
        /// 无职业
        /// </summary>
        None,
        /// <summary>
        /// 斧头
        /// </summary>
        Axe = 1,
        /// <summary>
        /// 爪
        /// </summary>
        Claw = 2,
        /// <summary>
        /// 镖
        /// </summary>
        Dart = 3,
        /// <summary>
        /// 棒
        /// </summary>
        Stick = 4
    }
    /// <summary>
    /// 怪物等级
    /// </summary>
    public enum MonsterRank
    {
        None,
        小怪 = 1,
        精英 = 2,
        BOSS = 3,
        守关BOSS = 4
    }
    /// <summary>
    /// 怪物性格
    /// </summary>
    public enum MonsterDisposition
    {
        None = 0,
        胆小 = 1,
        谨慎 = 2,
        勇敢 = 3
    }
    /// <summary>
    /// 宠物性格--和怪物区分,防止更改
    /// </summary>
    public enum PetDisposition
    {
        None = 0,
        胆小 = 1,
        谨慎 = 2,
        勇敢 = 3
    }
    /// <summary>
    /// 角色属性
    /// </summary>
    public enum RolePropertyType
    {
        /// <summary>
        /// 血量
        /// </summary>
        Hp = 10000,
        /// <summary>
        /// 攻击力
        /// </summary>
        Atk = 10001,
        /// <summary>
        /// 护盾
        /// </summary>
        护盾值 = 10002,
        /// <summary>
        /// 耐力(体力)
        /// </summary>
        Stamina = 10003,
        /// <summary>
        /// 耐力恢复
        /// </summary>
        StaminaRecovery = 10004,
        /// <summary>
        /// 闪避率
        /// </summary>
        Dodge = 10005,
        /// <summary>
        /// 暴击率
        /// </summary>
        CritPro = 10006,
        /// <summary>
        /// 暴击伤害倍数增加
        /// </summary>
        CritValue = 10007,
        /// <summary>
        /// 暴击伤害抵抗
        /// </summary>
        CritResistance = 10008,
        /// <summary>
        /// 攻击距离
        /// </summary>
        AtkRange = 10031,
        /// <summary>
        /// 攻击速度
        /// </summary>
        AtkSpeed = 10032,
        /// <summary>
        /// 移动速度
        /// </summary>
        MoveSpeed = 10033,
        /// <summary>
        /// 冷却缩减
        /// </summary>
        CoolingDown = 10034,
        /// <summary>
        /// 技能伤害
        /// </summary>
        SkillDamage = 10035,
        /// <summary>
        /// 检测距离
        /// </summary>
        CheckRang = 10036,
        /// <summary>
        /// 击退时间
        /// </summary>
        Knockback_time = 10037,
        /// <summary>
        /// 击退速度
        /// </summary>
        Knockback_speed = 10038,
        /// <summary>
        /// 击退时间抗性
        /// </summary>
        KnockbackTime_resistance = 10039,
        击退抗性 = 10040,
        击退值 = 10041,
        //==============额外增加===============
        /// <summary>
        /// 额外减伤
        /// </summary>
        ExtraMinus = 20011,
        /// 最终伤害
        /// </summary>
        AtLastHurt = 20021,
        /// <summary>
        /// 最终增伤
        /// </summary>
        ExternalAtk_F = 20023,
        /// <summary>
        /// 最终减伤
        /// </summary>
        ExternalDef_F = 20024,
        //===============属性相关===============
        /// <summary>
        /// 物理攻击(固定值)
        /// </summary>
        PhysicsAtk = 30000,
        /// <summary>
        /// 物理攻击增加(百分比)
        /// </summary>
        PhysicsAtkAdd = 30001,
        /// <summary>
        /// 物理防御
        /// </summary>
        PhysicsDef = 30002,
        /// <summary>
        /// 无视物理防御(百分比)
        /// </summary>
        IgnorePhysicsDef = 30003,
        /// <summary>
        /// 物理防御减伤(百分比)
        /// </summary>
        PhysicsDefMinus = 30004,
        //===========================
        /// <summary>
        /// 火属性攻击(固定值)
        /// </summary>
        FireAtk = 30011,
        /// <summary>
        /// 火属性攻击增加(百分比)
        /// </summary>
        FireAtkAdd = 30012,
        /// <summary>
        /// 火属性防御
        /// </summary>
        FireDef = 30013,
        /// <summary>
        /// 无视火属性防御(百分比)
        /// </summary>
        IgnoreFireDef = 30014,
        /// <summary>
        /// 火属性防御减伤(百分比)
        /// </summary>
        FireDefMinus = 30015,
        //===========================
        /// <summary>
        /// 冰属性(固定值)
        /// </summary>
        IceAtk = 30021,
        /// <summary>
        /// 冰属性攻击增加(百分比)
        /// </summary>
        IceAtkAdd = 30022,
        /// <summary>
        /// 冰属性防御
        /// </summary>
        IceDef = 30023,
        /// <summary>
        /// 无视冰属性防御(百分比)
        /// </summary>
        IgnoreIceDef = 30024,
        /// <summary>
        /// 火属性防御减伤(百分比)
        /// </summary>
        IceDefMinus = 30025,
        //===========================
        /// <summary>
        /// 雷属性攻击(固定值)
        /// </summary>
        ThunderAtk = 30031,
        /// <summary>
        /// 雷属性攻击增加(百分比)
        /// </summary>
        ThunderAtkAdd = 30032,
        /// <summary>
        /// 雷属性防御
        /// </summary>
        ThunderDef = 30033,
        /// <summary>
        /// 无视雷属性防御(百分比)
        /// </summary>
        IgnoreThunderDef = 30034,
        /// <summary>
        /// 火属性防御减伤(百分比)
        /// </summary>
        ThunderDefMinus = 30035,
        //===========================
        /// <summary>
        /// 全元素防御
        /// </summary>
        ElementResist = 30100,
        /// <summary>
        /// 全元素伤害(固定值)
        /// </summary>
        ElementAtk = 30101,
        /// <summary>
        /// 全元素伤害(百分比)
        /// </summary>
        ElementAtkAdd = 30102,
        //============附加信息============
        击中吸血百分比 = 30200,
        击中吸血固定值 = 30201,
        生命恢复效果增加百分比 = 30202,
        生命恢复时额外恢复最大生命值百分比 = 30203,
        普通攻击力增加固定值 = 30204,
        普通攻击力增加百分比 = 30205,
        特殊攻击力增加固定值 = 30206,
        特殊攻击力增加百分比 = 30207,
        最终物理伤害增加百分比 = 30208,
        最终元素伤害增加百分比 = 30209,
        造成基于目标生命百分比的额外伤害 = 30210,
        造成基于目标生命最大值百分比的额外伤害 = 30211,
        根据目标受到的减益BUFF数量增加伤害百分比 = 30212,
        根据目标受到的减益BUFF数量增加暴击几率 = 30213,
    }
    /// <summary>
    /// 角色标记
    /// </summary>
    public enum TagStateType
    {
        /// <summary>
        /// 不受所有伤害
        /// </summary>
        NoHit = 0,
        /// <summary>
        /// 强制保留1血
        /// </summary>
        NoDie = 1,
        /// <summary>
        /// 无法回血
        /// </summary>
        NoRecoverHp = 2,
        击倒起身中,
    }
    /// <summary>
    /// 角色正面状态
    /// </summary>
    public enum ControlStateType
    {
        None = 0,
        /// <summary>
        /// 正常状态
        /// </summary>
        NoBeHit = 5,
        /// <summary>
        /// 翻滚
        /// </summary>
        Roll = 39,
        /// <summary>
        /// 霸体
        /// </summary>
        SuperArmor = 40,
        /// <summary>
        /// 无敌
        /// </summary>
        Invincible = 60
    }
    /// <summary>
    /// 角色负面状态
    /// </summary>
    public enum DeControlStateType
    {
        None = 0,
        受击 = 1,
        击倒 = 6,
        击退 = 7,
        减速 = 8,
        拉动 = 9,
        沉默 = 21,
        禁锢 = 22,
        击飞 = 23,
        麻痹 = 24,
        魅惑 = 26,
        眩晕 = 41,
        冰冻 = 42,
        感电 = 43,
        燃烧 = 44,
        致盲 = 45,
        嘲讽 = 46,
        //下面是程序用
        击倒起身 = 1000,
    }
    /// <summary>
    /// 技能类型
    /// </summary>
    public enum SkillType
    {
        None,
        [LabelText("普通攻击")]
        NormalAttack = 1,
        [LabelText("主动技能")]
        ActiveSkill = 2,
        [LabelText("被动技能")]
        PassiveSkill = 3,
        [LabelText("技能")]
        Skill = 4,
        [LabelText("躲避")]
        Flee = 5,
        [LabelText("其他")]
        Other = 6,
        [LabelText("死亡技能")]
        DieSkill = 7,
    }
    /// <summary>
    /// 所有技能ID(方便查看)
    /// </summary>
    public enum SkillId
    {
        None,
        爪子普攻 = 11001,
        斧头普攻 = 11002,
        null_占位 = 11003,

        主动技能_丢臭臭 = 12001,
        主动技能_暴走 = 12002,
        主动技能_净化 = 12003,
    }
    /// <summary>
    /// buffID
    /// </summary>
    public enum BuffID
    {
        None,
        出血固定值 = 10001,
        出血百分比 = 10002,
        中毒固定值 = 10003,
        中毒百分比 = 10004,
        冰冻固定值 = 10005,
        冰冻百分比 = 10006,
        感电固定值 = 10007,
        感电百分比 = 10008,
        燃烧固定值 = 10009,
        燃烧百分比 = 10010,
        致盲 = 10011,
        晕眩 = 10012,
        沉默 = 10013,
        加速 = 10014,
        减速 = 10015,
        击退 = 10016,
        即死 = 10017,
        技能击退 = 10301,
        技能嘲讽 = 10302,
        技能霸体 = 10303,
        永久霸体 = 10313,
        受击 = 10314,
        击倒 = 10315,
        无敌 = 10316,
        净化 = 20001,
        暴走 = 20002,
        丢臭臭 = 20003,
        角色固定生命攻击防御元素防御 = 20004,
        角色百分比生命攻击防御元素防御 = 20005,
        角色体能上限固定 = 20006,
        角色体能上限百分比 = 20007,
        角色额外生命百分比 = 20008,
        恢复已损生命值 = 30001,
        持续回血百分比 = 30002,
        恢复已损能量 = 30003,
        能量恢复提速 = 30004,
        冷却减少1 = 30005,
        冷却加速1 = 30006,
        血量损失1 = 30007,
        暴击率增加1 = 30008,
        攻速增加1 = 30009,
        强制右位移 = 30010,
        非战斗加速 = 16000001
    }
    /// <summary>
    /// 子弹ID枚举
    /// </summary>
    public enum BulletID
    {
        None,
        斧头 = 1000001,
        斧头低伤 = 1000002,
    }
    /// <summary>
    /// 技能作用目标
    /// </summary>
    public enum SkillTarget
    {
        None,
        敌人,
        中立,
        友军,
    }
    /// <summary>
    /// 技能释放目标
    /// </summary>
    public enum SkillUseTarget
    {
        None,
        自身中心,
        指定点中心
    }
    /// <summary>
    /// Bool标签(角色用字典统一管理)
    /// </summary>
    public enum BoolUseEnum
    {
        None,
        /// <summary>
        /// 是否死亡
        /// </summary>
        IsDie,
        /// <summary>
        /// 是否初始化
        /// </summary>
        IsInit,
        /// <summary>
        /// 是否发现目标
        /// </summary>
        IsDiscoveryRole,
        /// <summary>
        /// 是否可以被选中
        /// </summary>
        IsCanSelect,
        是否强制跟随主人中,
        /// <summary>
        /// 攻击冷却中
        /// </summary>
        IsAttackCooling,
        /// <summary>
        /// 使用技能中
        /// </summary>
        IsUseSkilling,
        /// <summary>
        /// 是否击倒过
        /// </summary>
        HaveBeHitDown,
        /// <summary>
        /// 弃用
        /// </summary>
        NUll_占位,
        /// <summary>
        /// 放回武器中
        /// </summary>
        是否需要播放切换武器动画,
        /// <summary>
        /// 拿出武器中
        /// </summary>
        IsTakeWeapon,
        滑步打断值,
        强制跟随主人中,
        /// <summary>
        /// 是否是巡逻状态
        /// </summary>
        IsPatrol,
        是否要走发现目标流程,
        IsPatrolIdle,
        是否攻击时丢失目标,
        是否移动中,
        是否需要切换职业,
        主角专用_角色是否在战斗中,
        是否需要记录音效和特效,
        怪物是否激活,
        是否潜地中,
        Null占位_11
    }
    /// <summary>
    /// Int标签(角色用字典统一管理)
    /// </summary>
    public enum IntUseEnum
    {
        None,
    }
    /// <summary>
    /// Float标签(角色用字典统一管理)
    /// </summary>
    public enum FloatUseEnum
    {
        None,
        普攻中百分比,
        普攻前摇百分比,
        普攻后摇百分比,
        巡逻待机,
        角色技能公共CD,
        动画斧头特殊动作前固定时间,
        动画斧头特殊动作中固定时间,
        动画斧头特殊动作后固定时间,
        体力恢复_恢复间隔最大值,
        体力恢复_恢复间隔计算值,
        体力恢复_每秒恢复,
        武器拿出动画时间,
        角色技能公共CD最大值,
        战斗中人宠之间的最大距离,
        非战斗中人宠之间的最大距离,
        动画爪子特殊动作前固定时间,
        动画爪子特殊动作中固定时间,
        动画爪子特殊动作后固定时间,
        依赖攻速但手动输入百分比,

        //====下面是特殊用法====不用作float只用作枚举
        使用自定义动画时间 = 99999999,
    }

    /// <summary>
    /// 移动方式
    /// </summary>
    public enum RoleMoveType
    {
        /// <summary>
        /// 自动
        /// </summary>
        NavMeshAgent,
        /// <summary>
        /// 物理运动
        /// </summary>
        PhysicalMove,
        /// <summary>
        /// 不移动
        /// </summary>
        NoMove
    }
    /// <summary>
    /// 移动结束类型
    /// </summary>
    public enum CustomEndType
    {
        None,
        /// <summary>
        /// 回到地面结束移动
        /// </summary>
        OnGround,
        /// <summary>
        /// 时间结束
        /// </summary>
        Time,
        /// <summary>
        /// 速度到0结束-不计算Y轴速度
        /// </summary>
        Speed,
        /// <summary>
        /// 手动结束
        /// </summary>
        Hand,
    }
    #endregion


    //[Serializable]
    ///// <summary>
    ///// 检测信息--这个结构数据类 伤害检查和范围预警都在用
    ///// </summary>
    //public class CheckConfig
    //{
    //    [LabelText("检测类型")]
    //    public HitCheckType checkType = HitCheckType.圆形;
    //    [HideInInspector]
    //    public float time;//预警用
    //    [LabelText("@GetEditorName(0)")]
    //    public float Long = 1;
    //    [ShowIf("IsNeedShow"), LabelText("@GetEditorName(1)")]
    //    public float Width;
    //    [LabelText("偏移量x,y,z")]
    //    public Vector3 offsetVector;
    //    [HideInInspector]
    //    [LabelText("旋转")]
    //    public Vector3 rotate;
    //    /// <summary>
    //    /// 直线用
    //    /// </summary>
    //    [HideInInspector]
    //    public List<Vector3> vector3List = new List<Vector3>();
    //    /// <summary>
    //    /// 根据类型获得名称
    //    /// </summary>
    //    public string GetEditorName(int index)
    //    {
    //        switch (checkType)
    //        {
    //            case HitCheckType.None:
    //            case HitCheckType.扇形:
    //                if (index == 0)
    //                {
    //                    return "直径";
    //                }
    //                else
    //                {
    //                    return "角度";
    //                }
    //            case HitCheckType.矩形:
    //                if (index == 0)
    //                {
    //                    return "长";
    //                }
    //                else
    //                {
    //                    return "宽";
    //                }
    //            case HitCheckType.圆形:
    //                return "半径";
    //            case HitCheckType.圆环:
    //                if (index == 0)
    //                {
    //                    return "内半径";
    //                }
    //                else
    //                {
    //                    return "外半径";
    //                }
    //            case HitCheckType.球形射线:
    //                if (index == 0)
    //                {
    //                    return "距离";
    //                }
    //                else
    //                {
    //                    return "半径";
    //                }
    //            case HitCheckType.射线:
    //                return "距离";
    //        }

    //        return string.Empty;
    //    }
    //    /// <summary>
    //    /// 是否需要显示宽度
    //    /// </summary>
    //    public bool IsNeedShow()
    //    {
    //        switch (checkType)
    //        {
    //            case HitCheckType.射线:
    //            case HitCheckType.圆形:
    //                return false;
    //        }

    //        return true;
    //    }
    //    public CheckConfig Clone()
    //    {
    //        CheckConfig temp = (CheckConfig)this.MemberwiseClone();
    //        temp.vector3List = this.vector3List.GetClone();
    //        return temp;
    //    }
    //}
    //[Serializable]
    ///// <summary>
    ///// 攻击特效信息
    ///// </summary>
    //public class HitEffectInfo
    //{
    //    [HideInEditorMode]
    //    [LabelText("攻击来源点")]
    //    public Vector3 atkerSourcePoint;
    //    [LabelText("特效名字")]
    //    public string effectName;
    //    [LabelText("是否是循环特效")]
    //    public bool isLoop;
    //    [LabelText("是否放角色身上")]
    //    public bool isFollowRole;
    //    [LabelText("特效位置")]
    //    public EffectPointType effectPointType;
    //    /// <summary>
    //    /// 播放特效
    //    /// </summary>
    //    public void ShowEffect(RoleBase role)
    //    {
    //        if (ProjectSettingInfo.IsEditorMode()) return;

    //        if (!IsCanShowEffect()) return;

    //        //放角色身上
    //        if (isFollowRole)
    //        {
    //            Vector3 pos = default;
    //            Transform parent = null;
    //            switch (effectPointType)
    //            {
    //                case EffectPointType.头部:
    //                case EffectPointType.左手:
    //                case EffectPointType.右手:
    //                case EffectPointType.身体:
    //                case EffectPointType.脚底:
    //                    {
    //                        pos = role.roleModelInfo.GetEffectPoint(effectPointType).position;
    //                        parent = role.roleModelInfo.GetEffectPoint(effectPointType);
    //                    }
    //                    break;
    //                case EffectPointType.ComputePoint:
    //                    {
    //                        pos = role.GetHitEffectPoint(atkerSourcePoint);
    //                        parent = role.roleModelInfo.GetEffectPoint(EffectPointType.脚底);
    //                    }
    //                    break;
    //                default:
    //                    break;
    //            }

    //            role.PlayEffectAtSelf(effectName, parent, pos, isLoop);
    //        }
    //        else
    //        {
    //            Vector3 pos = default;
    //            switch (effectPointType)
    //            {
    //                case EffectPointType.头部:
    //                case EffectPointType.左手:
    //                case EffectPointType.右手:
    //                case EffectPointType.身体:
    //                case EffectPointType.脚底:
    //                    {
    //                        pos = role.roleModelInfo.GetEffectPoint(effectPointType).position;
    //                    }
    //                    break;
    //                case EffectPointType.ComputePoint:
    //                    {
    //                        pos = role.GetHitEffectPoint(atkerSourcePoint);
    //                    }
    //                    break;
    //                default:
    //                    break;
    //            }

    //            role.PlayEffect(effectName, pos, isLoop);
    //        }
    //    }
    //    /// <summary>
    //    /// 获得特效播放点
    //    /// </summary>
    //    public Vector3 GetEffectPos(RoleBase role)
    //    {
    //        Vector3 pos = default;
    //        switch (effectPointType)
    //        {
    //            case EffectPointType.头部:
    //            case EffectPointType.左手:
    //            case EffectPointType.右手:
    //            case EffectPointType.身体:
    //            case EffectPointType.脚底:
    //                {
    //                    pos = role.roleModelInfo.GetEffectPoint(effectPointType).position;
    //                }
    //                break;
    //            case EffectPointType.ComputePoint:
    //                {
    //                    pos = role.GetHitEffectPoint(atkerSourcePoint);
    //                }
    //                break;
    //            default:
    //                break;
    //        }
    //        return pos;
    //    }
    //    /// <summary>
    //    /// 是否可以播放特效
    //    /// </summary>
    //    private bool IsCanShowEffect()
    //    {
    //        return !string.IsNullOrEmpty(effectName);
    //    }
    //    /// <summary>
    //    /// 获得克隆
    //    /// </summary>
    //    public HitEffectInfo Clone()
    //    {
    //        return (HitEffectInfo)this.MemberwiseClone();
    //    }
    //}
    ///// <summary>
    ///// 范围攻击检测信息
    ///// </summary>
    //public class AtkCheckInfo
    //{
    //    /// <summary>
    //    /// 范围类型
    //    /// </summary>
    //    public HitCheckType areaDamageType;
    //    /// <summary>
    //    /// 范围(圆形:半径 矩形:长 伞形:直径)
    //    /// </summary>
    //    public float rang = 10f;
    //    /// <summary>
    //    /// 角度(扇形:角度,矩形:宽)
    //    /// </summary>
    //    public float angle = 0f;
    //    /// <summary>
    //    /// 偏移量
    //    /// </summary>
    //    public Vector3 OffsetVector;
    //    /// <summary>
    //    /// 旋转
    //    /// </summary>
    //    public Vector3 Rotate;
    //}
    ///// <summary>
    ///// 攻击信息
    ///// </summary>
    //public class AtkInfo
    //{
    //    public int index;
    //    public Vector3 atkPoint;
    //    [HideInInspector]
    //    public RoleBase target;
    //}

    //[Serializable]
    ///// <summary>
    ///// 伤害数据(未处理的为数据,处理后为信息)
    ///// </summary>
    //public class DamageData
    //{
    //    [SerializeField]
    //    /// <summary>
    //    /// 伤害类型
    //    /// </summary>
    //    public DamageType damageType = DamageType.物理攻击;
    //    /// <summary>
    //    /// 攻击类型
    //    /// </summary>
    //    public AtkType atkType;
    //    /// <summary>
    //    /// 是否播放受击动画--受CD和待机移动影响
    //    /// </summary>
    //    public bool needBeHitAni;
    //    [SerializeField]
    //    /// <summary>
    //    /// 固定值
    //    /// </summary>
    //    public float fixedValue = 0;
    //    [SerializeField]
    //    /// <summary>
    //    /// 基础百分比
    //    /// </summary>
    //    public float basePercent = 1;
    //}
    ///// <summary>
    ///// 攻击额外信息
    ///// </summary>
    //public class AtkShowExtraInfo
    //{
    //    /// <summary>
    //    /// 攻击音效
    //    /// </summary>
    //    public string atkSoundName;
    //    /// <summary>
    //    /// 击中音效
    //    /// </summary>
    //    public string hitSoundName;
    //    /// <summary>
    //    /// 攻击时自身特效
    //    /// </summary>
    //    public HitEffectInfo selfShowEffect;
    //    /// <summary>
    //    /// 命中后目标特效
    //    /// </summary>
    //    public HitEffectInfo targetShowEffect;
    //    /// <summary>
    //    /// buff触发信息
    //    /// </summary>
    //    public BuffTriggerInfo buffTriggerInfo;
    //    /// <summary>
    //    /// BUFF概率触发
    //    /// </summary>
    //    public List<ProbabilityItem> buffOdds = new List<ProbabilityItem>();
    //    /// <summary>
    //    /// 负面状态概率触发
    //    /// </summary>
    //    public List<ProbabilityItem> deStateOdds = new List<ProbabilityItem>();
    //    /// <summary>
    //    /// 击中回调--只调用一次
    //    /// </summary>
    //    public Action hitCallback_OnlyOne;
    //}
    //[Serializable]
    ///// <summary>
    ///// buff触发信息
    ///// </summary>
    //public class BuffTriggerInfo
    //{
    //    [LabelText("BUFF目标")]
    //    public SkillBuffTarget usersBuffTarget;
    //    [LabelText("额外参数")]
    //    public int usersBuffValue;
    //    [LabelText("BUFF信息")]
    //    public List<ProbabilityItem> buffOdds = new List<ProbabilityItem>();
    //}
    ///// <summary>
    ///// TimeLine子弹信息记录
    ///// </summary>
    //public class TimeLineBulletRecord
    //{
    //    public int emitterID;
    //    public int bulletID;
    //    public List<SkillEjectorEventParameter> parmList = new List<SkillEjectorEventParameter>();
    //    public List<Vector3> recordPoint = new List<Vector3>();
    //}
}