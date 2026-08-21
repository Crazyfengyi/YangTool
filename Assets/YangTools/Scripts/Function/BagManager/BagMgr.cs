using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cfg;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core;
using YangTools.Scripts.Core.YangSaveData;

namespace GameMain
{
    public class BagMgr : MonoSingleton<BagMgr>
    {
        private readonly Dictionary<int, ItemData_BagProp> _bagPropDict = new();
        public List<ItemData_BagProp> AllBagPropList => _bagPropDict.Values.ToList();

        protected override void Awake()
        {
            base.Awake();
            InitPropDict();
        }

        /// <summary>
        /// 初始化道具字典
        /// </summary>
        public void InitPropDict()
        {
            // 清空当前道具字典
            _bagPropDict.Clear();
            // 从本地存档中获取道具列表
            List<SaveBagPropItem> propList =
                YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_BagProp>(false).savePropList;
            // 遍历道具列表，将每个道具添加到字典中
            for (int i = 0; i < propList.Count; i++)
            {
                // 添加道具到背包，不触发保存操作
                AddBagProp(propList[i].bagPropId, propList[i].bagPropCount, false);
            }
        }

        public ItemData_BagProp TryGetBagProp(int propId)
        {
            return _bagPropDict.GetValueOrDefault(propId);
        }

        public float GetBagPropCount(int propId)
        {
            var bagProp = TryGetBagProp(propId);

            return bagProp?.PropCount ?? 0;
        }

        public void AddBagProp(List<ItemData_BagProp> bagProps, bool saveToLocal = true)
        {
            for (int i = 0; i < bagProps.Count; i++)
            {
                AddBagProp(bagProps[i].PropId, bagProps[i].PropCount, saveToLocal);
            }
        }

        public void SetBagProp(int bagPropId, float addCount, bool saveToLocal)
        {
            ItemData_BagProp bagPropItem = TryGetBagProp(bagPropId);

            if (bagPropItem == null)
            {
                bagPropItem = new ItemData_BagProp(bagPropId, addCount);
                _bagPropDict.Add(bagPropId, bagPropItem);
            }
            else
            {
                bagPropItem.SetBagProp(addCount);
            }

            Debug.Log($"道具数量 Id:{bagPropId} 添加数量:{addCount} 剩余数量:{bagPropItem.PropCount}");
            //更新
            if (saveToLocal)
            {
                YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_BagProp>()
                    .SetBagPropItem(bagPropId, addCount);
            }
        }

        /// <summary>
        /// 添加道具
        /// </summary>
        public void AddBagProp(int bagPropId, float addCount, bool saveToLocal = true, string add_source = "未知来源")
        {
            ItemData_BagProp bagPropItem = TryGetBagProp(bagPropId);

            if (bagPropItem == null)
            {
                bagPropItem = new ItemData_BagProp(bagPropId, addCount);
                _bagPropDict.Add(bagPropId, bagPropItem);
            }
            else
            {
                bagPropItem.AddProp(addCount);
            }

            //Debug.Log($"添加道具成功 Id:{bagPropId} 添加数量:{addCount} 剩余数量:{bagPropItem.PropCount}");
            //更新
            if (saveToLocal)
            {
                YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_BagProp>(true)
                    .AddBagPropItem(bagPropId, addCount);

                QuestProgressEvent questEvent = new QuestProgressEvent();
                questEvent.EventType = QuestProgressEventType.ItemNum;
                questEvent.TargetId = bagPropId.ToString();
                questEvent.Amount = bagPropItem.PropCount;
                questEvent.SendEvent();
            }

            // BagPropChange temp = new BagPropChange();
            // temp.propID = bagPropId;
            // temp.num = addCount;
            // temp.SendEvent();
        }

        /// <summary>
        /// 道具是否充足
        /// </summary>
        public bool BagPropEnough(List<ItemData_BagProp> bagProps, bool debug = true)
        {
            for (int i = 0; i < bagProps.Count; i++)
            {
                if (BagPropEnough(bagProps[i], debug) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 道具是否充足
        /// </summary>
        public bool BagPropEnough(ItemData_BagProp bagProp, bool debug = true)
        {
            return BagPropEnough(bagProp.PropId, bagProp.PropCount, debug);
        }

        /// <summary>
        /// 道具是否充足
        /// </summary>
        public bool BagPropEnough(int bagPropId, float propCount, bool debug = true)
        {
            var bagPropItem = TryGetBagProp(bagPropId);

            if (bagPropItem == null)
            {
                if (debug)
                {
                    Debug.Log($"不存在的道具 道具Id:{bagPropId} propCount:{propCount}");
                }

                return false;
            }

            if (bagPropItem.PropCount < propCount)
            {
                if (debug)
                {
                    Debug.Log($"道具不足 道具Id:{bagPropId} 需要数量:{propCount} 拥有数量:{bagPropItem.PropCount}");
                }

                return false;
            }

            return true;
        }

        public void RemoveBagProp(int bagPropId, float removeCount)
        {
            var bagPropItem = TryGetBagProp(bagPropId);

            if (bagPropItem == null)
            {
                Debug.LogError($"移除道具失败 不存在的道具 bagPropId:{bagPropId} removeCount:{removeCount}");
                return;
            }

            if (bagPropItem.PropCount < removeCount)
            {
                Debug.LogError(
                    $"移除道具失败 道具不足 bagPropId:{bagPropId} removeCount:{removeCount} 拥有数量:{bagPropItem.PropCount}");
                return;
            }

            bagPropItem.RemoveProp(removeCount);
            //Debug.Log($"移除道具成功 Id:{bagPropId} 移除数量:{removeCount} 剩余数量:{bagPropItem.PropCount}");

            //更新
            YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_BagProp>(true)
                .RemoveBagPropItem(bagPropId, removeCount);

            // BagPropChange temp = new BagPropChange();
            // temp.propID = bagPropId;
            // temp.num = -removeCount;
            // temp.SendEvent();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _bagPropDict.Clear();
        }
    }

    /// <summary>
    /// 背包数据
    /// </summary>
    public class Save_BagProp : SaveDataBase
    {
        public List<SaveBagPropItem> savePropList;

        public override void SetDefaultData(string tableData)
        {
            savePropList = new List<SaveBagPropItem>();
            // foreach (var item in category.InitBagProps)
            // {
            //     AddBagPropItem(item.Key, item.Value);
            // }
        }

        private SaveBagPropItem SafeGetBagPropItem(int propId)
        {
            for (int i = 0; i < savePropList.Count; i++)
            {
                if (savePropList[i].bagPropId == propId)
                {
                    return savePropList[i];
                }
            }

            var newBagProp = new SaveBagPropItem()
            {
                bagPropId = propId,
                bagPropCount = 0
            };

            savePropList.Add(newBagProp);
            return newBagProp;
        }

        private void RemoveBagPropItem(int propId)
        {
            for (int i = 0; i < savePropList.Count; i++)
            {
                if (savePropList[i].bagPropId == propId)
                {
                    savePropList.RemoveAt(i);
                    break;
                }
            }
        }

        public void AddBagPropItem(int propId, float addCount)
        {
            var bagPropItem = SafeGetBagPropItem(propId);
            bagPropItem.bagPropCount += addCount;
        }

        public void SetBagPropItem(int propId, float propCount)
        {
            var bagPropItem = SafeGetBagPropItem(propId);
            bagPropItem.bagPropCount = propCount;
        }

        public void RemoveBagPropItem(int propId, float removeCount)
        {
            var bagPropItem = SafeGetBagPropItem(propId);

            if (bagPropItem.bagPropCount < removeCount)
            {
                Debug.LogError($"道具不足不能移除:{bagPropItem} 移除道具:{removeCount}");
            }

            bagPropItem.bagPropCount -= removeCount;
            if (bagPropItem.bagPropCount <= 0)
            {
                RemoveBagPropItem(bagPropItem.bagPropId);
            }
        }
    }

    [System.Serializable]
    public class SaveBagPropItem
    {
        public int bagPropId;
        public float bagPropCount;

        public override string ToString()
        {
            return $"背包,道具Id:{bagPropId} 道具数量:{bagPropCount}";
        }
    }
}