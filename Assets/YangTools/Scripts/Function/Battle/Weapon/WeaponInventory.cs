using System.Collections.Generic;
using UnityEngine;

/**
 * 武器库存管理类
 */
[System.Serializable]
public class WeaponInventory
{
    //存储已装备的武器列表
    [SerializeField] private List<WeaponDefinition> equippedWeapons = new List<WeaponDefinition>();
    //当前选中的武器索引，-1表示没有选中任何武器
    [SerializeField] private int currentIndex = -1;

    //获取已装备的武器列表
    public IReadOnlyList<WeaponDefinition> EquippedWeapons => equippedWeapons;
    //获取当前选中的武器索引
    public int CurrentIndex => currentIndex;
    //获取当前选中的武器，如果没有则返回null
    public WeaponDefinition CurrentWeapon =>
        currentIndex >= 0 && currentIndex < equippedWeapons.Count ? equippedWeapons[currentIndex] : null;

    /**
     * 设置武器列表
     */
    public void SetWeapons(IEnumerable<WeaponDefinition> weapons)
    {
        equippedWeapons.Clear();
        if (weapons != null)
        {
            equippedWeapons.AddRange(weapons);
        }

        // 如果有武器则选中第一个，否则设置为-1
        currentIndex = equippedWeapons.Count > 0 ? 0 : -1;
    }

    /**
     * 添加武器到库存
     * @param weapon 要添加的武器
     * @return 是否添加成功
     */
    public bool AddWeapon(WeaponDefinition weapon)
    {
        // 如果武器为空或已存在，则返回false
        if (weapon == null || equippedWeapons.Contains(weapon))
        {
            return false;
        }

        equippedWeapons.Add(weapon);
        // 如果当前没有选中武器，则选中新添加的武器
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        return true;
    }

    /**
     * 从库存中移除武器
     * @param weapon 要移除的武器
     * @return 是否移除成功
     */
    public bool RemoveWeapon(WeaponDefinition weapon)
    {
        if (weapon == null)
        {
            return false;
        }

        int removedIndex = equippedWeapons.IndexOf(weapon);
        if (removedIndex < 0)
        {
            return false;
        }

        equippedWeapons.RemoveAt(removedIndex);
        // 移除后处理当前选中索引
        if (equippedWeapons.Count == 0)
        {
            currentIndex = -1;
        }
        else if (currentIndex >= equippedWeapons.Count)
        {
            currentIndex = equippedWeapons.Count - 1;
        }
        else if (removedIndex <= currentIndex)
        {
            currentIndex = Mathf.Max(0, currentIndex - 1);
        }

        return true;
    }

    /**
     * 装备指定武器
     * @param weapon 要装备的武器
     * @return 是否装备成功
     */
    public bool Equip(WeaponDefinition weapon)
    {
        if (weapon == null)
        {
            currentIndex = -1;
            return false;
        }

        int index = equippedWeapons.IndexOf(weapon);
        if (index < 0)
        {
            // 如果武器不在列表中，则先添加
            equippedWeapons.Add(weapon);
            index = equippedWeapons.Count - 1;
        }

        currentIndex = index;
        return true;
    }

    /**
     * 通过索引装备武器
     * @param index 武器在列表中的索引
     * @return 是否装备成功
     */
    public bool EquipByIndex(int index)
    {
        if (index < 0 || index >= equippedWeapons.Count)
        {
            return false;
        }

        currentIndex = index;
        return true;
    }

    /**
     * 通过装备槽位装备武器
     * @param slot 武器装备槽位
     * @return 是否装备成功
     */
    public bool EquipBySlot(WeaponEquipSlot slot)
    {
        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            var weapon = equippedWeapons[i];
            // 查找指定槽位的武器并装备
            if (weapon != null && weapon.EquipSlot == slot)
            {
                currentIndex = i;
                return true;
            }
        }

        return false;
    }
}
