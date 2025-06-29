using YangTools;
using YooAsset;
using StateMachine = UniFramework.Machine.StateMachine;

namespace GameMain
{
    public class PatchOperation : GameAsyncOperation
    {
        private readonly StateMachine machine;
        private StepsType stepsType = StepsType.None;

        public PatchOperation(string packageName, string buildPipeline, EPlayMode playMode)
        {
            // 注册监听事件
            YangTools.YangExtend.AddEventListener<UserTryInitialize>(GameInit.Instance.gameObject, OnHandleEventMessage);
            YangTools.YangExtend.AddEventListener<UserBeginDownloadWebFiles>(GameInit.Instance.gameObject, OnHandleEventMessage);
            YangTools.YangExtend.AddEventListener<UserTryUpdatePackageVersion>(GameInit.Instance.gameObject, OnHandleEventMessage);
            YangTools.YangExtend.AddEventListener<UserTryUpdatePatchManifest>(GameInit.Instance.gameObject, OnHandleEventMessage);
            YangTools.YangExtend.AddEventListener<UserTryDownloadWebFiles>(GameInit.Instance.gameObject, OnHandleEventMessage);

            // 创建状态机
            machine = new StateMachine(this);
            machine.AddNode(new FsmInitializePackage());
            machine.AddNode(new FsmRequestPackageVersion());
            machine.AddNode(new FsmUpdatePackageManifest());
            machine.AddNode(new FsmCreatePackageDownloader());
            machine.AddNode(new FsmDownloadPackageFiles());
            machine.AddNode(new FsmDownloadPackageOver());
            machine.AddNode(new FsmClearPackageCache());
            machine.AddNode(new FsmReadyStartGame());
            machine.AddNode(new FsmLoadDone());

            machine.SetBlackboardValue("PackageName", packageName);
            machine.SetBlackboardValue("PlayMode", playMode);
            machine.SetBlackboardValue("BuildPipeline", buildPipeline);
        }

        protected override void OnStart()
        {
            stepsType = StepsType.Update;
            machine.Run<FsmInitializePackage>();
        }

        protected override void OnUpdate()
        {
            if (stepsType is StepsType.None or StepsType.Done)
            {
                return;
            }

            if (stepsType == StepsType.Update)
            {
                machine.Update();
                if (machine.CurrentNode == typeof(FsmLoadDone).FullName)
                {
                    YangEventManager.Instance.Clear();
                    Status = EOperationStatus.Succeed;
                    stepsType = StepsType.Done;
                }
            }
        }

        protected override void OnAbort()
        {
        }

        /// <summary>
        /// 接收事件
        /// </summary>
        private void OnHandleEventMessage(EventData eventData)
        {
            if (eventData.Args is UserTryInitialize)
            {
                machine.ChangeState<FsmInitializePackage>();
            }
            else if (eventData.Args is UserBeginDownloadWebFiles)
            {
                machine.ChangeState<FsmDownloadPackageFiles>();
            }
            else if (eventData.Args is UserTryUpdatePackageVersion)
            {
                machine.ChangeState<FsmRequestPackageVersion>();
            }
            else if (eventData.Args is UserTryUpdatePatchManifest)
            {
                machine.ChangeState<FsmUpdatePackageManifest>();
            }
            else if (eventData.Args is UserTryDownloadWebFiles)
            {
                machine.ChangeState<FsmCreatePackageDownloader>();
            }
            else
            {
                throw new System.NotImplementedException($"错误:{eventData.Name}");
            }
        }
    }

    public enum StepsType
    {
        None,
        Update,
        Done,
    }
}