using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoopList : MonoBehaviour
{
     //private EncodeLong poolID;
     //private GButton Back;//返回按钮
     //private GButton hatchEggBtn;//孵化按钮
     //private GButton commpoundBtn;//合成按钮
     //private GButton OpenOneBtn;//打开一个按钮
     //private GLoader OpenOneBtnIcon;//打开一个按钮货币图标
     //private GTextField OpenOneBtnCount;//打开一个按钮货币数量
     //private GButton OpenTenBtn;//打开10个按钮
     //private GLoader OpenTenBtnIcon;//打开10个按钮货币图标
     //private GTextField OpenTenBtnCount;//打开10个按钮货币数量

     //private GButton ticketBtn;//抽取券
     //private GButton masonryBtn;//砖石

     //private GLabel R_ReflectCom;//所有中间的
     //private GLoader poolImage;//大背景
     //private Transition arrowsAni;//动画
     //private GTextField BigTitle;//大标题
     //private GTextField title;//抽奖池标题
     //private GRichTextField minTitle;//小标题
     //private GTextField tipsText;//抽一次按钮头上的提示
     //private GTextField btnMinText;//按钮下的提示
     //private GTextField btnMinText2;//按钮下的提示2
     //private GLabel bottomAll;//下方UI

     public static int openOneDiamond = 0/*20;*/;
     public static int openTenDiamond = 0/*200*/;
     private long countDownTime;//倒计时
     /// <summary>
     /// 当前选择的Item
     /// </summary>
     public ScrollLoopItem currentScrollLoopItem;

      #region 循环拖拽相关
      //=======循环拖拽相关=======
      private bool inThisPanel;
      public List<ScrollLoopItem> itemList = new List<ScrollLoopItem>();//循环滑动列表
      List<Vector2> origePosList = new List<Vector2>();//列表物体初始坐标
      private float leftRightScale = 0.82f; //两边物体的缩放
      public bool isMove = false;//是否移动
      private bool isBegin = false;//是在列表上按下
      private Vector2 monseStartPosition = Vector2.zero; //鼠标按键初始坐标
      private float itemWidth = 0;//滑动物体的宽
      public int currentPosIndex = 0;//父物体当前应到位置下标记录--默认0向右移动+1向左-1--每次只移动一个item块
      private float xLeft; //左边临界点
      private float xRight;//右边临界点
      private Action moveEnd;//拖动结束
      private float moveTime;//拖拽移动时间
      public bool canClick = true;//是否可以点击
      private RectTransform parentNode;//父节点
      private Vector2 parentNodeStartPos;//父节点起始点
      private Vector2 parentStartPositon;//父节点鼠标点下时的点
      Vector2 origePos = Vector2.zero;  //原始位置

      private Vector2 startPos;//开始局部位置
      private Vector2 startWorldPos;//开始世界位置
      private float intervalWorldX;//物体世界宽度(间隔)


      private ScrollLoopItem bestLeftItem;//最左边的item
      private ScrollLoopItem bestRightItem;//最右边的item
      #endregion

      public List<int> allPoolDataList = new List<int>();//所有池子数据

      public void Start()
      {
          #region 循环拖拽相关
          parentNode = transform as RectTransform;
          parentNodeStartPos = parentNode.position;
          for (int i = 0; i < 4; i++)
          {
              RectTransform item = parentNode.GetChild(i) as RectTransform;
              ScrollLoopItem script = new ScrollLoopItem(item);
              script.itemIndex = i;
              itemList.Add(script);
          }

          for (int i = 0; i < itemList.Count; i++)
          {
              var item = itemList[i];
              if (i == 0)//数据下标
              {
                  item.dataIndex = -1;
              }
              else//数据下标
              {
                  item.dataIndex = i - 1;
              }
          }

          bestLeftItem = itemList[0];
          bestRightItem = itemList[itemList.Count - 1];
          //设置位置枚举
          itemList[0].loopItemPosType = LoopItemPosType.Left;
          itemList[1].loopItemPosType = LoopItemPosType.Middle;
          itemList[2].loopItemPosType = LoopItemPosType.Right;
          itemList[3].loopItemPosType = LoopItemPosType.RightReserve;
          //更新数据
          for (int i = 0; i < itemList.Count; i++)
          {
              itemList[i].UpdateData();
          }

          startPos = itemList[1].component.position;
          startWorldPos = SelfLocalToWorld(itemList[1].component);

          itemWidth = itemList[0].component.sizeDelta.x;
          //初始位置--以中心的为起点(备用item是第0个)
          for (int i = 0; i < itemList.Count; i++)
          {
              if (i == 0) //备用item
              {
                  itemList[i].component.position = startPos - new Vector2(itemWidth, 0);
              }
              else
              {
                  itemList[i].component.position = startPos + new Vector2(itemWidth * (i - 1), 0);
              }
              origePosList.Add(itemList[i].component.position);
              //Debuger.ToError("物体的初始局部坐标:" + itemList[i].component.position);
          }

          intervalWorldX = Mathf.Abs(SelfLocalToWorld(itemList[2].component).x - startWorldPos.x);

          var tempPos = itemList[0].component.position;
          xLeft = new Vector2(tempPos.x - itemWidth, tempPos.y).x;  //左边临界点--世界坐标
          xRight = SelfLocalToWorld(itemList[3].component).x; //右边临界点--世界坐标
          origePos = parentNode.position;
          PositionCorrection();
          #endregion
      }
      
     public void OnEnable()
     {
         inThisPanel = true;
         Init(null);
     }
     public void OnDisEnable()
     {
         inThisPanel = false;
     }
     /// <summary>
     /// 初始化
     /// </summary>
     public void Init(Action _moveEnd, int dex = 0)
     {
       currentPosIndex = dex;
       moveEnd = _moveEnd;
       moveEnd += () =>
       {
           ScrollLoopItem temp = GetItem(LoopItemPosType.Middle);
           currentScrollLoopItem = temp;
           int data = GetDataFromDataIndex(temp.dataIndex);
           UpdateBtnState();
       };

       Restore();
       moveEnd?.Invoke();
       PositionCorrection();
       for (int i = 0; i < itemList.Count; i++)
       {
           itemList[i].UpdateData();
       }
   }
       /// <summary>
       /// 还原
       /// </summary>
       public void Restore()
       {
           currentPosIndex = 0;
           parentNode.position = parentNodeStartPos;
           for (int i = 0; i < itemList.Count; i++)
           {
               itemList[i].component.position = origePosList[i];
           }
           //设置位置枚举
           itemList[0].loopItemPosType = LoopItemPosType.Left;
           itemList[1].loopItemPosType = LoopItemPosType.Middle;
           itemList[2].loopItemPosType = LoopItemPosType.Right;
           itemList[3].loopItemPosType = LoopItemPosType.RightReserve;

           //更新数据
           for (int i = 0; i < itemList.Count; i++)
           {
               var item = itemList[i];

            if (i == 0)//数据下标
            {
                item.dataIndex = -1;
            }
            else//数据下标
            {
                item.dataIndex = i - 1;
            }
            itemList[i].UpdateData();
        }
    }
    /// <summary>
    /// 更新按钮状态
    /// </summary>
    public void UpdateBtnState()
    {
        //ScrollLoopItem temp = GetItem(LoopItemPosType.Middle);
        //GoCha data = GetDataFromDataIndex(temp.dataIndex);
        //if (data.TodayTimes >= data.NumLimit)
        //{
        //    //上限变灰
        //    OpenOneBtn.ToGrayed();
        //    OpenTenBtn.ToGrayed();
        //}
        //else
        //{
        //    OpenOneBtn.ReGrayed();
       //    OpenTenBtn.ReGrayed();
       //}
   }

   void Update()
   {
       //if (btnMinText != null)
       //{
       //    countDownTime -= (long)Time.unscaledDeltaTime;
       //    countDownTime = Math.Max(0, countDownTime);//毫秒
       //    long allSecends = countDownTime / 1000;//秒
       //    long hour = allSecends / 60 / 60;
       //    long minute = allSecends / 60 % 60;
       //    long second = allSecends % 60;
       //    btnMinText2.text = $"次数刷新倒计时{hour.ToString("D2")}:{minute.ToString("D2")}:{second.ToString("D2")}";
       //}

       //TODO:未实现
       //if (Input.GetKeyDown(KeyCode.Z))
       //{
       //    Move(1);
       //}
       //if (Input.GetKeyDown(KeyCode.X))
       //{
       //    Move(-10);
       //}
       //if (Input.GetKeyDown(KeyCode.C))
       //{
       //    Move(100);
       //}
       if (!inThisPanel) return;
       if (Input.GetMouseButtonDown(0))
       {
           //原点位置转换
           Vector3 mousPos = Input.mousePosition;
           mousPos.y = Screen.height - mousPos.y;
           Vector2 inputPos = mousPos; //R_ReflectCom.GlobalToLocal(mousPos)
           Vector2 parentNodesize = new Vector2(parentNode.sizeDelta.x, parentNode.sizeDelta.y);
           //按下 
           if (inputPos.x > origePos.x - parentNodesize.x / 2 &&
               inputPos.x < origePos.x + parentNodesize.x / 2 &&
               inputPos.y < origePos.y + parentNodesize.y / 2 &&
               inputPos.y > origePos.y - parentNodesize.y / 2)
           {
               //范围以内
               OnPointerDown();
           }
       }
       if (Input.GetMouseButton(0))
       {
           OnPointerStay();
       }
       if (Input.GetMouseButtonUp(0))
       {
           //抬起
           OnPointerUp();
       }
   }
   //点下
   void OnPointerDown()
   {
       if (!canClick) return;
       isBegin = true;
       isMove = false;
       monseStartPosition = Input.mousePosition;
       parentStartPositon = parentNode.position;
       moveTime = 0;
   }
   //持续
   void OnPointerStay()
   {
       if (isBegin == false) return;

       float movePosX = Input.mousePosition.x - monseStartPosition.x;
       parentNode.position = parentStartPositon + new Vector2(movePosX, 0);
       PositionCorrection();
       moveTime += Time.deltaTime;
       if (Math.Abs(movePosX) > 0)
       {
           isMove = true;
       }
       else
       {
           isMove = false;
       }
   }
   //抬起
   void OnPointerUp()
   {
       if (isBegin == false) return;
       isBegin = false;

       bool? isRight = null;//是向右移动

       //每次拖拽只能移动一个宽度位置
       float _x = monseStartPosition.x - Input.mousePosition.x; //x的距离，如果是负数则右移动，正数左移动
       if (_x < -200)
       {
           //位置应当-向右
           currentPosIndex++;
           isRight = true;
       }
       else if (_x > 200)
       {
           //位置应当--向左
           currentPosIndex--;
           isRight = false;
       }
       else if (_x < -20 && moveTime < 0.3f)
       {
           //位置应当--向右
           currentPosIndex++;
           isRight = true;
       }
       else if (_x > 20 && moveTime < 0.3f)
       {
           //位置应当--向左
           currentPosIndex--;
           isRight = false;
       }

       if (isRight != null)
       {
           SetPosType((bool)isRight);
       }

       if (Mathf.Abs(_x) > 0)
       {
           Move(currentPosIndex);
       }
   }
   /// <summary>
   /// 移动方法
   /// </summary>
   public void Move(int _index)
   {
       Vector2 nowPosition = parentNode.position;
       float endPos = itemWidth * (_index + 1);
       canClick = false;
       DOTween.To(() => nowPosition.x, value =>
       {
           parentNode.position = new Vector2(value, parentNode.position.y);
           PositionCorrection();
       }, endPos, 0.2f)
       .OnComplete(() =>
       {
           PositionCorrection();
           canClick = true;
       });

       moveEnd?.Invoke();
       moveTime = 0;
   }
   public void LeftMoveOne()
   {
       if (isMove) return;

       currentPosIndex--;
       SetPosType(false);
       Move(currentPosIndex);
   }
   public void RightMoveOne()
   {
       if (isMove) return;

       currentPosIndex++;
       SetPosType(true);
       Move(currentPosIndex);
   }
   /// <summary>
   /// 位置矫正
   /// </summary>
   void PositionCorrection()
   {
       for (int i = 0; i < itemList.Count; i++)
       {
           ScrollLoopItem item = itemList[i];
           if (SelfLocalToWorld(item.component).x > xRight)
           {
               //大于右边界
               item.component.position = new Vector2(item.component.position.x - itemWidth * itemList.Count, item.component.position.y);
               //旧的最左边的数据下标-1
               item.dataIndex = bestLeftItem.dataIndex - 1;
               //自己设置为最左边
               bestLeftItem = item;
               bestLeftItem.UpdateData();

               var rightIndex = item.itemIndex - 1;
               if (rightIndex < 0)
               {
                   rightIndex = itemList.Count - 1;
               }
               bestRightItem = itemList[rightIndex];
           }
           if (SelfLocalToWorld(item.component).x <= xLeft)
           {
               //小于左边界
               item.component.position = new Vector2(item.component.position.x + itemWidth * itemList.Count, item.component.position.y);
               //上一个最右边的数据下标+1
               item.dataIndex = bestRightItem.dataIndex + 1;
               var leftIndex = item.itemIndex + 1;
               if (leftIndex > itemList.Count - 1)
               {
                   leftIndex = 0;
               }
               bestLeftItem = itemList[leftIndex];
               //自己设置为最左边
               bestRightItem = item;
               bestRightItem.UpdateData();
           }

           float centerX = startWorldPos.x;
           float current = SelfLocalToWorld(item.component).x;
           float result = Mathf.Abs(centerX - current);
           result = Mathf.Clamp(result, 0, intervalWorldX);
           var ratio = result / intervalWorldX;

           var resultScale = 1 - (ratio * (1 - leftRightScale));
           item.component.localScale = new Vector2(resultScale, resultScale);
       }
   }

   /// <summary>
   /// 转世界坐标
   /// </summary>
   Vector3 SelfLocalToWorld(RectTransform gObject)
   {
       return gObject.position;
   }
   /// <summary>
   /// 根据数据下标获取数据
   /// </summary>
   /// <returns></returns>
   public int GetDataFromDataIndex(int dataIndex)
   {
       int dataCount = allPoolDataList.Count;
       int tempIndex = Mathf.Abs(dataIndex);
       if (dataIndex > 0)
       {
           int temp = (dataIndex % dataCount);
           return allPoolDataList[temp];
       }
       else
       {
           int temp = Mathf.CeilToInt((float)tempIndex / (float)dataCount);
           int tempCount = dataIndex + dataCount * temp;
           return allPoolDataList[tempCount];
       }
   }
   /// <summary>
   /// 设置位置枚举
   /// </summary>
   private void SetPosType(bool isRight)
   {
       if (isRight)
       {
           for (int i = 0; i < itemList.Count; i++)
           {
               ScrollLoopItem item = itemList[i];
               int temp = (((int)item.loopItemPosType + 1) % 4);//加1取余数
               item.loopItemPosType = (LoopItemPosType)temp;
           }
       }
       else
       {
           for (int i = 0; i < itemList.Count; i++)
           {
               ScrollLoopItem item = itemList[i];
               int temp = ((int)item.loopItemPosType - 1);//减1
               if (temp < 0)
               {
                   temp = 3; //left=>RightReserve
               }
               item.loopItemPosType = (LoopItemPosType)temp;
           }
       }
   }
    /// <summary>
    /// 根据位置枚举获得item--需要在抬起后调用--并且建议只取左中右
    /// </summary>
    /// <param name="_loopItemPosType">位置枚举</param>
    public ScrollLoopItem GetItem(LoopItemPosType _loopItemPosType)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            ScrollLoopItem item = itemList[i];
            if (item.loopItemPosType == _loopItemPosType)
            {
                return item;
            }
        }
        return null;
    }
}
/// <summary>
/// 循环列表物体
/// </summary>
public class ScrollLoopItem
{
    //public UIExtractEggs parent;
    public RectTransform component;
    public Image poolSelectBg;//背景
    public int itemIndex;//item自己的下标
    public int dataIndex;//数据下标
    public LoopItemPosType loopItemPosType;//在循环列表里的位置枚举
    public ScrollLoopItem(RectTransform com)
    {
        component = com;
        //parent = _parent;
        //component.BtnOnClick(ClickThisCard, false);
        //poolSelectBg = component.GetChild("poolSelectBg").asLoader;
    }
    /// <summary>
    /// 更新数据
    /// </summary>
    public void UpdateData()
    {
        //GoCha data = parent.GetDataFromDataIndex(dataIndex);
        //poolSelectBg.url = FUIUrl.GetCommonUrl(data.PoolSmallBgIcon);

        //component.GetChild("test").asTextField.text = $"{dataIndex}";
        //component.GetChild("id").asTextField.text = $"{data.Id}";
    }
    /// <summary>
    /// 点击卡片
    /// </summary>
    public void ClickThisCard()
    {
        //if (!parent.canClick) return;
        //switch (loopItemPosType)
        //{
        //    case LoopItemPosType.Left:
        //        parent.RightMoveOne();
        //        break;
        //    case LoopItemPosType.Right:
        //        parent.LeftMoveOne();
        //        break;
        //    default:
        //        break;
        //}
    }
}
public enum LoopItemPosType
{
    None = -1,
    Left = 0,
    Middle = 1,
    Right = 2,
    RightReserve = 3
}