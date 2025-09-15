using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using YangTools;

namespace GameMain
{
    public enum PlatformType
    {
        [LabelText("默认")] Default,
        [LabelText("微信")] WeiXin,
        [LabelText("抖音")] DouYin,
    }

    [Serializable]
    public class PlatformData
    {
        public string appId;
        public string appKey;
        public string appSecret;

        public string adsId;
        public string gameTag;
    }

    public class GetOpenIdSuccess : EventMessageBase
    {
        public string openId;
    }

    public class SaveDataToServer : EventMessageBase
    {
       
    }
}