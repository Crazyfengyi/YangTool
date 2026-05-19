using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 轮盘转盘类
/// </summary>
public class RouletteWheel : MonoBehaviour
{
    [Header("UI相关")]
    [SerializeField] private RectTransform wheelRoot; // 转盘根节点，作为整个轮盘的容器
    [SerializeField] private RouletteWheelItemUI itemPrefab; // 轮盘项目预制体，用于生成轮盘上的各个项目
    [SerializeField] private RectTransform pointerRoot; // 指针根节点，用于指示中奖位置
    [SerializeField] private Button spinButton; // 旋转按钮，触发轮盘旋转

    [Header("布局相关")]
    [SerializeField]
    [LabelText("轮盘半径")]
    private float radius = 160f;
    [SerializeField] 
    [LabelText("起始角度偏移")]
    private float startAngleOffset;
    [SerializeField] 
    [LabelText("指针角度")]
    private float pointerAngle;

    [Header("旋转相关参数")]
    [SerializeField]
    [LabelText("最小旋转圈数")]
    private int minFullTurns = 5;
    [SerializeField] 
    [LabelText("额外随机旋转圈数，增加旋转的随机性")]
    private int extraRandomTurns = 2;
    [SerializeField] private float spinFastDuration = 1.2f; //快速旋转持续时间
    [SerializeField] private float spinSlowDuration = 2f; //慢速旋转持续时间
    [SerializeField] private float overshootDegrees = 4f; //超出角度，控制轮盘停止前的超出角度
    [SerializeField] private bool allowSpinWhileSpinning; //是否允许在旋转中再次旋转，控制旋转状态下的交互
    [SerializeField] private Ease fastEase = Ease.Linear; //快速旋转缓动效果，设置快速旋转的动画曲线
    [SerializeField] private Ease slowEase = Ease.OutQuart; //慢速旋转缓动效果，设置减速阶段的动画曲线

    //事件声明
    public event Action OnSpinStarted; // 旋转开始事件，在轮盘开始旋转时触发
    public event Action<WheelItemData> OnSpinCompleted; // 旋转完成事件，在轮盘停止并确定结果后触发
    public event Action<string> OnSpinFailed; // 旋转失败事件，在旋转过程中出现错误时触发

    //数据存储
    private readonly List<WheelItemData> items = new List<WheelItemData>(); // 轮盘项目数据列表，存储所有轮盘项目
    private readonly List<RouletteWheelItemUI> itemList = new List<RouletteWheelItemUI>(); // 轮盘项目UI列表，存储所有项目对应的UI元素
    private Sequence spinSequence; //旋转动画
    private bool isSpinning; //是否正在旋转
    private WheelItemData pendingResult; //等待处理的结果，存储旋转结果
    private Action<WheelItemData> pendingCompleteCallback; //等待处理的完成回调

    //属性
    public bool IsSpinning => isSpinning; // 是否正在旋转的只读属性，外部可查询旋转状态
    public IReadOnlyList<WheelItemData> Items => items; // 轮盘项目的只读列表，外部可访问项目数据

    private void Awake()
    {
        if (wheelRoot == null)
        {
            wheelRoot = transform as RectTransform; // 如果未设置轮盘根节点，则使用当前对象的RectTransform
        }
        ApplyPointerAngle(); // 应用指针角度，确保指针指向正确
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(() => SpinRandom()); // 绑定旋转按钮点击事件，点击时执行随机旋转
        }
    }
    /// <summary>
    /// 值更改时
    /// </summary>
    private void OnValidate()
    {
        ApplyPointerAngle(); //在编辑器模式下应用指针角度
    }
    private void OnDisable()
    {
        StopImmediately(); // 当对象禁用时立即停止旋转
    }
    private void OnDestroy()
    {
        StopImmediately(); // 当对象销毁时立即停止旋转
    }
    /// <summary>
    /// 设置转盘区域
    /// </summary>
    public void SetItems(IReadOnlyList<WheelItemData> newItems)
    {
        StopImmediately(); //先停止当前旋转
        items.Clear(); // 清空现有列表
        if (newItems != null)
        {
            for (int i = 0; i < newItems.Count; i++)
            {
                if (newItems[i] != null)
                {
                    items.Add(newItems[i]); // 添加非空项目到列表
                }
            }
        }
        RebuildItems();
    }
    /// <summary>
    /// 旋转到下标
    /// </summary>
    public void SpinToIndex(int index, Action<WheelItemData> onComplete = null)
    {
        if (!CanSpin(out string failReason))
        {
            Fail(failReason); // 检查是否可以旋转，如果不能则触发失败事件
            return;
        }

        if (index < 0 || index >= items.Count)
        {
            Fail("RouletteWheel SpinToIndex failed: index is out of range."); // 检查索引是否有效
            return;
        }

        SpinToInternal(index, onComplete); // 执行旋转到指定索引
    }
    /// <summary>
    /// 旋转到ID
    /// </summary>
    public void SpinToId(string id, Action<WheelItemData> onComplete = null)
    {
        if (string.IsNullOrEmpty(id))
        {
            Fail("RouletteWheel SpinToId failed: id is null or empty."); // 检查ID是否有效
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].Id == id)
            {
                SpinToIndex(i, onComplete); // 找到匹配ID的项目并旋转到该位置
                return;
            }
        }

        Fail($"RouletteWheel SpinToId failed: id '{id}' was not found."); // 未找到匹配ID的项目
    }
    /// <summary>
    /// 旋转随机目标
    /// </summary>
    public void SpinRandom(Action<WheelItemData> onComplete = null)
    {
        if (!CanSpin(out string failReason))
        {
            Fail(failReason); //检查是否可以旋转
            return;
        }

        int index = PickWeightedIndex(); //根据权重随机选择一个索引
        Debug.LogError($"根据权重随机Index:{index}");
        if (index < 0)
        {
            Fail("RouletteWheel SpinRandom failed: all item weights are zero or negative."); //检查权重是否有效
            return;
        }

        SpinToInternal(index, onComplete); //执行随机旋转
    }
    /// <summary>
    /// 立即停止
    /// </summary>
    public void StopImmediately()
    {
        if (spinSequence != null)
        {
            spinSequence.Kill(false); // 立即停止旋转动画
            spinSequence = null;
        }
        isSpinning = false; // 重置旋转状态
        pendingResult = null; // 清空等待结果
        pendingCompleteCallback = null; // 清空回调
    }
    /// <summary>
    /// 重新构建转盘
    /// </summary>
    private void RebuildItems()
    {
        ClearItems(); //清空现有项目UI

        if (wheelRoot == null)
        {
            if (items.Count > 0)
            {
                Fail("RouletteWheel needs wheelRoot before generating items."); // 检查轮盘根节点是否存在
            }
            return;
        }

        if (items.Count == 0)
        {
            return; // 如果没有项目则直接返回
        }

        float sectorAngle = 360f / items.Count; //计算每个扇区的角度
        for (int i = 0; i < items.Count; i++)
        {
            RouletteWheelItemUI item = Instantiate(itemPrefab, wheelRoot);//实例化预制体
            float angle = startAngleOffset + sectorAngle * i; //计算项目角度
            item.Init(items[i]); //绑定项目数据
            item.SetLayout(angle, radius); //设置布局
            itemList.Add(item); //添加到项目UI列表
        }
    }
    /// <summary>
    /// 清空转盘区域
    /// </summary>
    private void ClearItems()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i] != null)
            {
                Destroy(itemList[i].gameObject); // 销毁所有项目UI
            }
        }
        itemList.Clear(); // 清空项目UI列表
    }
    /// <summary>
    /// 是否可以旋转
    /// </summary>
    private bool CanSpin(out string failReason)
    {
        if (items.Count == 0)
        {
            failReason = "RouletteWheel cannot spin without items."; // 检查是否有项目
            return false;
        }

        if (wheelRoot == null)
        {
            failReason = "RouletteWheel cannot spin because wheelRoot is missing."; // 检查轮盘根节点是否存在
            return false;
        }

        if (isSpinning && !allowSpinWhileSpinning)
        {
            failReason = "RouletteWheel is already spinning."; // 检查是否正在旋转且不允许重复旋转
            return false;
        }

        failReason = null;
        return true; // 可以旋转
    }
    /// <summary>
    /// 旋转到下标
    /// </summary>
    private void SpinToInternal(int index, Action<WheelItemData> onComplete)
    {
        if (isSpinning)
        {
            StopImmediately(); // 如果正在旋转则先停止
        }
        //设置目标结果
        pendingResult = items[index]; 
        //结束回调
        pendingCompleteCallback = onComplete;
        isSpinning = true;//标记为旋转中

        //获取当前角度并规范化
        float currentZ = NormalizeAngle(wheelRoot.localEulerAngles.z); 
        float targetZ = CalculateTargetAngle(index); // 计算目标角度
        float deltaToTarget = PositiveDelta(currentZ, targetZ); //计算到目标的正角度差
        //额外随机圈数
        int extraTurns = extraRandomTurns > 0 ? UnityEngine.Random.Range(0, extraRandomTurns + 1) : 0;
        //总旋转角度
        float fullTurnDegrees = Mathf.Max(0, minFullTurns + extraTurns) * 360f;
        //最终角度
        float finalZ = currentZ + fullTurnDegrees + deltaToTarget; 
        //快速旋转结束角度
        float fastZ = currentZ + Mathf.Max(0, minFullTurns - 1) * 360f;
        //计算超出角度
        float overshootZ = finalZ + Mathf.Sign(overshootDegrees == 0f ? 1f : overshootDegrees) * Mathf.Abs(overshootDegrees);

        // 创建旋转动画序列
        spinSequence = DOTween.Sequence()
            .SetTarget(this)
            .OnStart(() => OnSpinStarted?.Invoke()) // 旋转开始时触发事件
            .Append(wheelRoot.DOLocalRotate(new Vector3(0f, 0f, fastZ), spinFastDuration, RotateMode.FastBeyond360).SetEase(fastEase)) //快速旋转阶段
            .Append(wheelRoot.DOLocalRotate(new Vector3(0f, 0f, overshootZ), spinSlowDuration, RotateMode.FastBeyond360).SetEase(slowEase))//减速阶段
            .OnComplete(CompleteSpin); 
    }
    /// <summary>
    /// 目标角度
    /// </summary>
    private float CalculateTargetAngle(int index)
    {
        float sectorAngle = 360f / items.Count; //计算每个扇区的角度
        float itemCenterAngle = startAngleOffset + sectorAngle * index; //计算项目中心角度
        return NormalizeAngle(itemCenterAngle - pointerAngle); //计算目标角度并规范化
    }
    /// <summary>
    /// 旋转结束
    /// </summary>
    private void CompleteSpin()
    {
        isSpinning = false; // 标记旋转结束
        spinSequence = null; // 清空动画序列

        //获取结果
        WheelItemData result = pendingResult;
        Action<WheelItemData> callback = pendingCompleteCallback;
        //清空结果
        pendingResult = null;
        pendingCompleteCallback = null;

        OnSpinCompleted?.Invoke(result); // 触发旋转完成事件
        callback?.Invoke(result); // 执行回调
    }
    /// <summary>
    /// 权重随机下标
    /// </summary>
    private int PickWeightedIndex()
    {
        int totalWeight = 0; //计算总权重
        for (int i = 0; i < items.Count; i++)
        {
            totalWeight += Mathf.Max(0, items[i].Weight); //累加权重
        }
        if (totalWeight <= 0)
        {
            return -1; //如果总权重小于等于0则返回-1
        }

        int roll = UnityEngine.Random.Range(1, totalWeight + 1); // 生成随机数
        int sum = 0; //当前权重和
        for (int i = 0; i < items.Count; i++)
        {
            sum += Mathf.Max(0, items[i].Weight); //累加当前项目权重
            if (roll <= sum) //如果随机数小于等于当前权重和
            {
                return i; //返回当前索引
            }
        }
        return -1; //默认返回-1
    }
    /// <summary>
    /// 失败
    /// </summary>
    private void Fail(string reason)
    {
        Debug.LogWarning(reason, this); // 输出警告日志
        OnSpinFailed?.Invoke(reason); // 触发失败事件
    }
    /// <summary>
    /// 应用指针角度
    /// </summary>
    private void ApplyPointerAngle()
    {
        if (pointerRoot != null)
        {
            pointerRoot.localRotation = Quaternion.Euler(0f, 0f, -pointerAngle); // 应用指针角度
        }
    }
    /// <summary>
    /// 标准化角度
    /// </summary>
    private static float NormalizeAngle(float angle)
    {
        angle %= 360f; // 角度取模360
        if (angle < 0f)
        {
            angle += 360f; // 如果为负则加360
        }
        return angle; // 返回规范化后的角度
    }
    /// <summary>
    /// 角度差
    /// </summary>
    private static float PositiveDelta(float from, float to)
    {
        return NormalizeAngle(to - from); //计算正角度差
    }
}
