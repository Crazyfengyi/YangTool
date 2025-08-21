using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Resources;
using System.Threading.Tasks;
using YangTools.Scripts.Core;

public class YangGuideManager : MonoSingleton<YangGuideManager>
{
    public CommonGuideWindow guideWindow;
    //储存所有新手教程信息的字典<id,GuideInfo>
    private Dictionary<int, GuideInfo> guideInfoDict = new Dictionary<int, GuideInfo>();
    protected override void Awake()
    {
        base.Awake();
        gameObject.name = "GuideEventManager";
    }
    /// <summary>
    /// 是否有引导
    /// </summary>
    public bool HaveGuideInfo(int id)
    {
        return guideInfoDict.ContainsKey(id);
    }
    /// <summary>
    /// 添加引导信息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="guideInfo"></param>
    /// <param name="trans"></param>
    public void AddGuideInfoDict(int id, GuideInfo guideInfo, Transform trans)
    {
        //尝试加入
        if (!guideInfoDict.TryAdd(id, guideInfo))
        {
            guideInfoDict[id] = guideInfo;//覆盖
            // Debug.LogError($"{trans.name}的guideID-{id}重复,物体位于{trans.parent.name}");
            return;
        }
    }
    /// <summary>
    /// 获得引导信息
    /// </summary>
    /// <param name="id"></param>
    public GuideInfo GetGuideInfo(int id)
    {
        if (id != 0)
        {
            if (guideInfoDict.TryGetValue(id, value: out var info))
            {
                Debug.Log($"有这个教程 - {id}");
                return info;
            }
            else
            {
                Debug.Log($"未找到教程 - {id}");
            }
        }
        else
        {
            Debug.LogError($"没有这个教程 - {id}");
        }
        return new GuideInfo(id, null);
    }
  
    #region 游戏教程触发检查
    //返回主界面
    public async void BackMainUI()
    {
        // //第一次返回主界面
        // bool tempValue = LocalSaveMgr.Instance.DataCenter.GetSaveGuide().IsOverGuide(1001);
        // //有星星
        // bool haveStart = BagMgr.Instance.BagPropEnough(1002, 1);
        //
        // if (!tempValue && HaveGuideInfo(1001) && haveStart)
        // {
        //     LocalSaveMgr.Instance.DataCenter.GetSaveGuide().AddRecordOverId(1001);
        //     guideWindow.ShowGuideById(1001);
        //     return;
        // }
        
        // //延迟下，有Layout适配
        // await Task.Yield();
        // bool tempValue2 = LocalSaveMgr.Instance.DataCenter.GetSaveGuide().IsOverGuide(1004);
        // //有小鱼干
        // if (tempValue && !tempValue2 && HaveGuideInfo(1004) && BagMgr.Instance.BagPropEnough(1003,1)) 
        // {
        //     LocalSaveMgr.Instance.DataCenter.GetSaveGuide().AddRecordOverId(1004);
        //     guideWindow.ShowGuideById(1004);
        // }
    }
    
    /// <summary>
    /// 主界面聚焦时间
    /// </summary>
    public void MainOnTop()
    {
        //第二关引导
        if (HaveGuideInfo(1001)) 
        {
            //LocalSaveMgr.Instance.DataCenter.GetSaveGuide().AddRecordOverId(1001);
            guideWindow.ShowGuideById(1001);
        }
    }
    /// <summary>
    /// 第一次打开场景进度页面
    /// </summary>
    public void FirstOpenPartyProgressWindow()
    {
        //bool tempValue = LocalSaveMgr.Instance.DataCenter.GetSaveGuide().IsOverGuide(1003);
        // if (!tempValue && HaveGuideInfo(1003)) 
        {
            //LocalSaveMgr.Instance.DataCenter.GetSaveGuide().AddRecordOverId(1003);
            guideWindow.ShowGuideById(1003);
        }
    }

    #endregion

    #region Event

    #region 第一次进入游戏教程
    public void FirstEnterUIGuideOver()
    { 
        //PlayerData.instance.firstEnterGame = true;
        Debug.Log("触发事件 FirstEnterUIGuideOver");
    }

    public void ShowArrow()
    {
        guideWindow.SetArrowState(true);
    }

    public void HideArrow()
    {
        guideWindow.SetArrowState(false);
    }
    #endregion

    #region 第一次升级教程
    public void EnterPropertyPanel()
    {
//         UIManager.instance.ShowPanel(PanelType.PropertyPanel);
//         PropertyPanel.Instance.ChangeTab(0);
//         PropertyPanel.Instance.toggles[0].isOn = true;
    }

    public void FirstUpgradeGuideOver()
    {
        //PlayerData.Instance.playerData.guideInfo.firstUpgrade = true;
    }
    #endregion

    /// <summary>
    /// 引导事件1003结束
    /// </summary>
    public void Guide1003Over()
    {
        // var panel = UIWindowMgr.Instance.GetWindow<PuzzleWindow>();
        // if (panel != null)
        // {
        //     panel.Guide1003Over();
        // }
    }
    #endregion
}

/// <summary>
/// 引导类型
/// </summary>
public class GuideInfo
{
    public int guideID;//新手教程ID
    public RectTransform trans;
    public string guideText;//文字
    public bool isShowBG;//是否显示背景
    public Sprite roleSprite;//角色图片
    public string startEvent;//开始事件
    public string clickEvent;//点击事件
    public bool haveFinger;//是否有手指
    public GuideShowType showType;//显示类型
    public MaskType maskType;//遮罩类型
    public bool haveClickArea;//有无点击区域
    public int nextID;//下一个引导ID
    public DialogDir dialogDir;//对话方向

    //在构造函数中将新手教程所需要的信息都加载进来
    public GuideInfo(int guideID, RectTransform trans)
    {
        this.guideID = guideID;
        this.trans = trans;
        // GuideSetConfig data = ConfigMgr.Instance.Tables.GuideSetCategory.GetOrDefault(guideID);
        // isShowBG = data.IsShowBg;
        // guideText = data.GuideText;
        // if (!string.IsNullOrEmpty(data.RoleSprite)) GetRes(data);
        // startEvent = data.StartEvent;
        // clickEvent = data.ClickEvent;
        // haveFinger = data.HaveFinger;
        // haveClickArea = data.HaveClickArea;
        // if (Enum.TryParse<GuideShowType>(data.ShowType, out var type))
        // {
        //     showType = type;
        // }
        // if (Enum.TryParse<MaskType>(data.MaskType, out var type2))
        // {
        //     maskType = type2;
        // }
        // if (Enum.TryParse<DialogDir>(data.DialogDir, out var type3))
        // {
        //     dialogDir = type3;
        // }
    }

    // public async void GetRes(GuideSetConfig data)
    // {
    //     roleSprite = await ResourceMgr.Instance.LoadSprite(data.RoleSprite);
    // }
}

/// <summary>
/// 引导显示类型
/// </summary>
public enum GuideShowType
{
    None,
    Clone,
    Mask
}
/// <summary>
/// 遮罩类型
/// </summary>
public enum MaskType
{
    None,
    Circle,
    Rect,
}
/// <summary>
/// 对话位置
/// </summary>
public enum DialogDir
{
    Left,
    Right,
    Top,
    Bottom,
    Mid
}
