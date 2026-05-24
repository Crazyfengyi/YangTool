using UnityEngine;

/// <summary>
/// 武器定义类
/// </summary>
[CreateAssetMenu(menuName = "Game/Weapons/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    [Header("Identity")]
    public string Id;// 武器唯一标识符
    public string DisplayName;// 武器显示名称
    [TextArea] public string Description; // 武器描述，支持多行文本
    public Sprite Icon;// 武器图标

    [Header("Type")]
    public WeaponType WeaponType = WeaponType.Ranged;// 武器类型，默认为远程武器
    public WeaponFireMode FireMode = WeaponFireMode.Single; // 武器开火模式，默认为单发
    public WeaponEquipSlot EquipSlot = WeaponEquipSlot.Primary; // 武器装备槽位，默认为主武器槽

    [Header("Runtime")]
    [Min(0f)] public float FireInterval = 0.25f;// 开火间隔时间，最小值为0
    [Min(1)] public int BurstCount = 3;// 连发数量，最小值为1
    [Min(0f)] public float BurstInterval = 0.08f;// 连发间隔时间，最小值为0
    [Min(0f)] public float ReloadDuration = 1.2f;// 换弹持续时间，最小值为0
    [Min(0)] public int ClipSize = 12; // 弹夹容量，最小值为0
    [Min(0)] public int AmmoPerShot = 1; // 每次射击消耗弹药数，最小值为0
    [Min(0f)] public float Range = 10f; // 武器射程，最小值为0

    [Header("Skill")]
    public SkillDefinition UseSkill; // 使用武器时的技能定义

    [Header("View")]
    public GameObject ViewPrefab; // 武器的视图预制体
}
