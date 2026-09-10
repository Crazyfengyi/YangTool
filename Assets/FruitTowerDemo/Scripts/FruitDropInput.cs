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

        private bool isDragging;
        private bool isTouchInput;
        private int activeFingerId = -1;

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

            if (!isDragging && TryBeginTouch(out Touch touch))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return;
                }

                if (!gameController.CanDrop)
                {
                    return;
                }

                isDragging = true;
                isTouchInput = true;
                activeFingerId = touch.fingerId;
            }

            if (!isDragging && Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                if (gameController.CanDrop)
                {
                    isDragging = true;
                    isTouchInput = false;
                }
            }

            if (!isDragging)
            {
                return;
            }

            Vector2 pointerPosition = isTouchInput ? GetTouchPosition() : Input.mousePosition;
            if (TryGetWorldX(pointerPosition, out float worldX))
            {
                gameController.SetAimWorldX(worldX);
            }

            if (HasReleasedPointer())
            {
                isDragging = false;
                isTouchInput = false;
                activeFingerId = -1;
                if (TryGetWorldX(pointerPosition, out float releaseWorldX))
                {
                    gameController.DropAtWorldX(releaseWorldX);
                }
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

        private Vector2 GetTouchPosition()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.fingerId == activeFingerId)
                {
                    return touch.position;
                }
            }

            return Vector2.zero;
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

        private bool TryGetWorldX(Vector2 screenPosition, out float worldX)
        {
            worldX = 0f;
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

            worldX = ray.GetPoint(distance).x;
            return true;
        }
    }
}
