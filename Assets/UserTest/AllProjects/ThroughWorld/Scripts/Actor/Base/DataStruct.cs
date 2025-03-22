/**
   *Copyright(C) 2020 by DefaultCompany
   *All rights reserved.
   *Author:       DESKTOP-AJS8G4U
   *UnityVersion：2021.2.1f1c1
   *创建时间:         2021-12-23
  */

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using cfg.skill;
using static YangTools.Scripts.Core.YangExtend.YangExtend;

namespace DataStruct
{
    /// <summary>
    /// 阵营
    /// </summary>
    [Flags]
    public enum ActorCampType
    {
        [LabelText("未知的")] None = 0,
        [LabelText("玩家")] Player = 1 << 1,
        [LabelText("怪物")] Monster = 1 << 2,
        [LabelText("Npc")] Npc = 1 << 3,
        [LabelText("建筑")] Building = 1 << 4,
        [LabelText("怪物+建筑")] MonsterAndBuilding = Monster + Building,
        [LabelText("玩家+建筑")] PlayerAndBuilding = Player + Building,
    }

    /// <summary>
    /// 攻击信息
    /// </summary>
    public class AtkInfo
    {
        public GameActor sourceActor; //攻击来源
        public GameActor targetActor; //攻击目标
        public DamageInfo damageInfo; //伤害信息
        public EffectInfo atkEffectInfo;
    }

    /// <summary>
    /// 击中伤害信息
    /// </summary>
    public class DamageInfo
    {
        public float damage;
        public Vector3 atkPos; //攻击起点
        public Vector3 behitPos; //受击点
        public EffectInfo beHitEffectInfo; //击中特效
    }

    /// <summary>
    /// 特效信息
    /// </summary>
    [Serializable]
    public class EffectInfo
    {
        public string name;
    }

    /// <summary>
    /// 运行时技能数据
    /// </summary>
    public class RunTimeSkillData : IWeight<RunTimeSkillData>
    {
        public Skill skill;
        public int weight;
        public float currentCD;

        public void ToCd()
        {
            currentCD = skill.Cd;
        }

        public void UpdateCd(float timer)
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
        真实伤害,

        金属性伤害 = 1000,
        木属性伤害,
        水属性伤害,
        火属性伤害,
        土属性伤害,

        普通治疗 = 2000,
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
        Hp,
        Mp,
        Atk,
        Def,
        GuardRang,
        AtkSpeed
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

    /// <summary>
    /// 检测信息
    /// </summary>
    [Serializable]
    public class CheckConfig
    {
        [LabelText("检测类型")] public HitCheckType checkType = HitCheckType.圆形;

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
        public float diameter = 1; //直径

        [ShowIf("@(checkType == HitCheckType.扇形)"), LabelText("角度")]
        public float Angle = 30; //角度

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


        [LabelText("偏移量:左右,上下,前后")] public Vector3 offsetVector;
        [HideInInspector] [LabelText("旋转")] public Vector3 rotate;

        /// <summary>
        /// 直线用
        /// </summary>
        [HideInInspector] public List<Vector3> vector3List = new List<Vector3>();

        public CheckConfig Clone()
        {
            CheckConfig temp = (CheckConfig)this.MemberwiseClone();
            temp.vector3List = this.vector3List.GetClone();
            return temp;
        }
    }

    /// <summary>
    /// 怪物等级
    /// </summary>
    [Flags]
    public enum MonsterLevel
    {
        None,

        /// <summary>
        /// 士兵
        /// </summary>
        Soldier,

        /// <summary>
        /// 勇士
        /// </summary>
        Warrior,

        /// <summary>
        /// 将领
        /// </summary>
        General,

        /// <summary>
        /// 统帅
        /// </summary>
        Leader,
    }

    /// <summary>
    /// 角色属性
    /// </summary>
    public enum RolePropertyType
    {
        #region 基础

        /// <summary>
        /// 血量
        /// </summary>
        Hp = 10000,

        /// <summary>
        /// 魔法值
        /// </summary>
        Mp = 10001,

        /// <summary>
        /// 攻击力
        /// </summary>
        Atk = 10002,

        /// <summary>
        /// 护盾
        /// </summary>
        护盾值 = 10003,

        /// <summary>
        /// 体力
        /// </summary>
        Stamina = 10004,

        /// <summary>
        /// 体力恢复
        /// </summary>
        StaminaRecovery = 10005,

        /// <summary>
        /// 闪避率
        /// </summary>
        Dodge = 10006,

        /// <summary>
        /// 暴击率
        /// </summary>
        CritPro = 10007,

        /// <summary>
        /// 暴击倍数
        /// </summary>
        CritValue = 10008,

        /// <summary>
        /// 暴击抗性
        /// </summary>
        CritResistance = 10009,

        /// <summary>
        /// 视野
        /// </summary>
        ViewRang = 10031,

        /// <summary>
        /// 攻击距离
        /// </summary>
        AtkRange = 10032,

        /// <summary>
        /// 攻击速度
        /// </summary>
        AtkSpeed = 10033,

        /// <summary>
        /// 移动速度
        /// </summary>
        MoveSpeed = 10034,

        /// <summary>
        /// 冷却缩减
        /// </summary>
        CoolingDown = 10035,

        /// <summary>
        /// 技能伤害
        /// </summary>
        SkillDamage = 10036,

        /// <summary>
        /// 减伤
        /// </summary>
        ExtraMinus = 10037,

        #endregion

        #region 属性

        /// <summary>
        /// 金--导电
        /// </summary>
        Metal = 30010,

        /// <summary>
        /// 木--缠绕
        /// </summary>
        Wood = 30020,

        /// <summary>
        /// 水--冰冻
        /// </summary>
        Water = 30030,

        /// <summary>
        /// 火--燃烧
        /// </summary>
        Fire = 30040,

        /// <summary>
        /// 土--石化
        /// </summary>
        Earth = 30050,

        #endregion
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
    }

    /// <summary>
    /// 角色状态
    /// </summary>
    public enum ControlStateType
    {
        None = 0,

        /// <summary>
        /// 正常状态
        /// </summary>
        Normal = 1,
        倒地 = 2,
        禁锢 = 5,
        击飞 = 6,
        眩晕 = 7,
        冰冻 = 8,
        导电 = 9,
        燃烧 = 10,

        /// <summary>
        /// 翻滚
        /// </summary>
        Roll = 10,

        /// <summary>
        /// 霸体
        /// </summary>
        SuperArmor = 20,
        嘲讽 = 21,

        /// <summary>
        /// 无敌
        /// </summary>
        Invincible = 30,
    }

    /// <summary>
    /// 技能触发类型
    /// </summary>
    public enum SkillTriggerType
    {
        None,
        [LabelText("普通攻击")] NormalAtk = 1,
        [LabelText("主动技能")] ActiveSkill = 2,
        [LabelText("被动技能")] PassiveSkill = 3,
        [LabelText("其他")] Other = 4,
    }

    /// <summary>
    /// 技能释放目标
    /// </summary>
    public enum SkillTargetType
    {
        None,
        [LabelText("自身")] Self,
        [LabelText("敌人")] Enemy,
        [LabelText("中立")] Neutrality,
        [LabelText("友军")] Teammate,
        [LabelText("目标点")] Point,
        [LabelText("目标范围")] Rang,
    }
}