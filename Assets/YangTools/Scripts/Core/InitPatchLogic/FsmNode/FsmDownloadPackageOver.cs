using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;


namespace GameMain
{
    internal class FsmDownloadPackageOver : IStateNode
    {
        private StateMachine machine;
        public FsmDownloadPackageOver() { }

        void IStateNode.OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }
        void IStateNode.OnEnter()
        {
            Debug.Log("YooAsset: 下载资源完毕");
            
            machine.ChangeState<FsmClearPackageCache>();
        }
        void IStateNode.OnUpdate()
        {
        }
        void IStateNode.OnExit()
        {
        }
    }
}