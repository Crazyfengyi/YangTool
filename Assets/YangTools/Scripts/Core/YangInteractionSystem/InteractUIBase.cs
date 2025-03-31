using UnityEngine;
using UnityEngine.EventSystems;

namespace YangObjectInteract
{
    /// <summary>
    /// 互动UI基类
    /// </summary>
    public abstract class InteractUIBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        public bool enableInteract = true;
        private ICanFocus focusTarget = null;
        private ICanSelect selectTarget = null;
        private ICanDrag currentDrag = null;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!enableInteract) return;
            focusTarget = this as ICanFocus;
            selectTarget = this as ICanSelect;
            currentDrag = this as ICanDrag;
            if (focusTarget is {EnableFocus: true})
            {
                YangInteractSystem.Instance.SetCurrentFocused(focusTarget, true);
            }

            if (selectTarget is {EnableSelect: true})
            {
                YangInteractSystem.Instance.ReadySelect = selectTarget;
            }

            if (currentDrag is {EnableDrag: true})
            {
                YangInteractSystem.Instance.ReadyDrag = currentDrag;
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!enableInteract) return;
            if (selectTarget != null && selectTarget.EnableSelect)
            {
                YangInteractSystem.Instance.ReadySelect = selectTarget;
            }

            if (currentDrag != null && currentDrag.EnableDrag)
            {
                YangInteractSystem.Instance.ReadyDrag = currentDrag;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enableInteract) return;
            if (focusTarget != null && focusTarget.EnableFocus)
            {
                if (YangInteractSystem.Instance.CurrentFocused == focusTarget)
                {
                    YangInteractSystem.Instance.SetCurrentFocused(null, true);
                }
            }

            if (selectTarget != null && selectTarget.EnableSelect)
            {
                if (YangInteractSystem.Instance.ReadySelect == selectTarget)
                    YangInteractSystem.Instance.ReadySelect = null;
            }

            if (currentDrag != null && currentDrag.EnableDrag)
            {
                if (YangInteractSystem.Instance.ReadyDrag == currentDrag)
                    YangInteractSystem.Instance.ReadyDrag = null;
            }
        }
    }
}