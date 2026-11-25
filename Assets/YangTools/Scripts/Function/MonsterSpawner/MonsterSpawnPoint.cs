using UnityEngine;

/// <summary>
/// 提供怪物生成使用的世界坐标锚点
/// </summary>
public class MonsterSpawnPoint : MonoBehaviour
{
    // 生成点的可选位置锚点
    [SerializeField] private Transform spawnRoot;

    /// <summary>
    /// 取得配置锚点的位置 未配置时使用当前物体位置
    /// </summary>
    public Vector3 GetPosition()
    {
        return spawnRoot != null ? spawnRoot.position : transform.position;
    }
}
