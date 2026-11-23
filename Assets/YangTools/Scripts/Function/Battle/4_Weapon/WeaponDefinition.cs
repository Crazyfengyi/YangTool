using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 武器定义类
/// </summary>
[CreateAssetMenu(menuName = "Game/Weapons/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    [Title("基础信息")]
    [LabelText("武器标识")]
    public string Id;// 武器唯一标识符
    [LabelText("显示名称")]
    public string DisplayName;// 武器显示名称
    [LabelText("武器描述")]
    [MultiLineProperty] public string Description; // 武器描述，支持多行文本
    [LabelText("武器图标")]
    public Sprite Icon;// 武器图标

    [Title("武器类型")]
    [LabelText("武器类型")]
    public WeaponType WeaponType = WeaponType.Ranged;// 武器类型，默认为远程武器
    [LabelText("开火模式")]
    public WeaponFireMode FireMode = WeaponFireMode.Single; // 武器开火模式，默认为单发
    [LabelText("装备槽位")]
    public WeaponEquipSlot EquipSlot = WeaponEquipSlot.Primary; // 武器装备槽位，默认为主武器槽

    [Title("射击参数")]
    [LabelText("开火间隔（秒）")]
    [MinValue(0f)] public float FireInterval = 0.25f;// 开火间隔时间，最小值为0
    [LabelText("连发数量")]
    [MinValue(1)] public int BurstCount = 3;// 连发数量，最小值为1
    [LabelText("连发间隔（秒）")]
    [MinValue(0f)] public float BurstInterval = 0.08f;// 连发间隔时间，最小值为0
    [LabelText("装填时间（秒）")]
    [MinValue(0f)] public float ReloadDuration = 1.2f;// 换弹持续时间，最小值为0
    [LabelText("弹匣容量")]
    [MinValue(0)] public int ClipSize = 12; // 弹夹容量，最小值为0
    [LabelText("每次射击消耗弹药")]
    [MinValue(0)] public int AmmoPerShot = 1; // 每次射击消耗弹药数，最小值为0
    [LabelText("射程")]
    [MinValue(0f)] public float Range = 10f; // 武器射程，最小值为0

    [Title("技能配置")]
    [LabelText("使用技能")]
    public SkillDefinition UseSkill; // 使用武器时的技能定义

    [Title("外观配置")]
    [LabelText("外观预制体")]
    public GameObject ViewPrefab; // 武器的视图预制体
}
