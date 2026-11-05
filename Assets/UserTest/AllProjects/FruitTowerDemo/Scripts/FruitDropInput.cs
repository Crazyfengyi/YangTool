using UnityEngine;
using UnityEngine.EventSystems;

namespace FruitTowerDemo
{
    /// <summary>
    /// 水果拖拽投放输入
    /// </summary>
    public sealed class FruitDropInput : MonoBehaviour
    {
        /// <summary>
        /// 游戏控制器
        /// </summary>
        [SerializeField] private FruitTowerGameController gameController;

        /// <summary>
        /// 输入映射使用的相机
        /// </summary>
        [SerializeField] private Camera inputCamera;

        /// <summary>
        /// 指针映射平面高度
        /// </summary>
        [SerializeField] private float inputPlaneHeight;

        /// <summary>
        /// 纵深从一端往返到另一端的完整周期
        /// </summary>
        [SerializeField] private float depthCycleRoundTrip = 1.2f;

        private bool isDragging;
        private bool isTouchInput;
        private int activeFingerId = -1;
        private float depthCycleTime;
        private Vector3 currentAimWorldPoint;
        private Vector2 lastPointerScreenPosition;

        private void Awake()
        {
            if (inputCamera == null)
            {
                inputCamera = Camera.main;
            }

            if (Mathf.Approximately(inputPlaneHeight, 0f))
            {
                inputPlaneHeight = 0.1f;
            }
        }

        private void Update()
        {
            if (gameController == null)
            {
                return;
            }

            bool beganAimThisFrame = TryBeginAim();
            if (!isDragging)
            {
                return;
            }

            if (!gameController.CanDrop)
            {
                ResetAim();
                return;
            }

            Vector2 pointerPosition = GetActivePointerPosition();
            if (!beganAimThisFrame)
            {
                depthCycleTime += Time.deltaTime;
            }

            UpdateAim(pointerPosition);
            if (HasReleasedPointer())
            {
                gameController.DropAtWorldPoint(currentAimWorldPoint);
                ResetAim();
            }
        }

        /// <summary>
        /// 重置输入预览状态
        /// </summary>
        public void ResetAim()
        {
            isDragging = false;
            isTouchInput = false;
            activeFingerId = -1;
            depthCycleTime = 0f;
            currentAimWorldPoint = Vector3.zero;
            lastPointerScreenPosition = Vector2.zero;
        }

        /// <summary>
        /// 尝试开始一次横向瞄准
        /// </summary>
        private bool TryBeginAim()
        {
            if (TryBeginTouch(out Touch touch))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return false;
                }

                if (gameController.CanDrop)
                {
                    BeginAim(touch.position, true, touch.fingerId);
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return false;
                }

                if (gameController.CanDrop)
                {
                    BeginAim(Input.mousePosition, false, -1);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 初始化瞄准状态并设置最小纵深
        /// </summary>
        private void BeginAim(Vector2 pointerPosition, bool touchInput, int fingerId)
        {
            isDragging = true;
            isTouchInput = touchInput;
            activeFingerId = fingerId;
            depthCycleTime = 0f;
            lastPointerScreenPosition = pointerPosition;

            currentAimWorldPoint = new Vector3(0f, 0f, gameController.MinDropZ);
            if (TryGetWorldPoint(pointerPosition, out Vector3 worldPoint))
            {
                currentAimWorldPoint.x = worldPoint.x;
            }

            gameController.SetAimWorldPoint(currentAimWorldPoint);
        }

        /// <summary>
        /// 根据指针横向位置和自动纵深刷新目标点
        /// </summary>
        private void UpdateAim(Vector2 pointerPosition)
        {
            lastPointerScreenPosition = pointerPosition;
            if (TryGetWorldPoint(pointerPosition, out Vector3 worldPoint))
            {
                currentAimWorldPoint.x = worldPoint.x;
            }

            float halfCycle = Mathf.Max(0.05f, depthCycleRoundTrip * 0.5f);
            float normalizedDepth = Mathf.PingPong(depthCycleTime / halfCycle, 1f);
            float aimZ = Mathf.Lerp(gameController.MinDropZ, gameController.MaxDropZ, normalizedDepth);
            currentAimWorldPoint = new Vector3(currentAimWorldPoint.x, 0f, aimZ);
            gameController.SetAimWorldPoint(currentAimWorldPoint);
        }

        private bool TryBeginTouch(out Touch touch)
        {
            touch = default;
            if (Input.touchCount == 0)
            {
                return false;
            }

            touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Began;
        }

        private Vector2 GetActivePointerPosition()
        {
            if (!isTouchInput)
            {
                return Input.mousePosition;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.fingerId == activeFingerId)
                {
                    return touch.position;
                }
            }

            return lastPointerScreenPosition;
        }

        private bool HasReleasedPointer()
        {
            if (!isTouchInput)
            {
                return Input.GetMouseButtonUp(0);
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.fingerId == activeFingerId)
                {
                    return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
                }
            }

            return false;
        }

        private bool TryGetWorldPoint(Vector2 screenPosition, out Vector3 worldPoint)
        {
            worldPoint = Vector3.zero;
            if (inputCamera == null)
            {
                return false;
            }

            Ray ray = inputCamera.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, inputPlaneHeight, 0f));
            if (!plane.Raycast(ray, out float distance))
            {
                return false;
            }

            worldPoint = ray.GetPoint(distance);
            return true;
        }

        private void OnValidate()
        {
            depthCycleRoundTrip = Mathf.Max(0.1f, depthCycleRoundTrip);
        }
    }
}
