using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 鼠标拖拽临时图标
/// </summary>
public class DragTempIcon : MonoBehaviour
{
	public static DragTempIcon Instance;
	private Image icon;
	private bool isShow = false;

	private void Awake()
	{
		Instance = this;
	}
	private void Start()
	{
		icon = GetComponent<Image>();
		gameObject.SetActive(false);
	}
	private void Update()
	{
		if (isShow) 
		{
			transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
		}
	}
	public void ShowGhostIcon(Sprite sprite) 
	{
		if (sprite == null) return;
		transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
		icon.sprite = sprite;
		gameObject.SetActive(true);
		isShow = true;
	}

	public void HideGhostIcon() 
	{
		isShow = false;
		gameObject.SetActive(false);
	}
}
