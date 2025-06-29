using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using UniFramework.Machine;
using StateMachine = UniFramework.Machine.StateMachine;

namespace GameMain
{
    internal class FsmClearPackageCache : IStateNode
    {
        private StateMachine machine;

        public FsmClearPackageCache()
        {
        }

        public void OnCreate(StateMachine machine)
        {
        }

        void IStateNode.OnEnter()
        {
            PatchStepsChange temp = new PatchStepsChange
            {
                Tips = "清理未使用的缓存文件!"
            };
            temp.SendEvent();
  
            var packageName = (string)machine.GetBlackboardValue("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            operation.Completed += Operation_Completed;
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }

        private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
            Debug.Log("YooAsset: 清理未使用的缓存文件完成");
            machine.ChangeState<FsmReadyStartGame>();
        }
    }
}