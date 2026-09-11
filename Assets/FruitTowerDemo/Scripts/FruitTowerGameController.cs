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
        /// 果盘抛射目标参考点
        /// </summary>
        [SerializeField] private Transform throwTarget;

        /// <summary>
        /// 抛射路线预览控制器
        /// </summary>
        [SerializeField] private FruitTrajectoryPreview trajectoryPreview;

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
        /// 最小落点纵深坐标
        /// </summary>
        [SerializeField] private float minDropZ = -1.75f;

        /// <summary>
        /// 最大落点纵深坐标
        /// </summary>
        [SerializeField] private float maxDropZ = 1.75f;

        /// <summary>
        /// 两次投放之间的间隔
        /// </summary>
        [SerializeField] private float dropCooldown = 0.28f;

        /// <summary>
        /// 抛物线最高点相对目标的高度
        /// </summary>
        [SerializeField] private float throwArcHeight = 0.8f;

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
        /// 当前抛射目标点
        /// </summary>
        private Vector3 currentAimWorldPoint;

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

        /// <summary>
        /// 目标点最小纵深坐标
        /// </summary>
        public float MinDropZ => minDropZ;

        /// <summary>
        /// 目标点最大纵深坐标
        /// </summary>
        public float MaxDropZ => maxDropZ;

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

            SetAimWorldPoint(Vector3.zero);
        }

        /// <summary>
        /// 设置当前抛射目标点预览
        /// </summary>
        public void SetAimWorldPoint(Vector3 worldPoint)
        {
            Vector3 targetPosition = ClampTargetPoint(worldPoint);
            currentAimWorldPoint = targetPosition;
            if (aimMarker != null)
            {
                aimMarker.position = targetPosition;
            }

            if (trajectoryPreview != null && CanDrop)
            {
                trajectoryPreview.Refresh(targetPosition);
            }
        }

        /// <summary>
        /// 根据目标点抛射水果
        /// </summary>
        public void DropAtWorldPoint(Vector3 worldPoint)
        {
            if (!CanDrop || fruitPrefab == null || spawnAnchor == null)
            {
                return;
            }

            Vector3 spawnPosition = spawnAnchor.position;
            Vector3 targetPosition = ClampTargetPoint(worldPoint);
            currentAimWorldPoint = targetPosition;
            if (aimMarker != null)
            {
                aimMarker.position = targetPosition;
            }

            FruitPiece piece = Instantiate(fruitPrefab, spawnPosition, Quaternion.identity, fruitRoot);
            piece.Initialize(currentFruitType, this);
            piece.Launch(CalculateThrowVelocity(spawnPosition, targetPosition, out _));
            activePieces.Add(piece);

            currentFruitType = nextFruitType;
            nextFruitType = RollFruitType(initialRandomMaxType);
            canDrop = false;

            if (hud != null)
            {
                hud.SetNextFruit(nextFruitType);
            }

            StartCoroutine(EnableDropAfterDelay());
            if (trajectoryPreview != null)
            {
                trajectoryPreview.Hide();
            }
        }

        /// <summary>
        /// 兼容旧版横向目标接口
        /// </summary>
        public void SetAimWorldX(float worldX)
        {
            SetAimWorldPoint(new Vector3(worldX, 0f, 0f));
        }

        /// <summary>
        /// 兼容旧版横向投放接口
        /// </summary>
        public void DropAtWorldX(float worldX)
        {
            DropAtWorldPoint(new Vector3(worldX, 0f, 0f));
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
            if (trajectoryPreview != null)
            {
                trajectoryPreview.Hide();
            }
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
            if (canDrop && trajectoryPreview != null)
            {
                trajectoryPreview.Refresh(currentAimWorldPoint);
            }
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

        /// <summary>
        /// 限制目标点在果盘有效范围内
        /// </summary>
        private Vector3 ClampTargetPoint(Vector3 worldPoint)
        {
            float targetY = throwTarget != null ? throwTarget.position.y : 0.55f;
            float targetZ = throwTarget != null ? worldPoint.z : 0f;
            return new Vector3(
                Mathf.Clamp(worldPoint.x, minDropX, maxDropX),
                targetY,
                Mathf.Clamp(targetZ, minDropZ, maxDropZ));
        }

        /// <summary>
        /// 计算水果的抛射路线采样点
        /// </summary>
        public bool TryGetThrowTrajectory(Vector3 worldPoint, Vector3[] positions)
        {
            if (spawnAnchor == null || positions == null || positions.Length < 2)
            {
                return false;
            }

            Vector3 origin = spawnAnchor.position;
            Vector3 target = ClampTargetPoint(worldPoint);
            Vector3 velocity = CalculateThrowVelocity(origin, target, out float totalFlightTime);
            for (int i = 0; i < positions.Length; i++)
            {
                float normalizedTime = i / (positions.Length - 1f);
                float time = totalFlightTime * normalizedTime;
                positions[i] = origin + velocity * time + 0.5f * Physics.gravity * time * time;
            }

            return true;
        }

        /// <summary>
        /// 根据目标点计算真实抛射初速度和飞行时间
        /// </summary>
        private Vector3 CalculateThrowVelocity(Vector3 origin, Vector3 target, out float totalFlightTime)
        {
            float gravityMagnitude = Mathf.Abs(Physics.gravity.y);
            if (gravityMagnitude < 0.01f)
            {
                totalFlightTime = 1f;
                return (target - origin).normalized;
            }

            float apexY = Mathf.Max(origin.y, target.y) + Mathf.Max(0.1f, throwArcHeight);
            float timeToApex = Mathf.Sqrt(2f * (apexY - origin.y) / gravityMagnitude);
            float timeFromApex = Mathf.Sqrt(2f * (apexY - target.y) / gravityMagnitude);
            totalFlightTime = Mathf.Max(0.1f, timeToApex + timeFromApex);
            Vector3 horizontalVelocity = new Vector3(target.x - origin.x, 0f, target.z - origin.z) / totalFlightTime;
            return horizontalVelocity + Vector3.up * gravityMagnitude * timeToApex;
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
