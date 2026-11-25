/*
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-11-15 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using System.Collections.Generic;
using System.Linq;

namespace YangToolMageEngineer
{
    /// <summary>
    /// 法师工程塔
    /// </summary>
    public class MageEngineer
    {
        // 负责顺序消费法术的示例射击系统
        private ShooterSystem shooter;
        /// <summary>
        /// 构建内置的示例法术序列并初始化射击系统
        /// </summary>
        public MageEngineer()
        {
            shooter = new ShooterSystem();
            List<Spells> allWordList = new List<Spells>();
            allWordList.Add(new Spells()
            {
                name = "常驻BUFF",
                type = SpellsType.LongBuff
            });
            allWordList.Add(new Spells()
            {
                name = "一次性BUFF1",
                type = SpellsType.OnceBuff
            });
            allWordList.Add(new Spells()
            {
                name = "多重施法",
                type = SpellsType.OnceBuff,
                buffType = BuffType.MultipleCast,
                MultipleCastCount = 1,
            });
            allWordList.Add(new Spells()
            {
                name = "包装器",
                type = SpellsType.Bullet,
                buttleType = BuletsType.Wrapper
            });
            allWordList.Add(new Spells()
            {
                name = "子弹2",
                type = SpellsType.Bullet
            });
            allWordList.Add(new Spells()
            {
                name = "子弹3",
                type = SpellsType.Bullet
            });
            allWordList.Add(new Spells()
            {
                name = "一次性BUFF2",
                type = SpellsType.OnceBuff
            });
            allWordList.Add(new Spells()
            {
                name = "子弹4",
                type = SpellsType.Bullet
            });
            shooter.Init(allWordList);
        }

        /// <summary>
        /// 委托示例射击系统读取并触发下一枚逻辑子弹
        /// </summary>
        public void Shoot()
        {
            Debug.LogError("使用法术");
            shooter.Shoot();
        }

    }

    /// <summary>
    /// 顺序消费示例法术并组合常驻增益 一次性增益与多重施法
    /// </summary>
    public class ShooterSystem
    {
        // 示例使用的完整法术序列
        public List<Spells> AllWordList = new List<Spells>();
        // 当前累计的常驻增益法术
        public List<Spells> LongBuff = new List<Spells>();

        /// <summary>
        /// 保存参与模拟射击的法术序列
        /// </summary>
        /// <param name="allWordList">按顺序处理的法术列表</param>
        public void Init(List<Spells> allWordList)
        {
            AllWordList = allWordList;
            LongBuff = AllWordList.Where((item) => { return item.type == SpellsType.LongBuff; }).ToList();
        }

        /// <summary>
        /// 取得下一枚逻辑子弹并调用其开始钩子
        /// </summary>
        public Bullet Shoot()
        {
            Bullet bullet = GetBullet();
            bullet.OnStart();
            return bullet;
        }

        //多重施法
        int MultipleCast = 0;

        /// <summary>
        /// 发射
        /// </summary>
        private Bullet GetBullet()
        {
            List<Spells> longBuff = new List<Spells>();
            List<Spells> onceBuff = new List<Spells>();

            for (int i = 0; i < AllWordList.Count; i++)
            {
                var item = AllWordList[i];
                switch (item.type)
                {
                    case SpellsType.LongBuff:
                        if (item.CanUse)
                        {
                            item.Use();
                            longBuff.Add(item);
                        }
                        break;
                    case SpellsType.OnceBuff:
                        if (item.CanUse)
                        {
                            item.Use();
                            onceBuff.Add(item);
                            //多重施法
                            if (item.buffType == BuffType.MultipleCast)
                            {
                                MultipleCast = item.MultipleCastCount;
                            }
                        }
                        break;
                    case SpellsType.Bullet:
                        if (item.CanUse)
                        {
                            item.Use();
                            Bullet temp = new Bullet();

                            //包装器
                            if (item.buttleType == BuletsType.Wrapper)
                            {
                                temp.name = item.name;
                                temp.buffList = new List<Spells>(longBuff);
                                temp.buffList.AddRange(onceBuff);
                                temp.buttleType = item.buttleType;
                                temp.subBullet = GetBullet();

                                bool isLast = CheckIsLast(i);
                                if (isLast)
                                {
                                    MultipleCast = 0;
                                }
                                else
                                {
                                    //多重施法检测
                                    Multiple();
                                }
                                return temp;
                            }
                            else
                            {
                                temp.name = item.name;
                                temp.buttleType = item.buttleType;

                                //每次只使用一个子弹.
                                bool isLast = CheckIsLast(i);
                                if (isLast)
                                {
                                    MultipleCast = 0;
                                }
                                else
                                {
                                    //多重施法检测
                                    Multiple();
                                }
                                return temp;
                            }
                        }
                        break;
                }
            }

            Reset();
            MultipleCast = 0;
            return null;

            /// <summary>
            /// 检查序列末尾或后续无可用子弹时是否需要重置使用状态
            /// </summary>
            bool CheckIsLast(int index)
            {
                //最后一个重置--TODO:进入冷却
                if (index == AllWordList.Count - 1)
                {
                    Reset();
                    return true;
                }

                //预测下一次使用--确保后面有子弹类型的法术
                (List<Spells> allSpells, List<Spells> bulletList) list = Forecast();
                if (list.bulletList.Count <= 0)
                {
                    Reset();
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 消费多重施法次数并触发下一枚逻辑子弹
            /// </summary>
            void Multiple()
            {
                if (MultipleCast > 0)
                {
                    MultipleCast--;
                    var bullet = GetBullet();
                    bullet.OnStart();
                }
            }
        }

        // 预留的多重施法次数缓存
        private int TempMultipleCast = 0;

        /// <summary>
        /// 预测下一次使用
        /// </summary>
        public (List<Spells> allSpells, List<Spells> bulletList) Forecast()
        {
            List<Spells> allWord = new List<Spells>();
            List<Spells> longBuff = new List<Spells>();
            List<Spells> onceBuff = new List<Spells>();
            List<Spells> bulletList = new List<Spells>();

            for (int i = 0; i < AllWordList.Count; i++)
            {
                var item = AllWordList[i];
                switch (item.type)
                {
                    case SpellsType.LongBuff:
                        if (item.CanUse)
                        {
                            allWord.Add(item);
                            longBuff.Add(item);
                        }
                        break;
                    case SpellsType.OnceBuff:
                        if (item.CanUse)
                        {
                            allWord.Add(item);
                            onceBuff.Add(item);
                        }
                        break;
                    case SpellsType.Bullet:
                        if (item.CanUse)
                        {
                            allWord.Add(item);
                            //包装器
                            if (item.buttleType == BuletsType.Wrapper)
                            {
                                bulletList.Add(item);
                            }
                            else
                            {
                                bulletList.Add(item);
                                //每次只使用一个子弹
                                return (allWord, bulletList);
                            }
                        }
                        break;
                }
            }

            return (allWord, bulletList);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < AllWordList.Count; i++)
            {
                AllWordList[i].ResetUse();
            }
        }
    }

    /// <summary>
    /// 保存示例逻辑子弹与增益数据 并提供空的生命周期扩展钩子
    /// </summary>
    public class Bullet
    {
        // 示例法术或逻辑子弹名称
        public string name;
        // 示例逻辑子弹类别
        public BuletsType buttleType;
        // 附着在逻辑子弹上的增益
        public List<Spells> buffList = new List<Spells>();
        // 示例逻辑子弹集合
        public List<Spells> bulletList = new List<Spells>();
        // 包装器持有的下一枚逻辑子弹
        public Bullet subBullet;

        /// <summary>
        /// 提供逻辑子弹开始阶段的扩展钩子 基类不执行表现
        /// </summary>
        public virtual void OnStart()
        {
        }

        /// <summary>
        /// 提供逻辑子弹更新阶段的扩展钩子 基类不执行移动
        /// </summary>
        public virtual void OnUpdate()
        {
        }

        /// <summary>
        /// 提供逻辑子弹结束阶段的扩展钩子 基类不执行回收
        /// </summary>
        public virtual void OnEnd()
        {
        }
    }

    /// <summary>
    /// 保留逻辑子弹派生类型的示例占位
    /// </summary>
    public class Bullet1 : Bullet
    {
    }

    /// <summary>
    /// 法术
    /// </summary>
    public class Spells
    {
        // 示例法术或逻辑子弹名称
        public string name;
        // 示例法术类型
        public SpellsType type;
        // 示例增益类型
        public BuffType buffType;
        // 示例逻辑子弹类别
        public BuletsType buttleType;

        //多重施法次数
        public int MultipleCastCount;

        // 一次性法术或子弹是否已消费
        private bool isUsed;

        public bool CanUse
        {
            get
            {
                bool result = false;
                switch (type)
                {
                    case SpellsType.LongBuff:
                        result = true;
                        break;
                    case SpellsType.OnceBuff:
                        result = !isUsed;
                        break;
                    case SpellsType.Bullet:
                        result = !isUsed;
                        break;
                }

                return result;
            }
        }

        /// <summary>
        /// 按法术类型标记一次性使用状态并记录示例日志
        /// </summary>
        public void Use()
        {
            switch (type)
            {
                case SpellsType.LongBuff:
                    isUsed = false;
                    break;
                case SpellsType.OnceBuff:
                    isUsed = true;
                    break;
                case SpellsType.Bullet:
                    isUsed = true;
                    break;
            }

            Debug.LogError($"法术使用:{name}");
        }

        /// <summary>
        /// 清除法术的已使用标记
        /// </summary>
        public void ResetUse()
        {
            isUsed = false;
        }
    }

    /// <summary>
    /// 法术类型
    /// </summary>
    public enum SpellsType
    {
        LongBuff,
        OnceBuff,
        Bullet,
    }

    /// <summary>
    /// BUFF类型
    /// </summary>
    public enum BuffType
    {
        None,
        Buff1,

        /// <summary>
        /// 多重施法
        /// </summary>
        MultipleCast
    }

    /// <summary>
    /// 子弹类型
    /// </summary>
    public enum BuletsType
    {
        None,
        Bullet1,

        /// <summary>
        /// 包装器
        /// </summary>
        Wrapper,
    }
}