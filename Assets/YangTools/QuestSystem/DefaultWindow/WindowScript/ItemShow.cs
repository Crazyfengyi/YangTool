using TMPro;
using UnityEngine;

/// <summary>
/// 通用任务奖励显示项
/// </summary>
public class ItemShow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rewardText; //奖励文本

    /// <summary>
    /// 显示任务奖励的目标键和数量
    /// </summary>
    /// <param name="reward">任务奖励</param>
    public void Show(Reward reward)
    {
        if (reward == null)
        {
            Hide();
            return;
        }

        if (rewardText != null)
        {
            rewardText.text = reward.DisplayText;
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏未使用的奖励显示项
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
