using System;
using System.Collections;
using GameMain;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CommonGuideWindow : MonoBehaviour, ICanvasRaycastFilter 
{
    public TextMeshProUGUI dialogText; //对话文字
    public GuideCircleMaskController guideCircleMask; //圆形遮罩
    public GuideRectMaskController guideRectMask; //矩形遮罩
    public GameObject maskContainer; //遮罩容器 
    public GameObject bg; //背景
    public RectTransform cloneContainer; //克隆物体容器
    public RectTransform clickArea; //点击区域，默认全屏，如果有克隆物体则只能点击克隆物体
    public Image leftRoleSprite, rightRoleSprite, dialogBgSprite; //图片
    public GameObject fingerObj; //手指
    public GameObject arrow; //箭头
    public bool isInLoading = false; //是否在加载中

    private GameObject curCloneGameObj; //当前克隆的物体
    private int curGuideId = 0; //当前引导id
    private GuideInfo guideInfo; //当前引导信息

    private void Awake()
    {
        clickArea.GetComponent<Button>().onClick.AddListener(ClickAreaOnClick);
    }

    /// <summary>
    /// 设置引导数据
    /// </summary>
    private void SetDataById(int id)
    {
        if (curCloneGameObj != null)
        {
            Destroy(curCloneGameObj);
        }

        curGuideId = id;
        guideInfo = YangGuideManager.Instance.GetGuideInfo(id);
        //存档
        //PlayerData.instance.guideId = curGuideId;
        Vector2 targetTranCanvasPos = Vector3.zero;
        if (guideInfo.trans != null)
        {
            //targetTranCanvasPos = WorldToCanvasPos(UIWindowMgr.Instance.canvas, guideInfo.trans.position);
        }
        
        bool isShowBg = guideInfo.isShowBG;
        bg.gameObject.SetActive(isShowBg);
        if (!string.IsNullOrEmpty(guideInfo.guideText))
        {
            dialogText.gameObject.SetActive(true);
            dialogText.text = guideInfo.guideText;
            dialogBgSprite.gameObject.SetActive(true);

            if (guideInfo.trans)
            {
                //20的偏差为手指动画的位移
                Vector2 dialogPos = targetTranCanvasPos;
                dialogPos.x = 0;
                if (dialogPos.y > 0)
                {
                    dialogPos += new Vector2(0,-200);
                    //上半屏幕在目标下方
                    dialogBgSprite.GetComponent<RectTransform>().localPosition = dialogPos;
                }
                else
                {
                    dialogPos += new Vector2(0,200);
                    //下半屏幕在目标上方
                    dialogBgSprite.GetComponent<RectTransform>().localPosition = dialogPos;
                }
            }
            
            // if (guideInfo.dialogDir != DialogDir.Mid)
            // {
            //     RectTransform.Edge edge = (RectTransform.Edge) guideInfo.dialogDir;
            //     int sizeX = 0;
            //     int sizeY = 0;
            //     switch (edge)
            //     {
            //         case RectTransform.Edge.Bottom:
            //             sizeY = -(Screen.height / 3);
            //             break;
            //         case RectTransform.Edge.Top:
            //             sizeY = Screen.height / 3;
            //             break;
            //         case RectTransform.Edge.Left:
            //             sizeX = -(Screen.width / 3);
            //             break;
            //         case RectTransform.Edge.Right:
            //             sizeX = Screen.width / 3;
            //             break;
            //     }
            //
            //     dialogBgSprite.GetComponent<RectTransform>().localPosition = new Vector2(sizeX,sizeY);
            //     //dialogBgSprite.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(edge, 0, size);
            // }
            // else
            // {
            //    dialogBgSprite.GetComponent<RectTransform>().localPosition = new Vector2(0,-200);
            // }
        }
        else
        {
            dialogBgSprite.gameObject.SetActive(false);
        }

        Sprite roleSprite = guideInfo.roleSprite;
        if (roleSprite != null)
        {
            leftRoleSprite.sprite = roleSprite;
            rightRoleSprite.sprite = roleSprite;
            leftRoleSprite.gameObject.SetActive(true);
            rightRoleSprite.gameObject.SetActive(true);
        }
        else
        {
            leftRoleSprite.gameObject.SetActive(false);
            rightRoleSprite.gameObject.SetActive(false);
        }

        //20的偏差为手指动画的位移
        Vector2 fingerPos = targetTranCanvasPos;
        if (fingerPos != Vector2.zero && guideInfo.haveFinger)
        {
            fingerObj.SetActive(true);
            if (fingerPos.y > 0)
            {
                // fingerPos += new Vector2(0, -80);
                //上半屏幕在目标下方
                fingerObj.transform.localScale = new Vector3(1,1,1);
                fingerObj.transform.localPosition = fingerPos;
            }
            else
            {
                //fingerPos += new Vector2(0, 100);
                //下半屏幕在目标上方
                //fingerObj.transform.localScale = new Vector3(1,-1,1);
                fingerObj.transform.localScale = new Vector3(1,1,1);
                fingerObj.transform.localPosition = fingerPos;
            }
            
            DOTween.Kill(fingerObj);
            DOTween.Sequence()
                .Append(fingerObj.transform.DOLocalMoveY(fingerPos.y - 20, 0.5f))
                .Append(fingerObj.transform.DOLocalMoveY(fingerPos.y, 0.5f))
                .SetTarget(fingerObj)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }
        else
        {
            fingerObj.SetActive(false);
        }

        if (guideInfo.showType == GuideShowType.Clone)
        {
            if (guideInfo.trans.gameObject != null)
            {
                curCloneGameObj = CloneGameObject(guideInfo);
                curCloneGameObj.SetActive(true);
            }

            maskContainer.gameObject.SetActive(false);
            clickArea.gameObject.SetActive(true);
            clickArea.sizeDelta = curCloneGameObj.GetComponent<RectTransform>().sizeDelta;
            clickArea.position = curCloneGameObj.transform.position;
        }
        else
        {
            clickArea.gameObject.SetActive(false);
            if (guideInfo.maskType == MaskType.Circle)
            {
                clickArea.gameObject.SetActive(guideInfo.haveClickArea);
                clickArea.sizeDelta = new Vector2(3000, 3000); //默认全屏
                clickArea.localPosition = Vector3.zero;
                
                guideCircleMask.SetTarget(guideInfo.trans);
                maskContainer.SetActive(true);
                guideCircleMask.gameObject.SetActive(true);
                guideRectMask.gameObject.SetActive(false);
            }
            else if (guideInfo.maskType == MaskType.Rect)
            {
                clickArea.gameObject.SetActive(guideInfo.haveClickArea);
                clickArea.sizeDelta = new Vector2(3000, 3000); //默认全屏
                clickArea.localPosition = Vector3.zero;
                
                guideRectMask.SetTarget(guideInfo.trans);
                maskContainer.SetActive(true);
                guideCircleMask.gameObject.SetActive(false);
                guideRectMask.gameObject.SetActive(true);
            }
            else
            {
                maskContainer.SetActive(false);
                clickArea.gameObject.SetActive(true);
                clickArea.sizeDelta = new Vector2(3000, 3000); //默认全屏
                clickArea.localPosition = Vector3.zero;
            }
        }

        if (IsInvoking(nameof(DelayCallEvent)))
        {
            CancelInvoke(nameof(DelayCallEvent));
        }

        Invoke(nameof(DelayCallEvent), 0.1f);
    }

    /// <summary>
    /// 克隆物体
    /// </summary>
    private GameObject CloneGameObject(GuideInfo info)
    {
        GameObject cloneObj = GameObject.Instantiate(info.trans.gameObject, cloneContainer, true);

        //如果克隆的物体是灰色的，改成正常颜色
        //ChangeToGrayMaterial[] changeToGrayMaterials = cloneObj.GetComponentsInChildren<ChangeToGrayMaterial>();
        //if(changeToGrayMaterials.Length!=0){
        //    foreach (ChangeToGrayMaterial gray in changeToGrayMaterials)
        //    {
        //        gray.SetGray(false);
        //    }
        //}

        cloneObj.transform.position = info.trans.position;
        cloneObj.transform.localScale = info.trans.localScale;
        cloneObj.transform.eulerAngles = info.trans.eulerAngles;
        //关闭原物体的事件
        Button[] buttons = cloneObj.GetComponentsInChildren<Button>();
        foreach (var btn in buttons)
        {
            Destroy(btn);
        }

        return cloneObj;
    }

    /// <summary>
    /// 开始事件
    /// </summary>
    private void DelayCallEvent()
    {
        // string strEvent = ConfigMgr.Instance.Tables.GuideSetCategory.GetOrDefault(curGuideId).StartEvent;
        // if (!string.IsNullOrEmpty(strEvent))
        // {
        //     GuideEventManager.Instance.gameObject.SendMessage(strEvent);
        // }
    }

    /// <summary>
    /// 显示引导
    /// </summary>
    public void ShowGuideById(int id)
    {
        SetDataById(id);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 显示箭头
    /// </summary>
    public void SetArrowState(bool active)
    {
        arrow.SetActive(active);
    }

    /// <summary>
    /// 点击区域
    /// </summary>
    void ClickAreaOnClick()
    {
        ClickNext();
    }
    
    /// <summary>
    /// 点击Next
    /// </summary>
    public bool ClickNext()
    {
        Debug.Log("next");
        if (curGuideId == 0)
        {
            return false;
        }
        
        // var temp = ConfigMgr.Instance.Tables.GuideSetCategory.GetOrDefault(curGuideId);
        // string strClick = temp.ClickEvent;
        // int nextId = temp.NextID;
        // if (nextId != 0)
        // {
        //     if (!string.IsNullOrEmpty(strClick))
        //     {
        //         GuideEventManager.Instance.gameObject.SendMessage(strClick);
        //     }
        //     SetDataById(nextId);
        //     return true;
        // }
        // else
        // {
        //     if (isInLoading)
        //     {
        //         gameObject.SetActive(false);
        //     }
        //     else
        //     {
        //         gameObject.SetActive(false);
        //     }
        //
        //     if (!string.IsNullOrEmpty(strClick))
        //     {
        //         GuideEventManager.Instance.gameObject.SendMessage(strClick);
        //     }
        //     Debug.Log("下一步引导id为0，结束引导");
        // }
        return true;
    }

    /// <summary>
    /// 世界坐标到画布坐标的转换
    /// </summary>
    /// <param name="canvas">画布</param>
    /// <param name="world">世界坐标</param>
    /// <returns>转换后在画布的坐标</returns>
    private Vector2 WorldToCanvasPos(Canvas canvas, Vector3 world)
    {
        Vector2 position;
        Vector2 scenePos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, scenePos,
            canvas.worldCamera, out position);
        return position;
    }

    //穿透点击事件--是否拦截  true为拦截，false为不拦截
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (clickArea.gameObject.activeSelf) return true;
        if (guideInfo.trans == null) return false;
        
        if (clickArea.gameObject.activeSelf && guideInfo is {showType: GuideShowType.Mask}) return true;
        
        //bool result = !RectTransformUtility.RectangleContainsScreenPoint(guideInfo.trans, sp, UIWindowMgr.Instance.uiCamera);
        //return result;
        return false;
    }
}