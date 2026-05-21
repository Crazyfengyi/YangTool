using UnityEngine;

/// <summary>
/// 生成预制体技能效果类.在指定位置生成一个游戏对象预制体
/// </summary>
[CreateAssetMenu(menuName = "Game/Skills/Effects/Spawn Prefab")]
public class SpawnPrefabSkillEffect : SkillEffect
{
    [SerializeField] private GameObject prefab; // 要生成的预制体对象
    [SerializeField] private bool spawnAtTarget = true; // 是否在目标位置生成，默认为true
    [SerializeField] private Vector3 offset; // 相对于生成位置的偏移量
    [SerializeField] private float autoDestroyAfter; // 自动销毁时间，小于等于0时不自动销毁

    /// <summary>
    /// 执行技能效果的方法
    /// </summary>
    /// <param name="context">技能上下文，包含目标、位置等信息</param>
    public override void Execute(SkillContext context)
    {
        // 如果预制体为空，则直接返回
        if (prefab == null) return;

        // 确定生成位置：如果允许在目标生成且存在目标，则在目标位置生成，否则在指定点生成
        var position = spawnAtTarget && context.Target != null
            ? context.Target.transform.position
            : context.Point;

        // 在计算的位置加上偏移量后生成预制体实例
        var instance = Instantiate(prefab, position + offset, Quaternion.identity);
        // 如果设置了自动销毁时间且大于0，则启动自动销毁计时器
        if (autoDestroyAfter > 0f)
        {
            Destroy(instance, autoDestroyAfter);
        }
    }
}
