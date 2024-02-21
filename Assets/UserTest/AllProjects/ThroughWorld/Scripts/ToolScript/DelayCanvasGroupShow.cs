using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GameCode
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DelayCanvasGroupShow : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private bool isShow;
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            _canvasGroup.alpha = 0;
            isShow = false;
        }

        public void DelayShow()
        {
            if(isShow)return;
            _canvasGroup.DOFade(1, 0.2f);
            isShow = true;
        }
    }
}
