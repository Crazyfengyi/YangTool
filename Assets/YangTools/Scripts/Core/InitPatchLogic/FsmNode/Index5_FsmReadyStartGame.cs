using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniFramework.Machine;
using UnityEngine;
using YangTools.Scripts.Core.ResourceManager;
using YangTools.Scripts.Core.YangSaveData;
using YooAsset;

namespace GameMain
{
    internal class FsmReadyStartGame : IStateNode
    {
        private StateMachine machine;
        public FsmReadyStartGame() { }
        void IStateNode.OnCreate(StateMachine machine)
        {
            this.machine = machine;
        }

        async void IStateNode.OnEnter()
        {
            var gamePackage = YooAssets.GetPackage("DefaultPackage");
            YooAssets.SetDefaultPackage(gamePackage);
            UserResourcesReadyPress press = new UserResourcesReadyPress();
            press.press = 0.2f;
            press.SendEvent();
           
            //配置表
            // await .Instance.LoadAllConfigs();

            press.press = 0.5f;
            press.SendEvent();
            
            GameInit.Instance.InitializeAfterManagers();
            
            //预加载资源
            var preloadList = new List<string>()
            {
            };

            for (int i = 0; i < preloadList.Count; i++)
            {
                press.press = 0.5f + 0.5f / preloadList.Count * i;
                press.SendEvent();
                await ResourceManager.LoadAssetAsync<Object>(preloadList[i]);
            }
            
            if (YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_GameData>().isFirstEnter)
            {
                YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_GameData>().isFirstEnter = false;
                //TODO:进入游戏
                //await UIWindowTool.ChangeToBattleScene();
            }
            else
            {
                //await UIWindowTool.ChangeToMainUIScene(); 
            }
            press.press = 1f;
            press.SendEvent();
            machine.ChangeState<FsmLoadDone>();
        }

        void IStateNode.OnUpdate()
        {
        }

        void IStateNode.OnExit()
        {
        }
    }
}