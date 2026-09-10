using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FruitTowerDemo
{
    /// <summary>
    /// 水果塔游戏总控制器
    /// </summary>
    public sealed class FruitTowerGameController : MonoBehaviour
    {
        private struct MergeRequest
        {
            public FruitPiece First;
            public FruitPiece Second;
        }

        /// <summary>
        /// 水果预制体
        /// </summary>
        [SerializeField] private FruitPiece fruitPrefab;

        /// <summary>
        /// 水果父节点
        /// </summary>
        [SerializeField] private Transform fruitRoot;

        /// <summary>
        /// 水果生成点
        /// </summary>
        [SerializeField] private Transform spawnAnchor;

        /// <summary>
        /// 落点预览标记
        /// </summary>
        [SerializeField] private Transform aimMarker;

        /// <summary>
        /// 输入控制器
        /// </summary>
        [SerializeField] private FruitDropInput dropInput;

        /// <summary>
        /// 界面控制器
        /// </summary>
        [SerializeField] private FruitTowerHud hud;

        /// <summary>
        /// 最小落点横坐标
        /// </summary>
        [SerializeField] private float minDropX = -2.55f;

        /// <summary>
        /// 最大落点横坐标
        /// </summary>
        [SerializeField] private float maxDropX = 2.55f;

        /// <summary>
        /// 两次投放之间的间隔
        /// </summary>
        [SerializeField] private float dropCooldown = 0.28f;

        /// <summary>
        /// 初始水果最高等级
        /// </summary>
        [SerializeField] private int initialRandomMaxType = 2;

        private readonly List<FruitPiece> activePieces = new List<FruitPiece>();
        private readonly List<MergeRequest> pendingMerges = new List<MergeRequest>();
        private FruitType currentFruitType;
        private FruitType nextFruitType;
        private int score;
        private bool canDrop;
        private bool isGameOver;

        /// <summary>
        /// 当前水果等级
        /// </summary>
        public FruitType CurrentFruitType => currentFruitType;

        /// <summary>
        /// 下一个水果等级
        /// </summary>
        public FruitType NextFruitType => nextFruitType;

        /// <summary>
        /// 当前是否可以投放
        /// </summary>
        public bool CanDrop => canDrop && !isGameOver;

        /// <summary>
        /// 当前得分
        /// </summary>
        public int Score => score;

        private void Start()
        {
            RestartGame();
        }

        private void LateUpdate()
        {
            ProcessPendingMerges();
        }

        /// <summary>
        /// 重新开始游戏
        /// </summary>
        public void RestartGame()
        {
            StopAllCoroutines();
            ClearActivePieces();
            score = 0;
            isGameOver = false;
            canDrop = true;
            currentFruitType = RollFruitType(initialRandomMaxType);
            nextFruitType = RollFruitType(initialRandomMaxType);

            if (hud != null)
            {
                hud.HideGameOver();
                hud.SetScore(score);
                hud.SetNextFruit(nextFruitType);
            }

            if (dropInput != null)
            {
                dropInput.ResetAim();
            }
        }

        /// <summary>
        /// 设置当前落点预览
        /// </summary>
        public void SetAimWorldX(float worldX)
        {
            float clampedX = Mathf.Clamp(worldX, minDropX, maxDropX);
            if (aimMarker != null && spawnAnchor != null)
            {
                aimMarker.position = new Vector3(clampedX, spawnAnchor.position.y, spawnAnchor.position.z);
            }
        }

        /// <summary>
        /// 根据横向位置投放水果
        /// </summary>
        public void DropAtWorldX(float worldX)
        {
            if (!CanDrop || fruitPrefab == null || spawnAnchor == null)
            {
                return;
            }

            float clampedX = Mathf.Clamp(worldX, minDropX, maxDropX);
            Vector3 spawnPosition = new Vector3(clampedX, spawnAnchor.position.y, spawnAnchor.position.z);
            FruitPiece piece = Instantiate(fruitPrefab, spawnPosition, Quaternion.identity, fruitRoot);
            piece.Initialize(currentFruitType, this);
            activePieces.Add(piece);

            currentFruitType = nextFruitType;
            nextFruitType = RollFruitType(initialRandomMaxType);
            canDrop = false;

            if (hud != null)
            {
                hud.SetNextFruit(nextFruitType);
            }

            StartCoroutine(EnableDropAfterDelay());
        }

        /// <summary>
        /// 请求两个相同水果合成
        /// </summary>
        public void TryRequestMerge(FruitPiece first, FruitPiece second)
        {
            if (isGameOver || first == null || second == null || first.FruitType != second.FruitType)
            {
                return;
            }

            if (first.FruitType == FruitType.Watermelon)
            {
                return;
            }

            if (!first.TryLockForMerge())
            {
                return;
            }

            if (!second.TryLockForMerge())
            {
                first.UnlockMerge();
                return;
            }

            pendingMerges.Add(new MergeRequest
            {
                First = first,
                Second = second
            });
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        public void EndGame()
        {
            if (isGameOver)
            {
                return;
            }

            isGameOver = true;
            canDrop = false;
            if (hud != null)
            {
                hud.ShowGameOver(score);
            }
        }

        /// <summary>
        /// 判断当前游戏是否结束
        /// </summary>
        public bool IsGameOver()
        {
            return isGameOver;
        }

        private IEnumerator EnableDropAfterDelay()
        {
            yield return new WaitForSeconds(dropCooldown);
            canDrop = !isGameOver;
        }

        private void ProcessPendingMerges()
        {
            for (int i = 0; i < pendingMerges.Count; i++)
            {
                MergeRequest request = pendingMerges[i];
                if (request.First != null && request.Second != null)
                {
                    MergePieces(request.First, request.Second);
                }
            }

            pendingMerges.Clear();
        }

        private void MergePieces(FruitPiece first, FruitPiece second)
        {
            Vector3 mergePosition = (first.transform.position + second.transform.position) * 0.5f;
            Vector3 mergeVelocity = Vector3.zero;
            if (first.FruitRigidbody != null)
            {
                mergeVelocity += first.FruitRigidbody.linearVelocity;
            }

            if (second.FruitRigidbody != null)
            {
                mergeVelocity += second.FruitRigidbody.linearVelocity;
            }

            mergeVelocity *= 0.5f;
            FruitType mergedType = (FruitType)((int)first.FruitType + 1);
            activePieces.Remove(first);
            activePieces.Remove(second);
            Destroy(first.gameObject);
            Destroy(second.gameObject);

            FruitPiece mergedPiece = Instantiate(fruitPrefab, mergePosition, Quaternion.identity, fruitRoot);
            mergedPiece.Initialize(mergedType, this);
            if (mergedPiece.FruitRigidbody != null)
            {
                mergedPiece.FruitRigidbody.linearVelocity = mergeVelocity;
            }

            activePieces.Add(mergedPiece);
            score += ((int)mergedType + 1) * 10;
            if (hud != null)
            {
                hud.SetScore(score);
            }
        }

        private FruitType RollFruitType(int maxType)
        {
            int clampedMax = Mathf.Clamp(maxType, 0, (int)FruitType.Orange);
            return (FruitType)Random.Range(0, clampedMax + 1);
        }

        private void ClearActivePieces()
        {
            for (int i = activePieces.Count - 1; i >= 0; i--)
            {
                if (activePieces[i] != null)
                {
                    Destroy(activePieces[i].gameObject);
                }
            }

            activePieces.Clear();
            pendingMerges.Clear();
        }
    }
}
