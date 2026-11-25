using UnityEngine;

/// <summary>
/// 在首次启动后按配置延迟销毁怪物的演示组件
/// </summary>
public class MonsterAutoDespawn : MonoBehaviour
{
    // 首次启动后延迟销毁的秒数 非正值不安排销毁
    [SerializeField] private float lifeSeconds = 8f;

    /// <summary>
    /// 存活时长大于零时安排物体销毁
    /// </summary>
    private void Start()
    {
        if (lifeSeconds > 0f)
        {
            Destroy(gameObject, lifeSeconds);
        }
    }
}
