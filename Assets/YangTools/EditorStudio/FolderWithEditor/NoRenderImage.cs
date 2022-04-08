using UnityEngine.UI;

namespace YangTools
{
    /// <summary>
    /// 无渲染Image,用作透明背景--->https://connect.unity.com/p/unityyou-hua-quan-ping-uide-overdrawzhi-imagebox?app=true
    /// </summary>
    public class NoRenderImage : Graphic
    {
        public override void Rebuild(CanvasUpdate update)
        {
        }
    }
}

