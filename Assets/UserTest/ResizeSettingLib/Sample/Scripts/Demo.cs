using System;
using System.Collections.Generic;
using ResizeSettingLib.Core.Config;
using UnityEngine;

namespace Demo
{
    /// <summary>
    /// Demo演示
    /// </summary>
    public class Demo : MonoBehaviour
    {
        [SerializeField] private UISettingWindow m_SettingWindow;
        [SerializeField] private Sprite m_BtnSprite;

        private void Start()
        {
            //拖动类配置
            Dictionary<int, DragObjectConfigNode> dragObjectConfigNodes = new Dictionary<int, DragObjectConfigNode>()
            {
                {
                    1001, new DragObjectConfigNode()
                    {
                        name = "左侧摇杆",
                        id = 1001,
                        anchorType = AnchorTypeEnum.LeftDown,
                        anchorPosition = new Vector2(580, 380), //相对于左下角为坐标原点的坐标
                        size = 200,
                        sizeMin = 150,
                        sizeMax = 250,
                        sizeStep = 10,
                        sprite = m_BtnSprite,
                    }
                },
                {
                    1002, new DragObjectConfigNode()
                    {
                        name = "右侧摇杆",
                        id = 1002,
                        anchorType = AnchorTypeEnum.RightDown,
                        anchorPosition = new Vector2(-600, 400), //相对于右下角为坐标原点的坐标
                        size = 200,
                        sizeMin = 150,
                        sizeMax = 200,
                        sizeStep = 5,
                        sprite = m_BtnSprite,
                    }
                }
            };

            //限制区域类配置
            List<LimitAreaConfigNode> limitAreaConfigNodes = new List<LimitAreaConfigNode>()
            {
                //左上角
                new LimitAreaConfigNode()
                {
                    name = "左上角", id = 2001, limitIds = new List<int>() { 1001, 1002 }, limitX = (0, 0.3f),
                    limitY = (0.6f, 1f)
                },
                //右上角
                new LimitAreaConfigNode()
                {
                    name = "右上角", id = 2002, limitIds = new List<int>() { 1001, 1002 }, limitX = (0.8f, 1f),
                    limitY = (0.7f, 1f)
                },
                //左下角
                new LimitAreaConfigNode()
                {
                    name = "左下角", id = 2003, limitIds = new List<int>() { 1001, 1002 }, limitX = (0, 0.2f),
                    limitY = (0, 0.3f)
                },
                //右下角
                new LimitAreaConfigNode()
                {
                    name = "右下角", id = 2004, limitIds = new List<int>() { 1001, 1002 }, limitX = (0.8f, 1f),
                    limitY = (0, 0.4f)
                },
            };

            //组装配置数据
            ResizeConfigData resizeConfigData = new ResizeConfigData();
            resizeConfigData.buttonConfigNodes = dragObjectConfigNodes;
            resizeConfigData.limitAreaConfigNodes = limitAreaConfigNodes;

            //界面绑定回调
            m_SettingWindow.BindCallback(OnSaveResize);
            //打开界面并给初始配置数据
            m_SettingWindow.Open(resizeConfigData);
        }

        private void OnSaveResize(ResizeConfigData configData)
        {
            //TODO：configData是调整后的配置数据！
            //TODO：这里后续执行自己自己的逻辑！
        }
    }
}