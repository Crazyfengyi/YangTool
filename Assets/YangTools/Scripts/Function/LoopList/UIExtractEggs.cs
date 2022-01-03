//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Cinemachine;
//using System;
//using YangToolDebuger;
//using DG.Tweening;
//using UnityEngine.SceneManagement;

//public class LoopList : MonoBehaviour
//{
//    //private EncodeLong poolID;
//    //private GButton Back;//���ذ�ť
//    //private GButton hatchEggBtn;//������ť
//    //private GButton commpoundBtn;//�ϳɰ�ť
//    //private GButton OpenOneBtn;//��һ����ť
//    //private GLoader OpenOneBtnIcon;//��һ����ť����ͼ��
//    //private GTextField OpenOneBtnCount;//��һ����ť��������
//    //private GButton OpenTenBtn;//��10����ť
//    //private GLoader OpenTenBtnIcon;//��10����ť����ͼ��
//    //private GTextField OpenTenBtnCount;//��10����ť��������

//    //private GButton ticketBtn;//��ȡȯ
//    //private GButton masonryBtn;//שʯ

//    //private GLabel R_ReflectCom;//�����м��
//    //private GLoader poolImage;//�󱳾�
//    //private Transition arrowsAni;//����
//    //private GTextField BigTitle;//�����
//    //private GTextField title;//�齱�ر���
//    //private GRichTextField minTitle;//С����
//    //private GTextField tipsText;//��һ�ΰ�ťͷ�ϵ���ʾ
//    //private GTextField btnMinText;//��ť�µ���ʾ
//    //private GTextField btnMinText2;//��ť�µ���ʾ2
//    //private GLabel bottomAll;//�·�UI

//    public static int openOneDiamond = 0/*20;*/;
//    public static int openTenDiamond = 0/*200*/;
//    private long countDownTime;//����ʱ
//    /// <summary>
//    /// ��ǰѡ���Item
//    /// </summary>
//    public ScrollLoopItem currentScrollLoopItem;

//    #region ѭ����ק���
//    //=======ѭ����ק���=======
//    private bool inThisPanel;
//    public List<ScrollLoopItem> itemList = new List<ScrollLoopItem>();//ѭ�������б�
//    List<Vector2> origePosList = new List<Vector2>();//�б������ʼ����
//    private float leftRightScale = 0.82f; //�������������
//    public bool isMove = false;//�Ƿ��ƶ�
//    private bool isBegin = false;//�����б��ϰ���
//    private Vector2 monseStartPosition = Vector2.zero; //��갴����ʼ����
//    private float itemWidth = 0;//��������Ŀ�
//    public int currentPosIndex = 0;//�����嵱ǰӦ��λ���±��¼--Ĭ��0�����ƶ�+1����-1--ÿ��ֻ�ƶ�һ��item��
//    private float xLeft; //����ٽ��
//    private float xRight;//�ұ��ٽ��
//    private Action moveEnd;//�϶�����
//    private float moveTime;//��ק�ƶ�ʱ��
//    public bool canClick = true;//�Ƿ���Ե��
//    private GLabel parentNode;//���ڵ�
//    private Vector2 parentNodeStartPos;//���ڵ���ʼ��
//    private Vector2 parentStartPositon;//���ڵ�������ʱ�ĵ�
//    Vector2 origePos = Vector2.zero;  //ԭʼλ��

//    private Vector2 startPos;//��ʼ�ֲ�λ��
//    private Vector2 startWorldPos;//��ʼ����λ��
//    private float intervalWorldX;//����������(���)

//    private ScrollLoopItem bestLeftItem;//����ߵ�item
//    private ScrollLoopItem bestRightItem;//���ұߵ�item
//    #endregion

//    public List<int> allPoolDataList = new List<int>();//���г�������

//    public void Start()
//    {
//        #region ѭ����ק���
//        //parentNode = GetCom("R_ReflectCom").asLabel.GetChild("selectAll").asLabel;
//        parentNodeStartPos = parentNode.position;
//        for (int i = 0; i < 4; i++)
//        {
//            //GButton item = parentNode.GetChild($"poolSelect{i}").asButton;
//            ScrollLoopItem script = new ScrollLoopItem(item, this);
//            script.itemIndex = i;
//            itemList.Add(script);
//        }

//        for (int i = 0; i < itemList.Count; i++)
//        {
//            var item = itemList[i];
//            if (i == 0)//�����±�
//            {
//                item.dataIndex = -1;
//            }
//            else//�����±�
//            {
//                item.dataIndex = i - 1;
//            }
//        }

//        bestLeftItem = itemList[0];
//        bestRightItem = itemList[itemList.Count - 1];
//        //����λ��ö��
//        itemList[0].loopItemPosType = LoopItemPosType.Left;
//        itemList[1].loopItemPosType = LoopItemPosType.Middle;
//        itemList[2].loopItemPosType = LoopItemPosType.Right;
//        itemList[3].loopItemPosType = LoopItemPosType.RightReserve;
//        //��������
//        for (int i = 0; i < itemList.Count; i++)
//        {
//            itemList[i].UpdateData();
//        }

//        startPos = itemList[1].component.position;
//        startWorldPos = SelfLocalToWorld(itemList[1].component);

//        itemWidth = itemList[0].component.width;
//        //��ʼλ��--�����ĵ�Ϊ���(����item�ǵ�0��)
//        for (int i = 0; i < itemList.Count; i++)
//        {
//            if (i == 0) //����item
//            {
//                itemList[i].component.position = startPos - new Vector2(itemWidth, 0);
//            }
//            else
//            {
//                itemList[i].component.position = startPos + new Vector2(itemWidth * (i - 1), 0);
//            }
//            origePosList.Add(itemList[i].component.position);
//            //Debuger.ToError("����ĳ�ʼ�ֲ�����:" + itemList[i].component.position);
//        }

//        intervalWorldX = Mathf.Abs(SelfLocalToWorld(itemList[2].component).x - startWorldPos.x);

//        var tempPos = itemList[0].component.position;
//        xLeft = LocalToWorld(itemList[0].component, new Vector2(tempPos.x - itemWidth, tempPos.y)).x;  //����ٽ��--��������
//        xRight = SelfLocalToWorld(itemList[3].component).x; //�ұ��ٽ��--��������
//        origePos = parentNode.position;
//        PositionCorrection();
//        #endregion
//    }
//    public void OnEnable()
//    {
//        inThisPanel = true;
//        Init(null);
//    }
//    public void OnDisEnable()
//    {
//        inThisPanel = false;
//    }
//    /// <summary>
//    /// ��ʼ��
//    /// </summary>
//    public void Init(Action _moveEnd, int dex = 0)
//    {
//        currentPosIndex = dex;
//        moveEnd = _moveEnd;
//        moveEnd += () =>
//        {
//            ScrollLoopItem temp = GetItem(LoopItemPosType.Middle);
//            currentScrollLoopItem = temp;
//            int data = GetDataFromDataIndex(temp.dataIndex);
//            UpdateBtnState();
//        };

//        Restore();
//        moveEnd?.Invoke();
//        PositionCorrection();
//        for (int i = 0; i < itemList.Count; i++)
//        {
//            itemList[i].UpdateData();
//        }
//    }
//    /// <summary>
//    /// ��ԭ
//    /// </summary>
//    public void Restore()
//    {
//        currentPosIndex = 0;
//        parentNode.position = parentNodeStartPos;
//        for (int i = 0; i < itemList.Count; i++)
//        {
//            itemList[i].component.position = origePosList[i];
//        }
//        //����λ��ö��
//        itemList[0].loopItemPosType = LoopItemPosType.Left;
//        itemList[1].loopItemPosType = LoopItemPosType.Middle;
//        itemList[2].loopItemPosType = LoopItemPosType.Right;
//        itemList[3].loopItemPosType = LoopItemPosType.RightReserve;

//        //��������
//        for (int i = 0; i < itemList.Count; i++)
//        {
//            var item = itemList[i];

//            if (i == 0)//�����±�
//            {
//                item.dataIndex = -1;
//            }
//            else//�����±�
//            {
//                item.dataIndex = i - 1;
//            }
//            itemList[i].UpdateData();
//        }
//    }
//    /// <summary>
//    /// ���°�ť״̬
//    /// </summary>
//    public void UpdateBtnState()
//    {
//        //ScrollLoopItem temp = GetItem(LoopItemPosType.Middle);
//        //GoCha data = GetDataFromDataIndex(temp.dataIndex);
//        //if (data.TodayTimes >= data.NumLimit)
//        //{
//        //    //���ޱ��
//        //    OpenOneBtn.ToGrayed();
//        //    OpenTenBtn.ToGrayed();
//        //}
//        //else
//        //{
//        //    OpenOneBtn.ReGrayed();
//        //    OpenTenBtn.ReGrayed();
//        //}
//    }



//    void Update()
//    {
//        //if (btnMinText != null)
//        //{
//        //    countDownTime -= (long)Time.unscaledDeltaTime;
//        //    countDownTime = Math.Max(0, countDownTime);//����
//        //    long allSecends = countDownTime / 1000;//��
//        //    long hour = allSecends / 60 / 60;
//        //    long minute = allSecends / 60 % 60;
//        //    long second = allSecends % 60;
//        //    btnMinText2.text = $"����ˢ�µ���ʱ{hour.ToString("D2")}:{minute.ToString("D2")}:{second.ToString("D2")}";
//        //}

//        //TODO:δʵ��
//        //if (Input.GetKeyDown(KeyCode.Z))
//        //{
//        //    Move(1);
//        //}
//        //if (Input.GetKeyDown(KeyCode.X))
//        //{
//        //    Move(-10);
//        //}
//        //if (Input.GetKeyDown(KeyCode.C))
//        //{
//        //    Move(100);
//        //}
//        if (!inThisPanel) return;
//        if (Input.GetMouseButtonDown(0))
//        {
//            //ԭ��λ��ת��
//            Vector3 mousPos = Input.mousePosition;
//            mousPos.y = Screen.height - mousPos.y;
//            Vector2 inputPos = R_ReflectCom.GlobalToLocal(mousPos);
//            Vector2 parentNodesize = new Vector2(parentNode.width, parentNode.height);
//            //���� 
//            if (inputPos.x > origePos.x - parentNodesize.x / 2 &&
//                inputPos.x < origePos.x + parentNodesize.x / 2 &&
//                inputPos.y < origePos.y + parentNodesize.y / 2 &&
//                inputPos.y > origePos.y - parentNodesize.y / 2)
//            {
//                //��Χ����
//                OnPointerDown();
//            }
//        }
//        if (Input.GetMouseButton(0))
//        {
//            OnPointerStay();
//        }
//        if (Input.GetMouseButtonUp(0))
//        {
//            //̧��
//            OnPointerUp();
//        }
//    }
//    //����
//    void OnPointerDown()
//    {
//        if (!canClick) return;
//        isBegin = true;
//        isMove = false;
//        monseStartPosition = Input.mousePosition;
//        parentStartPositon = parentNode.position;
//        moveTime = 0;
//    }
//    //����
//    void OnPointerStay()
//    {
//        if (isBegin == false) return;

//        float movePosX = Input.mousePosition.x - monseStartPosition.x;
//        parentNode.position = parentStartPositon + new Vector2(movePosX, 0);
//        PositionCorrection();
//        moveTime += Time.deltaTime;
//        if (Math.Abs(movePosX) > 0)
//        {
//            isMove = true;
//        }
//        else
//        {
//            isMove = false;
//        }
//    }
//    //̧��
//    void OnPointerUp()
//    {
//        if (isBegin == false) return;
//        isBegin = false;

//        bool? isRight = null;//�������ƶ�

//        //ÿ����קֻ���ƶ�һ�����λ��
//        float _x = monseStartPosition.x - Input.mousePosition.x; //x�ľ��룬����Ǹ��������ƶ����������ƶ�
//        if (_x < -200)
//        {
//            //λ��Ӧ��-����
//            currentPosIndex++;
//            isRight = true;
//        }
//        else if (_x > 200)
//        {
//            //λ��Ӧ��--����
//            currentPosIndex--;
//            isRight = false;
//        }
//        else if (_x < -20 && moveTime < 0.3f)
//        {
//            //λ��Ӧ��--����
//            currentPosIndex++;
//            isRight = true;
//        }
//        else if (_x > 20 && moveTime < 0.3f)
//        {
//            //λ��Ӧ��--����
//            currentPosIndex--;
//            isRight = false;
//        }

//        if (isRight != null)
//        {
//            SetPosType((bool)isRight);
//        }

//        if (Mathf.Abs(_x) > 0)
//        {
//            Move(currentPosIndex);
//        }
//    }
//    /// <summary>
//    /// �ƶ�����
//    /// </summary>
//    public void Move(int _index)
//    {
//        Vector2 nowPosition = parentNode.position;
//        float endPos = itemWidth * (_index + 1);
//        canClick = false;
//        DOTween.To(() => nowPosition.x, value =>
//        {
//            parentNode.position = new Vector2(value, parentNode.position.y);
//            PositionCorrection();
//        }, endPos, 0.2f)
//        .OnComplete(() =>
//        {
//            PositionCorrection();
//            canClick = true;
//        });

//        moveEnd?.Invoke();
//        moveTime = 0;
//    }
//    public void LeftMoveOne()
//    {
//        if (isMove) return;

//        currentPosIndex--;
//        SetPosType(false);
//        Move(currentPosIndex);
//    }
//    public void RightMoveOne()
//    {
//        if (isMove) return;

//        currentPosIndex++;
//        SetPosType(true);
//        Move(currentPosIndex);
//    }
//    /// <summary>
//    /// λ�ý���
//    /// </summary>
//    void PositionCorrection()
//    {
//        for (int i = 0; i < itemList.Count; i++)
//        {
//            ScrollLoopItem item = itemList[i];
//            if (SelfLocalToWorld(item.component).x > xRight)
//            {
//                //�����ұ߽�
//                item.component.position = new Vector2(item.component.position.x - itemWidth * itemList.Count, item.component.position.y);
//                //�ɵ�����ߵ������±�-1
//                item.dataIndex = bestLeftItem.dataIndex - 1;
//                //�Լ�����Ϊ�����
//                bestLeftItem = item;
//                bestLeftItem.UpdateData();

//                var rightIndex = item.itemIndex - 1;
//                if (rightIndex < 0)
//                {
//                    rightIndex = itemList.Count - 1;
//                }
//                bestRightItem = itemList[rightIndex];
//            }
//            if (SelfLocalToWorld(item.component).x <= xLeft)
//            {
//                //С����߽�
//                item.component.position = new Vector2(item.component.position.x + itemWidth * itemList.Count, item.component.position.y);
//                //��һ�����ұߵ������±�+1
//                item.dataIndex = bestRightItem.dataIndex + 1;
//                var leftIndex = item.itemIndex + 1;
//                if (leftIndex > itemList.Count - 1)
//                {
//                    leftIndex = 0;
//                }
//                bestLeftItem = itemList[leftIndex];
//                //�Լ�����Ϊ�����
//                bestRightItem = item;
//                bestRightItem.UpdateData();
//            }

//            float centerX = startWorldPos.x;
//            float current = SelfLocalToWorld(item.component).x;
//            float result = Mathf.Abs(centerX - current);
//            result = Mathf.Clamp(result, 0, intervalWorldX);
//            var ratio = result / intervalWorldX;

//            var resultScale = 1 - (ratio * (1 - leftRightScale));
//            item.component.scale = new Vector2(resultScale, resultScale);
//        }
//    }
//    /// <summary>
//    /// ת��������
//    /// </summary>
//    Vector3 LocalToWorld(GObject gObject, Vector2 _pos)
//    {
//        //��Ҫ�Ƚ�UI�������ת��Ϊ��Ļ����
//        Vector2 screenPos = gObject.LocalToGlobal(_pos);
//        //ԭ��λ��ת��
//        screenPos.y = Screen.height - screenPos.y;
//        //һ������£�����Ҫ�ṩ�����������Ұ��ǰ��distance���ȵĲ�����ΪscreenPos.z(�����Ҫ����screenPos��ΪVector3���ͣ�
//        Vector3 worldPos = FUIFW.FUIMgr.Ins.UICamera.ScreenToWorldPoint(screenPos);
//        worldPos.z = 0;
//        return worldPos;
//    }
//    /// <summary>
//    /// ת��������
//    /// </summary>
//    Vector3 SelfLocalToWorld(GObject gObject)
//    {
//        //��Ҫ�Ƚ�UI�������ת��Ϊ��Ļ����
//        Vector2 screenPos = gObject.LocalToGlobal(Vector2.zero);
//        //ԭ��λ��ת��
//        screenPos.y = Screen.height - screenPos.y;
//        //һ������£�����Ҫ�ṩ�����������Ұ��ǰ��distance���ȵĲ�����ΪscreenPos.z(�����Ҫ����screenPos��ΪVector3���ͣ�
//        Vector3 worldPos = FUIFW.FUIMgr.Ins.UICamera.ScreenToWorldPoint(screenPos);
//        worldPos.z = 0;
//        return worldPos;
//    }
//    /// <summary>
//    /// ���������±��ȡ����
//    /// </summary>
//    /// <returns></returns>
//    public int GetDataFromDataIndex(int dataIndex)
//    {
//        int dataCount = allPoolDataList.Count;
//        int tempIndex = Mathf.Abs(dataIndex);
//        if (dataIndex > 0)
//        {
//            int temp = (dataIndex % dataCount);
//            return allPoolDataList[temp];
//        }
//        else
//        {
//            int temp = Mathf.CeilToInt((float)tempIndex / (float)dataCount);
//            int tempCount = dataIndex + dataCount * temp;
//            return allPoolDataList[tempCount];
//        }
//    }
//    /// <summary>
//    /// ����λ��ö��
//    /// </summary>
//    private void SetPosType(bool isRight)
//    {
//        if (isRight)
//        {
//            for (int i = 0; i < itemList.Count; i++)
//            {
//                ScrollLoopItem item = itemList[i];
//                int temp = (((int)item.loopItemPosType + 1) % 4);//��1ȡ����
//                item.loopItemPosType = (LoopItemPosType)temp;
//            }
//        }
//        else
//        {
//            for (int i = 0; i < itemList.Count; i++)
//            {
//                ScrollLoopItem item = itemList[i];
//                int temp = ((int)item.loopItemPosType - 1);//��1
//                if (temp < 0)
//                {
//                    temp = 3; //left=>RightReserve
//                }
//                item.loopItemPosType = (LoopItemPosType)temp;
//            }
//        }
//    }
//    /// <summary>
//    /// ����λ��ö�ٻ��item--��Ҫ��̧������--���ҽ���ֻȡ������
//    /// </summary>
//    /// <param name="_loopItemPosType">λ��ö��</param>
//    public ScrollLoopItem GetItem(LoopItemPosType _loopItemPosType)
//    {
//        for (int i = 0; i < itemList.Count; i++)
//        {
//            ScrollLoopItem item = itemList[i];
//            if (item.loopItemPosType == _loopItemPosType)
//            {
//                return item;
//            }
//        }
//        return null;
//    }
//}
///// <summary>
///// ѭ���б�����
///// </summary>
//public class ScrollLoopItem
//{
//    //public UIExtractEggs parent;
//    //public GComponent component;
//    //public GLoader poolSelectBg;//����
//    public int itemIndex;//item�Լ����±�
//    public int dataIndex;//�����±�
//    public LoopItemPosType loopItemPosType;//��ѭ���б����λ��ö��
//    public ScrollLoopItem(/*GComponent com, UIExtractEggs _parent*/)
//    {
//        //component = com;
//        //parent = _parent;
//        //component.BtnOnClick(ClickThisCard, false);
//        //poolSelectBg = component.GetChild("poolSelectBg").asLoader;
//    }
//    /// <summary>
//    /// ��������
//    /// </summary>
//    public void UpdateData()
//    {
//        //GoCha data = parent.GetDataFromDataIndex(dataIndex);
//        //poolSelectBg.url = FUIUrl.GetCommonUrl(data.PoolSmallBgIcon);

//        //component.GetChild("test").asTextField.text = $"{dataIndex}";
//        //component.GetChild("id").asTextField.text = $"{data.Id}";
//    }
//    /// <summary>
//    /// �����Ƭ
//    /// </summary>
//    public void ClickThisCard()
//    {
//        //if (!parent.canClick) return;
//        //switch (loopItemPosType)
//        //{
//        //    case LoopItemPosType.Left:
//        //        parent.RightMoveOne();
//        //        break;
//        //    case LoopItemPosType.Right:
//        //        parent.LeftMoveOne();
//        //        break;
//        //    default:
//        //        break;
//        //}
//    }
//}
//public enum LoopItemPosType
//{
//    None = -1,
//    Left = 0,
//    Middle = 1,
//    Right = 2,
//    RightReserve = 3
//}