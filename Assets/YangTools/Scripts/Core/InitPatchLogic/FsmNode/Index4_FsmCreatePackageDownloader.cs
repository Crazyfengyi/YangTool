using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;
using YooAsset;

namespace GameMain
{
    public class FsmCreatePackageDownloader : IStateNode
    {
        private StateMachine machine;

        public FsmCreatePackageDownloader()
        {
        }

        void IStateNode.OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }

        void IStateNode.OnEnter()
        {
            PatchStepsChange temp = new PatchStepsChange
            {
                Tips = "创建补丁下载器!"
            };
            temp.SendEvent();
            GameInit.Instance.StartCoroutine(CreateDownloader());
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        IEnumerator CreateDownloader()
        {
            yield return null;

            var packageName = (string)machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            machine.SetBlackboardValue("Downloader", downloader);

            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("YooAsset:不需要下载资源");
                machine.ChangeState<FsmReadyStartGame>();
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;
                Debug.Log($"YooAsset: 需要下载资源 totalDownloadCount:{totalDownloadCount} totalDownloadBytes:{totalDownloadBytes}");
                FoundUpdateFiles temp = new FoundUpdateFiles()
                {
                    TotalCount = totalDownloadCount,
                    TotalSizeBytes = totalDownloadBytes
                };
                temp.SendEvent();
            }
        }
    }
}