using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YangObjectInteract
{
	public class YangInteractSystem : IUnityLoopScriptExtend
	{
		private static YangInteractSystem instance;
		public static YangInteractSystem Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new YangInteractSystem();
				}
				return instance;
			}
		}

		/// <summary>
		/// 交互对象之间的关系映射
		/// </summary>
		public Dictionary<Type, List<Type>> InteractRelationMapper { get; } = null;
		/// <summary>
		/// 所以交互
		/// </summary>
		private readonly Dictionary<Type,IInteractBase> allInteractBase = new Dictionary<Type,IInteractBase>();
		/// <summary>
		/// 执行中的
		/// </summary>
		private readonly List<IInteractBase> executingInteractBase = new List<IInteractBase>();

		private ICanFocus previousFocused;
		private ICanDrag possibleDragTarget;//可能的拖动目标
		private Vector3 mouseDownPosition;//鼠标点下位置
		
		//当前激活的交互情景
		private IInteractBase currentActiveInteractBase;
		public bool InUpdate { get; set; }
		/// <summary>
		/// 指针在UI上
		/// </summary>
		public bool IsPointerOverUI { get; private set; }
		/// <summary>
		/// 当前聚焦物体
		/// </summary>
		public ICanFocus CurrentFocused { get; private set; }
		/// <summary>
		/// 当前选择物体
		/// </summary>
		public ICanSelect CurrentSelected { get; private set; }
		/// <summary>
		/// 准备选择
		/// </summary>
		public ICanSelect ReadySelect { get; set; }
		/// <summary>
		/// 当前拖拽
		/// </summary>
		public ICanDrag CurrentDragged { get; private set; }
		/// <summary>
		/// 准备拖拽
		/// </summary>
		public ICanDrag ReadyDrag { get; set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void MakeSureInstanceExist()
		{
			instance = new YangInteractSystem();
		}

		private YangInteractSystem()
		{
			this.EnableMonoUpdater();
			InteractRelationMapper = new Dictionary<Type, List<Type>>();
			//加载所有的交互情景
			var assembly = typeof(YangInteractSystem).Assembly;
			var types = assembly.GetTypes().Where(type => typeof(IInteractBase).IsAssignableFrom(type) && !type.IsAbstract).ToList();
			for (int i = 0; i < types.Count; i++)
			{
				ObjectInteractAttribute attribute = types[i].GetCustomAttribute<ObjectInteractAttribute>();
				if (attribute != null) 
				{
					IInteractBase ic = Activator.CreateInstance(types[i], attribute.InteractSubject, attribute.InteractTarget) as IInteractBase;
					allInteractBase.Add(types[i],ic);
					ic.Enable = attribute.EnableExecuteOnLoad;
					executingInteractBase.Add(ic);
					//记录下交互对象之间的关系
					List<Type> list;
					if (InteractRelationMapper.TryGetValue(attribute.InteractSubject, out list))
					{
						list.Add(attribute.InteractTarget);
					}
					else
					{
						InteractRelationMapper.Add(attribute.InteractSubject,new List<Type> { attribute.InteractTarget });
					}

					if (InteractRelationMapper.TryGetValue(attribute.InteractTarget, out list))
					{
						list.Add(attribute.InteractSubject);
					}
					else
					{
						InteractRelationMapper.Add(attribute.InteractTarget, new List<Type> { attribute.InteractSubject });
					}
				}
			}
		}
		
		public void Update()
		{
			//执行所有的交互情景
			IInteractBase targetActiveBase = null;
			for (int i = 0; i < executingInteractBase.Count; i++)
			{
				//Execute方法返回true则表示当前情景被激活
				if (executingInteractBase[i].Enable && executingInteractBase[i].Execute(CurrentFocused, CurrentSelected, CurrentDragged))
				{
					targetActiveBase = executingInteractBase[i];
				}
			}
			//为了在情景更改时首先执行激活情景的退出事件(如果有的话)
			if (targetActiveBase != null && targetActiveBase != currentActiveInteractBase)
			{
				//如果当前激活的情景更改了,则把激活的情景放到列表最前方第一个进行处理
				currentActiveInteractBase = targetActiveBase;
				executingInteractBase.Remove(currentActiveInteractBase);
				executingInteractBase.Insert(0, currentActiveInteractBase);
			}

			//当指针处于UI上时停止对场景中交互对象的操作
			if (EventSystem.current?.IsPointerOverGameObject() ?? false)
			{
				//如果当前指针不在UI上但是之前是在UI上则重置当前聚焦对象
				if (!IsPointerOverUI)
				{
					if (CurrentSelected == ReadySelect) ReadySelect = null;
					//如果从场景转到UI上但是当前聚焦的对象没有RectTransform组件说明当前UI不是一个交互对象
					if (CurrentFocused != null && !(CurrentFocused as MonoBehaviour).TryGetComponent(out RectTransform component))
					{
						SetCurrentFocused(null);
					}
					IsPointerOverUI = true;
				}
			}
			else
			{
				if (Camera.main == null) return;
				IsPointerOverUI = false;
				//射线检测
				Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(mouseRay, out RaycastHit hitInfo, Mathf.Infinity))
				{
					if (hitInfo.transform.TryGetComponent(out ICanFocus focus))
					{
						if (focus != CurrentFocused && focus.EnableFocus) SetCurrentFocused(focus);
					}
					else
					{
						SetCurrentFocused(null);
					}

					if (hitInfo.transform.TryGetComponent(out ICanSelect selectable) && selectable.EnableSelect)
					{
						ReadySelect = selectable;
					}
					else
					{
						ReadySelect = null;
					}

					if (hitInfo.transform.TryGetComponent(out ICanDrag dragable) && dragable.EnableDrag)
					{
						ReadyDrag = dragable;
					}
					else
					{
						ReadyDrag = null;
					}
				}
				else
				{
					ReadyDrag = null;
					ReadySelect = null;
					//射线没有任何碰撞应该把当前聚焦的对象置空
					SetCurrentFocused(null);
				}
			}

			bool mouseBtnPressed = Input.GetMouseButton(0);
			if (CurrentFocused != null)
			{
				if (Input.GetMouseButtonDown(0))
				{
					mouseDownPosition = Input.mousePosition;
					possibleDragTarget = (CurrentFocused as ICanDrag);
				}
				//如果鼠标按下并且移动了一定距离则开始拖拽
				if (CurrentDragged == null && mouseBtnPressed && possibleDragTarget != null)
				{
					if (Vector3.Distance(mouseDownPosition, Input.mousePosition) > 16f && ReadyDrag == possibleDragTarget)
					{
						//拖拽与被选中的不能是同一个
						if (ReadyDrag == CurrentSelected)
						{
							//把选择的对象置空
							SetCurrentSelected(null);
						}
						//再设置拖拽
						SetCurrentDraged(ReadyDrag);
					}
				}
			}
			else 
			{
				if (Input.GetMouseButtonDown(0))
				{
					possibleDragTarget = null;
					ReadyDrag = null;
				}
			}

			//拖拽处理
			if (mouseBtnPressed && CurrentDragged != null)
				CurrentDragged.OnDragging();

			if (Input.GetMouseButtonUp(0))
			{
				if (CurrentDragged == null)
				{
					if (ReadySelect != null)
					{
						if (CurrentSelected != ReadySelect) SetCurrentSelected(ReadySelect);
						//再次点击选择的对象则取消选中
						else if (CurrentSelected == ReadySelect) SetCurrentSelected(null);
					}
				}
				else
				{
					SetCurrentDraged(null);
				}
			}
			//一些调试信息
			//Debug.Log(currentFocused?.interactTag.Name ?? "Null");
			//Debug.Log($"【Focus:{currentFocused?.interactTag.Name}】【Select:{currentSelected?.interactTag.Name}]】【Drag:{currentDraged?.interactTag.Name}】");
		}

		/// <summary>
		/// 重置所有的交互状态
		/// </summary>
		public void Reset() 
		{
			SetCurrentDraged(null);
			SetCurrentSelected(null);
			SetCurrentFocused(null);
		}
		/// <summary>
		/// 设置当前聚焦对象
		/// </summary>
		public void SetCurrentFocused(ICanFocus canFocus,bool isUIItem = false)
		{
			previousFocused = CurrentFocused;
			CurrentFocused?.EndFocus();
			CurrentFocused = canFocus;
			CurrentFocused?.OnFocus();
		}
		/// <summary>
		/// 设置当前选择对象
		/// </summary>
		public void SetCurrentSelected(ICanSelect canSelect)
		{
			CurrentSelected?.EndSelect();
			CurrentSelected = canSelect;
			CurrentSelected?.OnSelect();
		}
		/// <summary>
		/// 设置当前拖拽对象
		/// </summary>
		public void SetCurrentDraged(ICanDrag canDrag)
		{
			CurrentDragged?.EndDrag(CurrentFocused);
			CurrentDragged = canDrag;
			CurrentDragged?.OnDrag();
		}
		/// <summary>
		/// 启用指定的交互情景
		/// </summary>
		public void EnableInteractCase<T>() where T : IInteractBase
		{
			Type type = typeof(T);
			if (allInteractBase.TryGetValue(type, out var value)) 
			{
				value.Enable = true;
			}
		}
		/// <summary>
		/// 禁用指定的交互情景
		/// </summary>
		public void DisableInteractCase<T>() where T : IInteractBase 
		{
			Type type = typeof(T);
			if (allInteractBase.ContainsKey(type))
			{
				allInteractBase[type].Enable = false;
			}
		}

		/// <summary>
		/// 获取所有的交互情景
		/// </summary>
		/// <returns></returns>
		public IInteractBase[] GetAllInteractCases()
		{
			return allInteractBase.Values.ToArray();
		}
		public void FixedUpdate() { }
		public void LateUpdate() { }
	}
}
