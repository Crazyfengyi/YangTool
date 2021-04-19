
/// <summary>
/// 无渲染Image,用作透明背景--->https://connect.unity.com/p/unityyou-hua-quan-ping-uide-overdrawzhi-imagebox?app=true
/// </summary>
namespace UnityEngine.UI
{
    public class NoRenderImage : Graphic
    {
        public override void Rebuild(CanvasUpdate update)
        {
        }
    }
}

