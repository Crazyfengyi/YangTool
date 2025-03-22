/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-05-28 
*/
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YangTools;
using YangTools.Log;
using YangToolsManager = YangTools.Scripts.Core.YangToolsManager;

/// <summary>
/// 块的信息
/// </summary>
public class ChunkInfo
{
    /// <summary>
    /// 坐标
    /// </summary>
    public Vector2 pos = new Vector2(0, 0);
    /// <summary>
    /// 有效的
    /// </summary>
    public bool isValid;
    /// <summary>
    /// 房间号
    /// </summary>
    public int roomID;
    /// <summary>
    /// h单元格开始(左边的坐标)
    /// </summary>
    public int hStartTitleIndex = 0;
    /// <summary>
    /// v单元格开始(底边的坐标)
    /// </summary>
    public int vStartTitleIndex = 0;
    /// <summary>
    /// h单元格结束(右边的坐标)
    /// </summary>
    public int hEndTitleIndex
    {
        get
        {
            return hStartTitleIndex + hTitleCount - 1;
        }
    }
    /// <summary>
    /// v单元格结束(顶边的坐标)
    /// </summary>
    public int vEndTitleIndex
    {
        get
        {
            return vStartTitleIndex + vTitleCount - 1;
        }
    }
    //行向单元格数量
    public int hTitleCount = 0;
    //列向单元格数量
    public int vTitleCount = 0;
    /// <summary>
    /// 初始cost值
    /// </summary>
    public int InitCost
    {
        get
        {
            //取宽度、高度中较大的一个,然后将其除以2后向下取整,作为一个Cost值
            int cost = Mathf.FloorToInt(hTitleCount / 2f);
            return cost;
        }
    }
    /// <summary>
    /// 中心点坐标
    /// </summary>
    public Vector2 CenterPos
    {
        get
        {
            int hPos = Mathf.CeilToInt(hStartTitleIndex + hTitleCount / 2f);
            int vPos = Mathf.CeilToInt(vStartTitleIndex + vTitleCount / 2f);
            return new Vector2(hPos, vPos);
        }
    }
    /// <summary>
    /// 房间的所有块
    /// </summary>
    public List<TitleInfo> roomAllTitle = new List<TitleInfo>();
    /// <summary>
    /// 连接的房间ID
    /// </summary>
    public HashSet<int> connectRooms = new HashSet<int>();
    public ChunkInfo()
    {

    }

    /// <summary>
    /// 用算法生成房间
    /// </summary>
    public void Dijkstra()
    {
        //记录已经计算的
        HashSet<TitleInfo> record = new HashSet<TitleInfo>();
        Queue<TitleInfo> queue = new Queue<TitleInfo>();

        TitleInfo center = MazeCreator.instance.GetTitle((int)CenterPos.x, (int)CenterPos.y);
        center.cost = InitCost;
        record.Add(center);
        queue.Enqueue(center);

        System.Random random = new System.Random();
        while (queue.Count > 0)
        {
            TitleInfo target = queue.Dequeue();
            List<TitleInfo> list = GetAround(target);
            for (int i = 0; i < list.Count; i++)
            {
                TitleInfo item = list[i];
                bool isInChunk = TitleIsInChunk(item);
                if (!record.Contains(item) && isInChunk)
                {
                    item.cost = Mathf.Max(0, target.cost - random.Next(1, 3));
                    bool isChunkWall = TitleIsChunkWall(item);
                    if (isChunkWall) item.cost = 0;
                    record.Add(item);
                    if (item.cost != 0)
                    {
                        queue.Enqueue(item);
                    }
                }
            }
        }
        roomAllTitle.AddRange(record);

        TitleInfo tempCenter = MazeCreator.instance.GetTitle((int)CenterPos.x, (int)CenterPos.y);
        tempCenter.Show();
        tempCenter.obj.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        tempCenter.obj.GetComponentInChildren<TextMeshPro>(true).gameObject.SetActive(true);
        tempCenter.obj.GetComponentInChildren<TextMeshPro>(true).text = roomID.ToString();
    }

    public void Show()
    {
        for (int j = 0; j < roomAllTitle.Count; j++)
        {
            if (roomAllTitle[j].cost == 0)
            {
                roomAllTitle[j].Show();
            }
        }
        //for (int i = 0; i < hEndTitleIndex + 1; i++)
        //{
        //    for (int j = 0; j < vEndTitleIndex + 1; j++)
        //    {
        //        TitleInfo temp = MazeCreator.instance.GetTitle(i, j);

        //        if (temp != null)
        //        {
        //            if (temp.x == hStartTitleIndex || temp.x == hEndTitleIndex)
        //            {
        //                temp.Show();
        //            }
        //            if (temp.y == vStartTitleIndex || temp.y == vEndTitleIndex)
        //            {
        //                temp.Show();
        //            }
        //        }
        //    }
        //}
    }
    /// <summary>
    /// 获得环绕的块(四周)
    /// </summary>
    /// <returns></returns>
    public List<TitleInfo> GetAround(TitleInfo center)
    {
        List<TitleInfo> list = new List<TitleInfo>();
        int x = center.x;
        int y = center.y;
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                //去除中间块
                if (!(i == x && y == j))
                {
                    TitleInfo temp = MazeCreator.instance.GetTitle(i, j);
                    if (temp != null)//存在
                    {
                        list.Add(temp);
                    }
                }
            }
        }
        return list;
    }
    /// <summary>
    /// 单元块是否在块内
    /// </summary>
    public bool TitleIsInChunk(TitleInfo target)
    {
        if (target.x >= hStartTitleIndex && target.x <= hEndTitleIndex && target.y >= vStartTitleIndex && target.y <= vEndTitleIndex)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 单元格是否是块的墙
    /// </summary>
    public bool TitleIsChunkWall(TitleInfo target)
    {
        if (target.x == hStartTitleIndex || target.x == hEndTitleIndex || target.y == vStartTitleIndex || target.y == vEndTitleIndex)
        {
            return true;
        }
        return false;
    }
}
/// <summary>
/// 单元格
/// </summary>
public class TitleInfo
{
    public GameObject obj;
    public int cost = -1;
    public int x;
    public int y;
    public TitleInfo(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public void Show()
    {
        if (obj == null)
        {
            GameObject _obj = GameObject.Instantiate(MazeCreator.instance.titlePrefab);
            _obj.transform.position = new Vector3(x, y);
            obj = _obj;
        }
    }
}
public class MazeCreator : MonoBehaviour
{
    #region 变量
    public static MazeCreator instance;
    public int roomCount = 6;//房间数
    //房间大小限制
    public Vector2 maxRoomSize = new Vector2(100, 100);
    //最长连接单元格数
    public int connectMaxTileCount = 10;
    //最小经过房间数--起点到终点(建议是取房间数的一半)
    public int minNumPassRooms = 3;
    //块数组
    private ChunkInfo[,] chunkArray;
    public GameObject titlePrefab;
    public List<List<TitleInfo>> allTitle = new List<List<TitleInfo>>();
    #endregion

    #region 生命周期
    public void Awake()
    {
        instance = this;
        Debuger.IsForceLog = true;
        //所谓的“块”即每个随机房间出现的范围。为了确保地图尽可能接近正方形范围

        //行向的块数= “房间的个数”的平方根向上取整，
        int horizontalChunk = Mathf.CeilToInt(Mathf.Sqrt(roomCount));
        //列向的块数=（总房间数/行向块数）向上取整
        int verticalChunk = Mathf.CeilToInt((float)roomCount / (float)horizontalChunk);

        //一个“块”的单元格数
        //单个块行向单元格数=房间最大行向格数+行向连线最大tile数；
        int oneChunkDefaultHTitle = (int)maxRoomSize.x + connectMaxTileCount;
        //单个块列向单元格数=房间最大列向格数+列向连线最大tile数
        int oneChunkDefaultVTitle = (int)maxRoomSize.y + connectMaxTileCount;

        chunkArray = new ChunkInfo[horizontalChunk, verticalChunk];
        for (int i = 0; i < horizontalChunk; i++)
        {
            for (int j = 0; j < verticalChunk; j++)
            {
                ChunkInfo chunk = new ChunkInfo();
                chunk.isValid = true;
                chunk.pos = new Vector2(i, j);
                chunk.hTitleCount = oneChunkDefaultHTitle;
                chunk.vTitleCount = oneChunkDefaultVTitle;
                chunkArray[i, j] = chunk;
            }
        }

        int chunkCount = horizontalChunk * verticalChunk;
        //解决块数>房间数的问题
        //如果是等于,就没有什么问题,但是如果是大于,我们就首先要随机取出(总块数-房间数)行(或者列),
        //然后在这些行（或者列）中取出1个“块”删除掉
        int takeOutCount = chunkCount - roomCount;
        Debuger.ToError($"总块数:{chunkCount}房间数:{roomCount},takeOutCount:{takeOutCount}/n行:{horizontalChunk},列{verticalChunk}");
        //行数必定>=列数(horizontalChunk = Mathf.CeilToInt(Mathf.Sqrt(roomCount));)
        HashSet<int> hasRandom = new HashSet<int>();
        System.Random rand = new System.Random(DateTime.Now.Millisecond);
        while (takeOutCount > 0)
        {
            int index = rand.Next(0, horizontalChunk);
            if (!hasRandom.Contains(index))
            {
                hasRandom.Add(index);
                int num = rand.Next(0, verticalChunk);
                chunkArray[index, num].isValid = false;
                takeOutCount--;
            }
        }
        //确定了每一个“块”拥有的行向.列向单元格数量,这个最小单元格数应该不小于房间的最小宽度(高度)+行向(列向)最短连接单元格数。
        //根据行分单元格数
        int tempRoomID = 0;
        //将行向的无效块分给有效的块--并且给有效房间确定房间号
        for (int i = 0; i < chunkArray.GetLength(0); i++)
        {
            int validHChunkCount = ValidHChunkCount(i);//有效块的数量
            int invalidHChunkCount = chunkArray.GetLength(1) - validHChunkCount;//无效块的数量
            int[] arrayH;
            if (invalidHChunkCount > 0)
            {
                arrayH = YangToolsManager.SplitTheNumber(invalidHChunkCount * oneChunkDefaultHTitle, validHChunkCount, oneChunkDefaultHTitle / 2);
            }
            else
            {
                arrayH = null;
            }
            int index = 0;
            for (int j = 0; j < chunkArray.GetLength(1); j++)
            {
                if (chunkArray[i, j].isValid == true)
                {
                    if (arrayH != null && index < arrayH.Length)
                    {
                        chunkArray[i, j].hTitleCount += arrayH[index];
                    }
                    chunkArray[i, j].roomID = tempRoomID;
                    index++;
                    tempRoomID++;
                }
            }
        }
        //计算单元格的开始位置
        int vStartTitle = 0;//每一行计数
        for (int i = 0; i < chunkArray.GetLength(0); i++)
        {
            int hStartTitle = 0;//每一列单元格分配计数
            for (int j = 0; j < chunkArray.GetLength(1); j++)
            {
                if (chunkArray[i, j].isValid)
                {
                    chunkArray[i, j].vStartTitleIndex = vStartTitle * oneChunkDefaultVTitle;
                    chunkArray[i, j].hStartTitleIndex = hStartTitle;
                    hStartTitle += chunkArray[i, j].hTitleCount;
                }
            }
            vStartTitle++;
        }
        //“块”与“块”之间的连接图
        //只处理右边和上边的连接
        for (int i = 0; i < chunkArray.GetLength(0); i++)
        {
            for (int j = 0; j < chunkArray.GetLength(1); j++)
            {
                //有效的
                if (chunkArray[i, j].isValid)
                {
                    //当前块
                    ChunkInfo thisChunk = chunkArray[i, j];
                    //左右边
                    int leftNum = thisChunk.hStartTitleIndex;
                    int rightNum = thisChunk.hStartTitleIndex + thisChunk.hTitleCount;

                    //右边第一个有效的块
                    for (int k = j; k < chunkArray.GetLength(1); k++)
                    {
                        if (chunkArray[i, k].isValid)
                        {
                            thisChunk.connectRooms.Add(chunkArray[i, k].roomID);
                            break;
                        }
                    }
                    //上边一排
                    if (i + 1 < chunkArray.GetLength(0))
                    {
                        for (int t = 0; t < chunkArray.GetLength(1); t++)
                        {
                            //有效块
                            if (chunkArray[i, t].isValid)
                            {
                                ChunkInfo item = chunkArray[i, t];
                                //左右边
                                int tempLeftNum = item.hStartTitleIndex;
                                int tempRightNum = item.hStartTitleIndex + item.hTitleCount;
                                //有交集
                                if ((leftNum < tempLeftNum && tempLeftNum < rightNum) || (leftNum < tempRightNum && tempRightNum < rightNum))
                                {
                                    thisChunk.connectRooms.Add(item.roomID);
                                }
                            }
                        }
                    }
                }
            }
        }
        //遍历互相告知连接关系
        for (int i = 0; i < chunkArray.GetLength(0); i++)
        {
            for (int j = 0; j < chunkArray.GetLength(1); j++)
            {
                ChunkInfo item = chunkArray[i, j];
                if (item.isValid)
                {
                    foreach (int roomId in item.connectRooms)
                    {
                        ChunkInfo room = GetRoomForID(roomId);
                        if (room != null)
                        {
                            room.connectRooms.Add(item.roomID);
                        }
                    }
                }
            }
        }

        //生成单元网格
        //总的行列单元格数量
        int allHTitleCount = oneChunkDefaultHTitle * verticalChunk;
        int allVTitleCount = oneChunkDefaultVTitle * horizontalChunk;
        for (int i = 0; i < allHTitleCount; i++)
        {
            allTitle.Add(new List<TitleInfo>());
            for (int j = 0; j < allVTitleCount; j++)
            {
                TitleInfo temp = new TitleInfo(i, j);
                allTitle[i].Add(temp);
            }
        }
        //用Dijkstra生成“房间”
        for (int i = 0; i < chunkArray.GetLength(0); i++)
        {
            for (int j = 0; j < chunkArray.GetLength(1); j++)
            {
                if (chunkArray[i, j].isValid)
                {
                    chunkArray[i, j].Dijkstra();
                }
            }
        }

        for (int i = 0; i < 20; i++)
        {
            GetRoomForID(i)?.Show();
        }
    }
    #endregion

    #region 方法
    /// <summary>
    /// 有效行块数
    /// </summary>
    /// <returns></returns>
    public int ValidHChunkCount(int index)
    {
        int result = 0;
        for (int i = 0; i < chunkArray.GetLength(1); i++)
        {
            if (chunkArray[index, i].isValid == true)
            {
                result++;
            }
        }
        return result;
    }
    /// <summary>
    /// 根据房间ID获得房间--可以存字典优化
    /// </summary>
    /// <param name="roomID">房间ID</param>
    /// <returns></returns>
    public ChunkInfo GetRoomForID(int roomID)
    {
        for (int i = 0; i < chunkArray.GetLength(0); i++)
        {
            for (int j = 0; j < chunkArray.GetLength(1); j++)
            {
                ChunkInfo item = chunkArray[i, j];
                if (item.roomID == roomID && item.isValid)
                {
                    return item;
                }
            }
        }
        return null;
    }
    /// <summary>
    /// 获得单元格
    /// </summary>
    public TitleInfo GetTitle(int x, int y)
    {
        if (x < 0 || x > allTitle.Count - 1)
        {
            return null;
        }
        if (y < 0 || y > allTitle[1].Count - 1)
        {
            return null;
        }

        return allTitle[x][y];
    }
    #endregion
}