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
using YangTools.UGUI;
using System.Collections.Generic;
using System.Linq;

namespace YangToolMageEngineer
{
    /// <summary>
    /// 法师工程塔
    /// </summary>
    public class MageEngineer
    {
        private ShooterSystem shooter;
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

        public void Shoot()
        {
            Debug.LogError("使用法术");
            shooter.Shoot();
        }

    }

    public class ShooterSystem
    {
        public List<Spells> AllWordList = new List<Spells>();
        public List<Spells> LongBuff = new List<Spells>();

        public void Init(List<Spells> allWordList)
        {
            AllWordList = allWordList;
            LongBuff = AllWordList.Where((item) => { return item.type == SpellsType.LongBuff; }).ToList();
        }

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

            //重置检测
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

            //多重施法检测
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

    public class Bullet
    {
        public string name;
        public BuletsType buttleType;
        public List<Spells> buffList = new List<Spells>();
        public List<Spells> bulletList = new List<Spells>();
        public Bullet subBullet;

        public virtual void OnStart()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnEnd()
        {
        }
    }

    public class Bullet1 : Bullet
    {
    }

    /// <summary>
    /// 法术
    /// </summary>
    public class Spells
    {
        public string name;
        public SpellsType type;

        public BuffType buffType;

        public BuletsType buttleType;

        //多重施法次数
        public int MultipleCastCount;

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