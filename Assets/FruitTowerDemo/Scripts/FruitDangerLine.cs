using System.Collections.Generic;
using UnityEngine;

namespace FruitTowerDemo
{
    /// <summary>
    /// 水果塔危险线检测器
    /// </summary>
    public sealed class FruitDangerLine : MonoBehaviour
    {
        /// <summary>
        /// 游戏控制器
        /// </summary>
        [SerializeField] private FruitTowerGameController gameController;

        /// <summary>
        /// 持续接触危险线的结束时间
        /// </summary>
        [SerializeField] private float dangerDuration = 0.8f;

        private readonly HashSet<FruitPiece> dangerousPieces = new HashSet<FruitPiece>();
        private float dangerousTime;

        private void Update()
        {
            if (gameController == null || gameController.IsGameOver())
            {
                return;
            }

            bool hasSettledPiece = false;
            foreach (FruitPiece piece in dangerousPieces)
            {
                if (piece != null && piece.IsSettled)
                {
                    hasSettledPiece = true;
                    break;
                }
            }

            if (!hasSettledPiece)
            {
                dangerousTime = 0f;
                return;
            }

            dangerousTime += Time.deltaTime;
            if (dangerousTime >= dangerDuration)
            {
                gameController.EndGame();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            FruitPiece piece = other.GetComponentInParent<FruitPiece>();
            if (piece != null)
            {
                dangerousPieces.Add(piece);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            FruitPiece piece = other.GetComponentInParent<FruitPiece>();
            if (piece != null)
            {
                dangerousPieces.Remove(piece);
                if (dangerousPieces.Count == 0)
                {
                    dangerousTime = 0f;
                }
            }
        }
    }
}
