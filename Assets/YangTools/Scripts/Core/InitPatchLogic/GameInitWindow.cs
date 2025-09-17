using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.UI;
using YangTools;
using YooAsset;

namespace YangTools
{
    public class GameInitWindow : MonoBehaviour
    {
        private readonly YangEventGroup eventGroup = new YangEventGroup();
        public readonly List<MessageBox> _msgBoxList = new List<MessageBox>();

        // UGUI相关
        private GameObject _messageBoxObj;
        private Slider _slider;
        private Text _tips;

        void Start()
        {
            _slider = transform.Find("UIWindow/Slider").GetComponent<Slider>();
            _tips = transform.Find("UIWindow/Slider/txt_tips").GetComponent<Text>();
            _tips.text = "Initializing the game world !";
            _messageBoxObj = transform.Find("UIWindow/MessgeBox").gameObject;
            _messageBoxObj.SetActive(false);

            eventGroup.AddListener<InitializeFailed>(OnHandleEventMessage);
            eventGroup.AddListener<PatchStepsChange>(OnHandleEventMessage);
            eventGroup.AddListener<FoundUpdateFiles>(OnHandleEventMessage);
            eventGroup.AddListener<DownloadUpdate>(OnHandleEventMessage);
            eventGroup.AddListener<PackageVersionRequestFailed>(OnHandleEventMessage);
            eventGroup.AddListener<PackageManifestUpdateFailed>(OnHandleEventMessage);
            eventGroup.AddListener<WebFileDownloadFailed>(OnHandleEventMessage);
        }

        void OnDestroy()
        {
            eventGroup.RemoveAllListener();
        }

        /// <summary>
        /// 接收事件
        /// </summary>
        private void OnHandleEventMessage(EventData message)
        {
            if (message.Args is InitializeFailed)
            {
                Action callback = () =>
                {
                    UserTryInitialize temp = new UserTryInitialize();
                    temp.SendEvent();
                };
                ShowMessageBox($"Failed to initialize package !", callback);
            }
            else if (message.Args is PatchStepsChange)
            {
                var msg = message.Args as PatchStepsChange;
                _tips.text = msg.Tips;
                Debug.Log(msg.Tips);
            }
            else if (message.Args is FoundUpdateFiles)
            {
                var msg = message.Args as FoundUpdateFiles;
                System.Action callback = () =>
                {
                    UserBeginDownloadWebFiles temp = new UserBeginDownloadWebFiles();
                    temp.SendEvent();
                };
                float sizeMB = msg.TotalSizeBytes / 1048576f;
                sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
                string totalSizeMB = sizeMB.ToString("f1");
                ShowMessageBox($"Found update patch files, Total count {msg.TotalCount} Total szie {totalSizeMB}MB",
                    callback);
            }
            else if (message.Args is DownloadUpdate)
            {
                var msg = message.Args as DownloadUpdate;
                _slider.value = (float)msg.CurrentDownloadCount / msg.TotalDownloadCount;
                string currentSizeMB = (msg.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
                string totalSizeMB = (msg.TotalDownloadSizeBytes / 1048576f).ToString("f1");
                _tips.text = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
            }
            else if (message.Args is PackageVersionRequestFailed)
            {
                System.Action callback = () =>
                {
                    UserTryRequestPackageVersion temp = new UserTryRequestPackageVersion();
                    temp.SendEvent();
                };
                ShowMessageBox($"Failed to request package version, please check the network status.", callback);
            }
            else if (message.Args is PackageManifestUpdateFailed)
            {
                System.Action callback = () =>
                {
                    UserTryUpdatePackageManifest temp = new UserTryUpdatePackageManifest();
                    temp.SendEvent();
                };
                ShowMessageBox($"Failed to update patch manifest, please check the network status.", callback);
            }
            else if (message.Args is WebFileDownloadFailed)
            {
                var msg = message.Args as WebFileDownloadFailed;
                System.Action callback = () =>
                {
                    UserTryDownloadWebFiles temp = new UserTryDownloadWebFiles();
                    temp.SendEvent();
                };
                ShowMessageBox($"Failed to download file : {msg.FileName}", callback);
            }
            else
            {
                throw new System.NotImplementedException($"{message.GetType()}");
            }
        }

        /// <summary>
        /// 显示对话框
        /// </summary>
        private void ShowMessageBox(string content, System.Action ok)
        {
            //尝试获取一个可用的对话框
            MessageBox msgBox = null;
            for (int i = 0; i < _msgBoxList.Count; i++)
            {
                var item = _msgBoxList[i];
                if (item.ActiveSelf == false)
                {
                    msgBox = item;
                    break;
                }
            }

            //如果没有可用的对话框，则创建一个新的对话框
            if (msgBox == null)
            {
                msgBox = new MessageBox();
                var cloneObject = GameObject.Instantiate(_messageBoxObj, _messageBoxObj.transform.parent);
                msgBox.Create(cloneObject);
                _msgBoxList.Add(msgBox);
            }

            //显示对话框
            msgBox.Show(content, ok);
        }
    }

    /// <summary>
    /// 对话框封装类
    /// </summary>
    public class MessageBox
    {
        public bool ActiveSelf => _cloneObject.activeSelf;

        private GameObject _cloneObject;
        private Text _content;
        private Button _btnOK;
        private System.Action _clickOK;

        public void Create(GameObject cloneObject)
        {
            _cloneObject = cloneObject;
            _content = cloneObject.transform.Find("txt_content").GetComponent<Text>();
            _btnOK = cloneObject.transform.Find("btn_ok").GetComponent<Button>();
            _btnOK.onClick.AddListener(OnClickYes);
        }

        public void Show(string content, System.Action clickOK)
        {
            _content.text = content;
            _clickOK = clickOK;
            _cloneObject.SetActive(true);
            _cloneObject.transform.SetAsLastSibling();
        }

        public void Hide()
        {
            _content.text = string.Empty;
            _clickOK = null;
            _cloneObject.SetActive(false);
        }

        private void OnClickYes()
        {
            _clickOK?.Invoke();
            Hide();
        }
    }
}