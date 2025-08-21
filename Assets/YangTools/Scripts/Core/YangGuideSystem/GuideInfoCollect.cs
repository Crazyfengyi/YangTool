using System.Collections;
using GameMain;
using UnityEngine;
using UnityEngine.UI;

public class GuideInfoCollect : MonoBehaviour
{
    public int guideID;
    private GuideInfo guideInfo;
    
    [Header("新手教程数据记录结束后此物体active状态")]
    public bool setActive = true;
    void Start()
    {
        //未完成这个引导就添加
        // if (!LocalSaveMgr.Instance.DataCenter.GetSaveGuide().IsOverGuide(guideID)) 
        {
            SetGuideInfo();
            //自动添加点击进行下一步引导的监听
            Button btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(ClickNext);
            }
        }
    }
    /// <summary>
    /// 在初始化的时候将所有新手教程的信息储存起来
    /// </summary>
    public void SetGuideInfo()
    {
        if (guideID == 0)
        {
            Debug.LogError($"{gameObject.name}的guideId为0,请在面板上赋值，物体位于{transform.parent.name}");
            return;
        }
        else
        {
            guideInfo = new GuideInfo(guideID,transform.GetComponent<RectTransform>());
            if (guideInfo != null)
            {
                YangGuideManager.Instance.AddGuideInfoDict(guideID, guideInfo, this.transform); 
            }
        }

        //记录完移除此脚本
        //BoxCollider boxCollider = transform.GetComponent<BoxCollider>();
        //if (gameObject.name.CompareTo("GuideBox") == 0)
        //{
        //    Destroy(this.gameObject);
        //}
        //else
        //{
        //    gameObject.SetActive(setActive);
        //    Destroy(this);
        //}
    }

    private void ClickNext()
    {
        var temp = YangGuideManager.Instance.guideWindow.ClickNext();
        Debug.Log($"点击引导:{guideInfo.guideID}--{temp}");
        if (temp)
        {
            if (gameObject.name.Equals("GuideBox"))
            {
                //自行创建的引导区域
                Destroy(this.gameObject);
            }
            else
            {
                gameObject.SetActive(setActive);
                Destroy(this);
            }
        }
    }
}