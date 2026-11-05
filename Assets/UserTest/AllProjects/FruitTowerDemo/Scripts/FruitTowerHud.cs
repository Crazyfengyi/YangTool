using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FruitTowerDemo
{
    /// <summary>
    /// 水果塔界面控制器
    /// </summary>
    public sealed class FruitTowerHud : MonoBehaviour
    {
        /// <summary>
        /// 游戏控制器
        /// </summary>
        [SerializeField] private FruitTowerGameController gameController;

        /// <summary>
        /// 分数文本
        /// </summary>
        [SerializeField] private TMP_Text scoreText;

        /// <summary>
        /// 下一个水果图标
        /// </summary>
        [SerializeField] private Image nextFruitImage;

        /// <summary>
        /// 下一个水果名称
        /// </summary>
        [SerializeField] private TMP_Text nextFruitText;

        /// <summary>
        /// 游戏结束面板
        /// </summary>
        [SerializeField] private GameObject gameOverPanel;

        /// <summary>
        /// 游戏结束分数文本
        /// </summary>
        [SerializeField] private TMP_Text gameOverScoreText;

        /// <summary>
        /// 重新开始按钮
        /// </summary>
        [SerializeField] private Button restartButton;

        private static readonly string[] FruitNames =
        {
            "浆果",
            "草莓",
            "橙子",
            "苹果",
            "梨",
            "西瓜"
        };

        /// <summary>
        /// 注册界面按钮事件
        /// </summary>
        private void Awake()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
        }

        /// <summary>
        /// 移除界面按钮事件
        /// </summary>
        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }
        }

        /// <summary>
        /// 设置当前分数
        /// </summary>
        public void SetScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"分数  {score}";
            }
        }

        /// <summary>
        /// 设置下一个水果显示
        /// </summary>
        public void SetNextFruit(FruitType fruitType)
        {
            int index = Mathf.Clamp((int)fruitType, 0, FruitNames.Length - 1);
            if (nextFruitImage != null)
            {
                nextFruitImage.color = FruitPiece.GetColor(fruitType);
            }

            if (nextFruitText != null)
            {
                nextFruitText.text = $"下一个  {FruitNames[index]}";
            }
        }

        /// <summary>
        /// 隐藏游戏结束面板
        /// </summary>
        public void HideGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 显示游戏结束面板
        /// </summary>
        public void ShowGameOver(int score)
        {
            if (gameOverScoreText != null)
            {
                gameOverScoreText.text = $"得分  {score}";
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 由界面按钮调用重新开始
        /// </summary>
        public void OnRestartClicked()
        {
            if (gameController != null)
            {
                gameController.RestartGame();
            }
        }
    }
}
