using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

internal class FsmRequestPackageVersion : IStateNode
{
    private StateMachine machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        this.machine = machine;
    }
    void IStateNode.OnEnter()
    {
        PatchStepsChange temp = new PatchStepsChange
        {
            Tips = "请求资源版本！"
        };
        temp.SendEvent();
        GameInit.Instance.StartCoroutine(UpdatePackageVersion());
    }
    void IStateNode.OnUpdate()
    {
    }
    void IStateNode.OnExit()
    {
    }

    private IEnumerator UpdatePackageVersion()
    {
        var packageName = (string)machine.GetBlackboardValue("PackageName");
        var package = YooAssets.GetPackage(packageName);
        var operation = package.RequestPackageVersionAsync();
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
            PackageVersionRequestFailed temp = new PackageVersionRequestFailed();
            temp.SendEvent();
        }
        else
        {
            Debug.Log($"Request package version: {operation.PackageVersion}");
            machine.SetBlackboardValue("PackageVersion", operation.PackageVersion);
            machine.ChangeState<FsmUpdatePackageManifest>();
        }
    }
}