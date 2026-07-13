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

public class TestHealthBarRegister : MonoBehaviour
{
	[SerializeField] private HealthBarManager manager;
	private HealthBarTarget target;

	private void Start()
	{
		target = GetComponent<HealthBarTarget>();
		manager.RegisterTarget(target);
	}

	private void OnDestroy()
	{
		if (manager != null && target != null)
		{
			manager.UnregisterTarget(target);
		}
	}

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