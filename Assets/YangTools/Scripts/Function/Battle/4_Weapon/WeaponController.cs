using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 武器控制器类，负责管理武器的装备、射击、装弹等功能
/// </summary>
[DisallowMultipleComponent]
public class WeaponController : MonoBehaviour
{
    [SerializeField] private SkillCaster skillCaster; // 技能施放器，用于关联武器技能
    [SerializeField] private List<WeaponDefinition> startingWeapons = new List<WeaponDefinition>(); // 初始武器列表
    [SerializeField] private WeaponInventory inventory = new WeaponInventory(); // 武器库存系统

    // 当前武器的属性访问器
    public WeaponDefinition CurrentWeapon => inventory.CurrentWeapon; // 当前装备的武器
    public int CurrentAmmoInClip => GetState(CurrentWeapon).ClipAmmo; // 当前弹夹中的弹药数量
    public bool IsReloading => GetState(CurrentWeapon).IsReloading; // 是否正在装弹
    public WeaponInventory Inventory => inventory; // 武器库存的访问器

    // 武器相关事件
    public event Action<WeaponDefinition> OnWeaponEquipped; // 装备武器事件
    public event Action<WeaponDefinition> OnFireStarted; // 开始射击事件
    public event Action<WeaponDefinition, WeaponFireFailReason> OnFireFailed; // 射击失败事件
    public event Action<WeaponDefinition, int, int> OnAmmoChanged; // 弹药变化事件
    public event Action<WeaponDefinition> OnReloadStarted; // 开始装弹事件
    public event Action<WeaponDefinition> OnReloadCompleted; // 装弹完成事件

    private readonly Dictionary<WeaponDefinition, WeaponRuntimeState> runtimeStates = new Dictionary<WeaponDefinition, WeaponRuntimeState>(); // 武器运行时状态字典
    private Coroutine reloadRoutine; // 装弹协程
    private Coroutine firingRoutine; // 射击协程
    private FireRequest currentFireRequest; // 当前射击请求
    private int weaponInstanceSeed; // 武器实例种子

    /// <summary>
    /// 重置时初始化技能施放器
    /// </summary>
    private void Reset()
    {
        skillCaster = GetComponent<SkillCaster>();
    }

    /// <summary>
    /// 唤醒时初始化武器系统
    /// </summary>
    private void Awake()
    {
        // 确保技能施放器已初始化
        if (skillCaster == null)
        {
            skillCaster = GetComponent<SkillCaster>();
        }

        // 初始化武器系统
        inventory.SetWeapons(startingWeapons);
        SyncInventoryStates();
        RegisterInventorySkills();
        if (inventory.CurrentWeapon != null)
        {
            OnWeaponEquipped?.Invoke(inventory.CurrentWeapon);
            NotifyAmmoChanged(inventory.CurrentWeapon);
        }
    }

    /// <summary>
    /// 装备指定武器
    /// </summary>
    /// <param name="weapon">要装备的武器定义</param>
    /// <returns>是否装备成功</returns>
    public bool Equip(WeaponDefinition weapon)
    {
        CancelReload(); // 取消当前装弹

        if (!inventory.Equip(weapon)) // 尝试装备武器
        {
            return false;
        }

        EnsureState(weapon); // 确保武器状态已初始化
        RegisterWeaponSkill(weapon); // 注册武器技能
        OnWeaponEquipped?.Invoke(weapon); // 触发装备事件
        NotifyAmmoChanged(weapon); // 通知弹药变化
        return true;
    }

    /// <summary>
    /// 通过装备槽位装备武器
    /// </summary>
    /// <param name="slot">装备槽位</param>
    /// <returns>是否装备成功</returns>
    public bool EquipBySlot(WeaponEquipSlot slot)
    {
        CancelReload(); // 取消当前装弹
        if (!inventory.EquipBySlot(slot)) // 尝试通过槽位装备武器
        {
            return false;
        }

        OnWeaponEquipped?.Invoke(inventory.CurrentWeapon); // 触发装备事件
        NotifyAmmoChanged(inventory.CurrentWeapon); // 通知弹药变化
        return true;
    }

    /// <summary>
    /// 尝试向目标射击
    /// </summary>
    /// <param name="target">目标游戏对象</param>
    /// <returns>是否成功射击</returns>
    public bool TryFireAtTarget(GameObject target)
    {
        var weapon = CurrentWeapon;
        if (weapon == null)
        {
            return FailFire(null, WeaponFireFailReason.NoWeaponEquipped);
        }

        var point = target != null ? target.transform.position : GetOriginPoint();
        var direction = target != null ? (point - GetOriginPoint()).normalized : transform.forward;
        return FireOnce(new FireRequest(target, point, direction));
    }

    /// <summary>
    /// 尝试向指定点射击
    /// </summary>
    /// <param name="point">目标点</param>
    /// <returns>是否成功射击</returns>
    public bool TryFireAtPoint(Vector3 point)
    {
        var direction = point - GetOriginPoint();
        return FireOnce(FireRequest.ForPoint(point, direction.sqrMagnitude > 0f ? direction.normalized : transform.forward));
    }

    /// <summary>
    /// 尝试向指定方向射击
    /// </summary>
    /// <param name="direction">射击方向</param>
    /// <returns>是否成功射击</returns>
    public bool TryFireInDirection(Vector3 direction)
    {
        var normalized = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
        var point = GetOriginPoint() + normalized * GetWeaponRange(CurrentWeapon);
        return FireOnce(FireRequest.ForDirection(normalized, point));
    }

    /// <summary>
    /// 开始持续射击
    /// </summary>
    public void StartFire()
    {
        var weapon = CurrentWeapon;
        if (weapon == null)
        {
            FailFire(null, WeaponFireFailReason.NoWeaponEquipped);
            return;
        }

        currentFireRequest = FireRequest.ForDirection(transform.forward, GetOriginPoint() + transform.forward * GetWeaponRange(weapon));
        if (weapon.FireMode == WeaponFireMode.Automatic)
        {
            if (firingRoutine == null)
            {
                firingRoutine = StartCoroutine(AutoFireRoutine());
            }
            return;
        }

        FireOnce(currentFireRequest);
    }

    /// <summary>
    /// 停止射击
    /// </summary>
    public void StopFire()
    {
        if (firingRoutine != null)
        {
            StopCoroutine(firingRoutine);
            firingRoutine = null;
        }
    }

    /// <summary>
    /// 尝试装弹
    /// </summary>
    /// <returns>是否开始装弹</returns>
    public bool TryReload()
    {
        var weapon = CurrentWeapon;
        if (weapon == null)
        {
            return false;
        }

        var state = GetState(weapon);
        if (state.IsReloading || weapon.ClipSize <= 0 || state.ClipAmmo >= weapon.ClipSize)
        {
            return false;
        }

        CancelReload();
        reloadRoutine = StartCoroutine(ReloadRoutine(weapon, state));
        return true;
    }

    /// <summary>
    /// 自动射击协程
    /// </summary>
    private IEnumerator AutoFireRoutine()
    {
        while (true)
        {
            var weapon = CurrentWeapon;
            if (weapon == null || weapon.FireMode != WeaponFireMode.Automatic)
            {
                firingRoutine = null;
                yield break;
            }

            FireOnce(currentFireRequest);
            yield return null;
        }
    }

    /// <summary>
    /// 执行单次射击
    /// </summary>
    /// <param name="request">射击请求</param>
    /// <returns>是否成功射击</returns>
    private bool FireOnce(FireRequest request)
    {
        var weapon = CurrentWeapon;
        if (weapon == null)
        {
            return FailFire(null, WeaponFireFailReason.NoWeaponEquipped);
        }

        if (skillCaster == null || weapon.UseSkill == null)
        {
            return FailFire(weapon, WeaponFireFailReason.CastRejected);
        }

        var state = GetState(weapon);
        var failReason = ValidateFire(weapon, state, request);
        if (failReason != WeaponFireFailReason.None)
        {
            return FailFire(weapon, failReason);
        }

        if (weapon.FireMode == WeaponFireMode.Burst)
        {
            if (state.PendingBurstShots > 0)
            {
                return FailFire(weapon, WeaponFireFailReason.OnCooldown);
            }
            state.PendingBurstShots = Mathf.Max(1, weapon.BurstCount);
            StartCoroutine(BurstRoutine(weapon, state, request));
            return true;
        }

        return ExecuteShot(weapon, state, request, true);
    }

    /// <summary>
    /// 连发射击协程
    /// </summary>
    private IEnumerator BurstRoutine(WeaponDefinition weapon, WeaponRuntimeState state, FireRequest request)
    {
        while (state.PendingBurstShots > 0 && CurrentWeapon == weapon)
        {
            if (!ExecuteShot(weapon, state, request, state.PendingBurstShots == weapon.BurstCount))
            {
                break;
            }

            state.PendingBurstShots--;
            if (state.PendingBurstShots > 0)
            {
                float wait = weapon.BurstInterval > 0f ? weapon.BurstInterval : weapon.FireInterval;
                yield return new WaitForSeconds(wait);
            }
        }

        state.PendingBurstShots = 0;
    }

    /// <summary>
    /// 执行射击
    /// </summary>
    private bool ExecuteShot(WeaponDefinition weapon, WeaponRuntimeState state, FireRequest request, bool raiseStartedEvent)
    {
        int sourceInstanceId = ++weaponInstanceSeed;
        bool castSucceeded;
        if (request.Target != null)
        {
            castSucceeded = skillCaster.TryCastFromWeapon(weapon, sourceInstanceId, weapon.UseSkill, request.Target);
        }
        else if (request.Mode == RequestMode.Point)
        {
            castSucceeded = skillCaster.TryCastFromWeaponAtPoint(weapon, sourceInstanceId, weapon.UseSkill, request.Point);
        }
        else
        {
            castSucceeded = skillCaster.TryCastFromWeaponInDirection(weapon, sourceInstanceId, weapon.UseSkill, request.Direction);
        }

        if (!castSucceeded)
        {
            return FailFire(weapon, WeaponFireFailReason.CastRejected);
        }

        if (weapon.AmmoPerShot > 0)
        {
            state.ClipAmmo = Mathf.Max(0, state.ClipAmmo - weapon.AmmoPerShot);
            NotifyAmmoChanged(weapon);
        }

        state.NextFireTime = Time.time + Mathf.Max(weapon.FireInterval, 0f);

        if (raiseStartedEvent)
        {
            OnFireStarted?.Invoke(weapon);
        }

        if (weapon.WeaponType == WeaponType.Ranged && state.ClipAmmo <= 0)
        {
            TryReload();
        }

        return true;
    }

    /// <summary>
    /// 验证射击是否有效
    /// </summary>
    private WeaponFireFailReason ValidateFire(WeaponDefinition weapon, WeaponRuntimeState state, FireRequest request)
    {
        if (state.IsReloading)
        {
            return WeaponFireFailReason.Reloading;
        }

        if (Time.time < state.NextFireTime)
        {
            return WeaponFireFailReason.OnCooldown;
        }

        if (weapon.AmmoPerShot > 0 && state.ClipAmmo < weapon.AmmoPerShot)
        {
            return WeaponFireFailReason.EmptyClip;
        }

        if (!IsRequestValid(weapon, request))
        {
            return WeaponFireFailReason.InvalidTarget;
        }

        return WeaponFireFailReason.None;
    }

    /// <summary>
    /// 检查射击请求是否有效
    /// </summary>
    private bool IsRequestValid(WeaponDefinition weapon, FireRequest request)
    {
        if (weapon == null)
        {
            return false;
        }

        if (request.Target != null)
        {
            return IsWithinRange(request.Target.transform.position, weapon);
        }

        if (request.Mode == RequestMode.Point)
        {
            return IsWithinRange(request.Point, weapon);
        }

        return request.Direction.sqrMagnitude > 0f;
    }

    /// <summary>
    /// 检查目标是否在武器射程内
    /// </summary>
    private bool IsWithinRange(Vector3 point, WeaponDefinition weapon)
    {
        float range = GetWeaponRange(weapon);
        if (range <= 0f)
        {
            return true;
        }

        return Vector3.Distance(GetOriginPoint(), point) <= range;
    }

    /// <summary>
    /// 获取武器射程
    /// </summary>
    private float GetWeaponRange(WeaponDefinition weapon)
    {
        if (weapon == null)
        {
            return 0f;
        }

        if (weapon.Range > 0f)
        {
            return weapon.Range;
        }

        return weapon.UseSkill != null ? weapon.UseSkill.Range : 0f;
    }

    /// <summary>
    /// 获取武器射击原点
    /// </summary>
    private Vector3 GetOriginPoint()
    {
        return skillCaster != null ? skillCaster.GetOriginPosition() : transform.position;
    }

    /// <summary>
    /// 装弹协程
    /// </summary>
    private IEnumerator ReloadRoutine(WeaponDefinition weapon, WeaponRuntimeState state)
    {
        state.IsReloading = true;
        OnReloadStarted?.Invoke(weapon);

        if (weapon.ReloadDuration > 0f)
        {
            yield return new WaitForSeconds(weapon.ReloadDuration);
        }

        state.ClipAmmo = Mathf.Max(weapon.ClipSize, 0);
        state.IsReloading = false;
        reloadRoutine = null;
        NotifyAmmoChanged(weapon);
        OnReloadCompleted?.Invoke(weapon);
    }

    /// <summary>
    /// 取消装弹
    /// </summary>
    private void CancelReload()
    {
        if (reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
        }

        if (CurrentWeapon != null)
        {
            GetState(CurrentWeapon).IsReloading = false;
        }
    }

    /// <summary>
    /// 射击失败处理
    /// </summary>
    private bool FailFire(WeaponDefinition weapon, WeaponFireFailReason reason)
    {
        OnFireFailed?.Invoke(weapon, reason);
        return false;
    }

    /// <summary>
    /// 同步库存状态
    /// </summary>
    private void SyncInventoryStates()
    {
        runtimeStates.Clear();
        var weapons = inventory.EquippedWeapons;
        for (int i = 0; i < weapons.Count; i++)
        {
            EnsureState(weapons[i]);
        }
    }

    /// <summary>
    /// 注册库存技能
    /// </summary>
    private void RegisterInventorySkills()
    {
        var weapons = inventory.EquippedWeapons;
        for (int i = 0; i < weapons.Count; i++)
        {
            RegisterWeaponSkill(weapons[i]);
        }
    }

    /// <summary>
    /// 注册武器技能
    /// </summary>
    private void RegisterWeaponSkill(WeaponDefinition weapon)
    {
        if (skillCaster != null && weapon != null && weapon.UseSkill != null)
        {
            skillCaster.AddSkill(weapon.UseSkill);
        }
    }

    /// <summary>
    /// 确保武器状态已初始化
    /// </summary>
    private void EnsureState(WeaponDefinition weapon)
    {
        if (weapon == null || runtimeStates.ContainsKey(weapon))
        {
            return;
        }

        var state = new WeaponRuntimeState();
        state.Init(weapon);
        runtimeStates.Add(weapon, state);
    }

    /// <summary>
    /// 获取武器运行时状态
    /// </summary>
    private WeaponRuntimeState GetState(WeaponDefinition weapon)
    {
        if (weapon == null)
        {
            return new WeaponRuntimeState();
        }

        EnsureState(weapon);
        return runtimeStates[weapon];
    }

    /// <summary>
    /// 通知弹药变化
    /// </summary>
    private void NotifyAmmoChanged(WeaponDefinition weapon)
    {
        if (weapon == null)
        {
            return;
        }

        var state = GetState(weapon);
        OnAmmoChanged?.Invoke(weapon, state.ClipAmmo, weapon.ClipSize);
    }

#if UNITY_EDITOR
    [Button("测试射击")]
    public void TestBtn()
    {
        SimpleHealth[] targets = GameObject.FindObjectsOfType<SimpleHealth>();
        bool result = TryFireAtTarget(targets[0].gameObject);
        Debug.Log($"测试射击:{result}");
    }
#endif
}
