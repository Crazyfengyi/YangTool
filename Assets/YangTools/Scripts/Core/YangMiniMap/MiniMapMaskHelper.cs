using UnityEngine;
using UnityEngine.UI;

namespace YangTools.MiniMap
{
    /// <summary>
    /// 小地图遮罩
    /// </summary>
    public class MiniMapMaskHelper : MonoSingleton<MiniMapMaskHelper>
    {
        [Header("Mask")]
        public Sprite MiniMapMask;

        public Sprite WorldMapMask;

        [Header("References")]
        [SerializeField]
        private Image Background;

        [SerializeField]
        private Sprite MiniMapBackGround;

        [SerializeField]
        private Sprite WorldMapBackGround;

        [SerializeField]
        private RectTransform MaskIconRoot;

        private Image _image = null;
        private Image m_image
        {
            get
            {
                if (_image == null)
                {
                    _image = this.GetComponent<Image>();
                }
                return _image;
            }
        }

        /// <summary>
        /// 设置被遮罩的Iocn
        /// </summary>
        public void SetMaskedIcon(RectTransform trans)
        {
            trans.SetParent(MaskIconRoot);
        }

        /// <summary>
        /// 大地图与小地图切换
        /// </summary>
        public void OnChange(bool full = false)
        {
            if (full)
            {
                m_image.sprite = WorldMapMask;
                if (Background != null) { Background.sprite = WorldMapBackGround; }
            }
            else
            {
                m_image.sprite = MiniMapMask;
                if (Background != null) { Background.sprite = MiniMapBackGround; }
            }
        }
    }
}