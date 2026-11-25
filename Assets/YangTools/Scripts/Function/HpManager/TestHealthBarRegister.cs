/*
 *Copyright(C) 2020 by xybt 
 *All rights reserved.
 *Author:PC-20260301BNFU 
 *UnityVersion：2022.3.62f3c1 
 *创建时间:2026-07-13 
 */  
using System;
using System.Collections;  
using UnityEngine;  
using UnityEngine.UI;
using TMPro;
using YangTools;

/// <summary>
/// 演示血条目标注册 护盾设置与按键受伤
/// </summary>
public class TestHealthBarRegister : MonoBehaviour
{
	// 测试目标注册到的血条管理器
	[SerializeField] private HealthBarManager manager;
	// 示例操作的目标对象
	private HealthBarTarget target;

	/// <summary>
	/// 取得同物体的血条目标并注册到配置的管理器
	/// </summary>
	private void Start()
	{
		target = GetComponent<HealthBarTarget>();
		manager.RegisterTarget(target);
	}

	/// <summary>
	/// 在目标与管理器仍有效时取消血条注册
	/// </summary>
	private void OnDestroy()
	{
		if (manager != null && target != null)
		{
			manager.UnregisterTarget(target);
		}
	}

	/// <summary>
	/// 按 A 设置测试护盾与血量 按 S 扣除测试生命
	/// </summary>
	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
		{
			//target.Heal(10f);
			target.SetShield(30f);
			target.SetHealth(80f, 100f);
			// target.SetBuffs(buffList);
			// target.SetStatuses(statusList);
		}

		if (Input.GetKeyDown(KeyCode.S))
		{
			target.TakeDamage(20f);
		}
	}
}