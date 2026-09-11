using UnityEngine;

namespace FruitTowerDemo
{
    /// <summary>
    /// 水果等级
    /// </summary>
    public enum FruitType
    {
        Berry,
        Strawberry,
        Orange,
        Apple,
        Pear,
        Watermelon
    }

    /// <summary>
    /// 场景中的单个水果
    /// </summary>
    public sealed class FruitPiece : MonoBehaviour
    {
        /// <summary>
        /// 水果刚体
        /// </summary>
        [SerializeField] private Rigidbody fruitRigidbody;

        /// <summary>
        /// 水果主体渲染器
        /// </summary>
        [SerializeField] private Renderer fruitRenderer;

        /// <summary>
        /// 叶片渲染器
        /// </summary>
        [SerializeField] private Renderer leafRenderer;

        /// <summary>
        /// 果梗渲染器
        /// </summary>
        [SerializeField] private Renderer stemRenderer;

        /// <summary>
        /// 当前水果等级
        /// </summary>
        [SerializeField] private FruitType fruitType;

        private FruitTowerGameController gameController;
        private bool isInitialized;
        private bool isMergeLocked;

        /// <summary>
        /// 水果等级
        /// </summary>
        public FruitType FruitType => fruitType;

        /// <summary>
        /// 水果刚体
        /// </summary>
        public Rigidbody FruitRigidbody => fruitRigidbody;

        /// <summary>
        /// 水果是否正在合成
        /// </summary>
        public bool IsMergeLocked => isMergeLocked;

        /// <summary>
        /// 水果是否已经基本静止
        /// </summary>
        public bool IsSettled => fruitRigidbody == null || fruitRigidbody.IsSleeping() || fruitRigidbody.linearVelocity.sqrMagnitude < 0.04f;

        private static readonly Color[] FruitColors =
        {
            new Color(0.35f, 0.46f, 0.95f),
            new Color(0.95f, 0.22f, 0.30f),
            new Color(1.00f, 0.55f, 0.10f),
            new Color(0.82f, 0.16f, 0.20f),
            new Color(0.62f, 0.82f, 0.22f),
            new Color(0.16f, 0.68f, 0.32f)
        };

        private static readonly Vector3[] FruitScales =
        {
            new Vector3(0.72f, 0.72f, 0.72f),
            new Vector3(0.82f, 0.88f, 0.82f),
            new Vector3(0.95f, 0.90f, 0.95f),
            new Vector3(1.04f, 1.00f, 1.04f),
            new Vector3(1.12f, 1.18f, 1.12f),
            new Vector3(1.30f, 1.12f, 1.30f)
        };

        /// <summary>
        /// 初始化水果等级和所属控制器
        /// </summary>
        public void Initialize(FruitType type, FruitTowerGameController controller)
        {
            fruitType = type;
            gameController = controller;
            isInitialized = true;
            isMergeLocked = false;
            ApplyVisual();
        }

        /// <summary>
        /// 设置水果刚体状态
        /// </summary>
        public void SetPhysicsEnabled(bool enabled)
        {
            if (fruitRigidbody == null)
            {
                return;
            }

            fruitRigidbody.isKinematic = !enabled;
            fruitRigidbody.detectCollisions = enabled;
        }

        /// <summary>
        /// 设置水果抛射初速度
        /// </summary>
        public void Launch(Vector3 initialVelocity)
        {
            if (fruitRigidbody == null)
            {
                return;
            }

            fruitRigidbody.isKinematic = false;
            fruitRigidbody.detectCollisions = true;
            fruitRigidbody.linearVelocity = initialVelocity;
            fruitRigidbody.WakeUp();
        }

        /// <summary>
        /// 锁定水果避免重复合成
        /// </summary>
        internal bool TryLockForMerge()
        {
            if (isMergeLocked)
            {
                return false;
            }

            isMergeLocked = true;
            return true;
        }

        /// <summary>
        /// 解除水果合成锁
        /// </summary>
        internal void UnlockMerge()
        {
            isMergeLocked = false;
        }

        private void Awake()
        {
            if (fruitRigidbody == null)
            {
                fruitRigidbody = GetComponent<Rigidbody>();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isInitialized || isMergeLocked || gameController == null)
            {
                return;
            }

            FruitPiece otherPiece = collision.collider.GetComponentInParent<FruitPiece>();
            if (otherPiece == null || otherPiece == this)
            {
                return;
            }

            gameController.TryRequestMerge(this, otherPiece);
        }

        private void ApplyVisual()
        {
            int index = Mathf.Clamp((int)fruitType, 0, FruitColors.Length - 1);
            transform.localScale = FruitScales[index];

            if (fruitRenderer != null)
            {
                fruitRenderer.material.color = FruitColors[index];
            }

            if (leafRenderer != null)
            {
                leafRenderer.material.color = Color.Lerp(FruitColors[index], Color.white, 0.18f);
            }

            if (stemRenderer != null)
            {
                stemRenderer.material.color = new Color(0.20f, 0.12f, 0.05f);
            }
        }

        /// <summary>
        /// 获取水果颜色
        /// </summary>
        public static Color GetColor(FruitType type)
        {
            int index = Mathf.Clamp((int)type, 0, FruitColors.Length - 1);
            return FruitColors[index];
        }
    }
}
