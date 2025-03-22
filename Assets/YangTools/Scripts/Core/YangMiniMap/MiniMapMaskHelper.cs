using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YangTools.Scripts.Core.YangToolsManager;

namespace YangTools.Scripts.Core.YangMiniMap
{
    /// <summary>
    /// 小地图遮罩
    /// </summary>
    public class MiniMapMaskHelper : MonoSingleton<MiniMapMaskHelper>
    {
        [FormerlySerializedAs("MiniMapMask")] [Header("Mask")]
        public Sprite miniMapMask;

        [FormerlySerializedAs("WorldMapMask")] public Sprite worldMapMask;

        [FormerlySerializedAs("Background")]
        [Header("References")]
        [SerializeField]
        private Image background;

        [FormerlySerializedAs("MiniMapBackGround")] [SerializeField]
        private Sprite miniMapBackGround;

        [FormerlySerializedAs("WorldMapBackGround")] [SerializeField]
        private Sprite worldMapBackGround;

        [FormerlySerializedAs("MaskIconRoot")] [SerializeField]
        private RectTransform maskIconRoot;

        private Image image = null;
        private Image MImage
        {
            get
            {
                if (image == null)
                {
                    image = this.GetComponent<Image>();
                }
                return image;
            }
        }

        /// <summary>
        /// 设置被遮罩的Icon
        /// </summary>
        public void SetMaskedIcon(RectTransform trans)
        {
            trans.SetParent(maskIconRoot);
        }

        /// <summary>
        /// 大地图与小地图切换
        /// </summary>
        public void OnChange(bool full = false)
        {
            if (full)
            {
                if (MImage) MImage.sprite = worldMapMask;
                if (background != null) { background.sprite = worldMapBackGround; }
            }
            else
            {
                if (MImage) MImage.sprite = miniMapMask;
                if (background != null) { background.sprite = miniMapBackGround; }
            }
        }
    }
}