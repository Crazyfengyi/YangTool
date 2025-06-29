using YangTools;
using YooAsset;

namespace GameMain
{
    public class UserTryUpdatePackageVersion : EventMessageBase
    {
        
    }
    
    public class UserTryUpdatePatchManifest : EventMessageBase
    {
        
    }
    
    public class UserResourcesReadyPress : EventMessageBase
    {
        public float press;
    }
    
    
    #region 资源下载流程

    /// <summary>
    /// 补丁流程步骤改变
    /// </summary>
    public class PatchStepsChange : EventMessageBase
    {
        public string Tips;
    }
    
    /// <summary>
    /// 补丁包初始化失败
    /// </summary>
    public class InitializeFailed : EventMessageBase
    {
    }
    /// <summary>
    /// 资源版本请求失败
    /// </summary>
    public class PackageVersionRequestFailed : EventMessageBase
    {
    }
    
    /// <summary>
    /// 资源清单更新失败
    /// </summary>
    public class PackageManifestUpdateFailed : EventMessageBase
    {
   
    }
    
    /// <summary>
    /// 发现更新文件
    /// </summary>
    public class FoundUpdateFiles : EventMessageBase
    {
        public int TotalCount;
        public long TotalSizeBytes;
    }

    /// <summary>
    /// 下载进度更新
    /// </summary>
    public class DownloadUpdate : EventMessageBase
    {
        public int TotalDownloadCount;
        public int CurrentDownloadCount;
        public long TotalDownloadSizeBytes;
        public long CurrentDownloadSizeBytes;
        
        public static void SendEventMessage(DownloadUpdateData updateData)
        {
            var msg = new DownloadUpdate
            {
                TotalDownloadCount = updateData.TotalDownloadCount,
                CurrentDownloadCount = updateData.CurrentDownloadCount,
                TotalDownloadSizeBytes = updateData.TotalDownloadBytes,
                CurrentDownloadSizeBytes = updateData.CurrentDownloadBytes
            };
            msg.SendEvent();
        }
    }
    
    /// <summary>
    /// 网络文件下载失败
    /// </summary>
    public class WebFileDownloadFailed : EventMessageBase
    {
        public string FileName;
        public string Error;
        
        public static void SendEventMessage(DownloadErrorData errorData)
        {
            var msg = new WebFileDownloadFailed
            {
                FileName = errorData.FileName,
                Error = errorData.ErrorInfo
            };
            msg.SendEvent();
        }
    }
    #endregion

    #region 用户事件

    /// <summary>
    /// 用户尝试再次初始化资源包
    /// </summary>
    public class UserTryInitialize : EventMessageBase
    {
   
    }

    /// <summary>
    /// 用户开始下载网络文件
    /// </summary>
    public class UserBeginDownloadWebFiles : EventMessageBase
    {
    
    }

    /// <summary>
    /// 用户尝试再次请求资源版本
    /// </summary>
    public class UserTryRequestPackageVersion : EventMessageBase
    {
  
    }

    /// <summary>
    /// 用户尝试再次更新补丁清单
    /// </summary>
    public class UserTryUpdatePackageManifest : EventMessageBase
    {
    
    }

    /// <summary>
    /// 用户尝试再次下载网络文件
    /// </summary>
    public class UserTryDownloadWebFiles : EventMessageBase
    {
     
    }
    #endregion
}