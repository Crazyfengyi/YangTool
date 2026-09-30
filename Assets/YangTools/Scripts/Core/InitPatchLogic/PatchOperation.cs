using YangTools;
using YooAsset;
using System.Collections.Generic;
using StateMachine = UniFramework.Machine.StateMachine;

namespace GameMain
{
    /// <summary>
    /// 热更新流程 仅管理自身注册的事件监听
    /// </summary>
    public class PatchOperation : GameAsyncOperation
    {
        private readonly StateMachine machine;
        private StepsType stepsType = StepsType.None;
        /// <summary>
        /// 本次操作持有的监听句柄
        /// </summary>
        private readonly List<EventInfo> listeners = new List<EventInfo>();

        /// <summary>
        /// 注册操作事件并创建更新状态机
        /// </summary>
        public PatchOperation(string packageName, string buildPipeline, EPlayMode playMode)
        {
            // 注册监听事件
            listeners.Add(YangExtend.AddEventListener<UserTryInitialize>(GameInit.Instance.gameObject, OnHandleEventMessage));
            listeners.Add(YangExtend.AddEventListener<UserBeginDownloadWebFiles>(GameInit.Instance.gameObject, OnHandleEventMessage));
            listeners.Add(YangExtend.AddEventListener<UserTryUpdatePackageVersion>(GameInit.Instance.gameObject, OnHandleEventMessage));
            listeners.Add(YangExtend.AddEventListener<UserTryUpdatePatchManifest>(GameInit.Instance.gameObject, OnHandleEventMessage));
            listeners.Add(YangExtend.AddEventListener<UserTryDownloadWebFiles>(GameInit.Instance.gameObject, OnHandleEventMessage));

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

        /// <summary>
        /// 开始执行更新状态机
        /// </summary>
        protected override void OnStart()
        {
            stepsType = StepsType.Update;
            machine.Run<FsmInitializePackage>();
        }

        /// <summary>
        /// 推进更新并在完成后释放监听
        /// </summary>
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
                    ReleaseListeners();
                    Status = EOperationStatus.Succeed;
                    stepsType = StepsType.Done;
                }
            }
        }

        /// <summary>
        /// 中止操作时释放本次监听
        /// </summary>
        protected override void OnAbort()
        {
            ReleaseListeners();
            stepsType = StepsType.Done;
        }

        /// <summary>
        /// 注销自身监听 不影响其他模块 重复调用安全
        /// </summary>
        private void ReleaseListeners()
        {
            for (int i = 0; i < listeners.Count; i++) YangExtend.RemoveEventListener(listeners[i]);
            listeners.Clear();
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
