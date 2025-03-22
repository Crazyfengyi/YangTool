/* 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-11 
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core;

/// <summary>
/// 程序生成无限世界的脚本
/// </summary>
public class EndlessWord : MonoBehaviour
{
    #region 变量
    /// <summary>
    /// 移动的目标
    /// </summary>
    public Transform target;
    /// <summary>
    /// 预制体信息数组
    /// </summary>
    public EndlessPrefab[] prefabs;
    /// <summary>
    /// 生成地图块的范围
    /// </summary>
    public int range = 1;
    /// <summary>
    /// 强制刷新范围(禁用异步加载范围内)
    /// </summary>
    public int disableAsyncLoadWithinRange = 1;
    /// <summary>
    /// 地图块大小
    /// </summary>
    public float tileSize = 100;
    /// <summary>
    /// 地图块内部分小区域的大小
    /// </summary>
    public int subTiles = 20;
    /// <summary>
    /// 生成的地图块是否启用静态批处理。
    /// 将提高整体的FPS，但当静态批处理完成时,可能会导致某些帧的FPS下降
    /// </summary>
    public bool staticBatching = false;
    /// <summary>
    /// 地图块生成队列
    /// </summary>
    Queue<IEnumerator> tileGenerationQueue = new Queue<IEnumerator>();
    /// <summary>
    /// 所有地图块
    /// </summary>
    Dictionary<Int2, EndlessTile> tiles = new Dictionary<Int2, EndlessTile>();
    #endregion

    #region 生命周期
    private void Start()
    {
        //计算最近的地图，然后重新计算图
        Update();
        AstarPath.active.Scan();
        StartCoroutine(GenerateTiles());
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            YangToolsManager.SetCursorLock(true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            YangToolsManager.SetCursorLock(false);
        }

        //计算目标所在的地图块
        Int2 p = new Int2(Mathf.RoundToInt((target.position.x - tileSize * 0.5f) / tileSize),
            Mathf.RoundToInt((target.position.z - tileSize * 0.5f) / tileSize));
        //限制最小范围
        range = range < 1 ? 1 : range;

        //移除超出范围的地图块
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (KeyValuePair<Int2, EndlessTile> pair in tiles)
            {
                if (Mathf.Abs(pair.Key.x - p.x) > range || Mathf.Abs(pair.Key.y - p.y) > range)
                {
                    pair.Value.Destroy();
                    tiles.Remove(pair.Key);
                    changed = true;
                    break;
                }
            }
        }

        //添加已经进入范围的地图块并开始计算它们
        for (int x = p.x - range; x <= p.x + range; x++)
        {
            for (int z = p.y - range; z <= p.y + range; z++)
            {
                if (!tiles.ContainsKey(new Int2(x, z)))
                {
                    EndlessTile tile = new EndlessTile(this, x, z);
                    IEnumerator generator = tile.Generate();
                    //生成主体
                    generator.MoveNext();
                    //稍后计算内部物体
                    tileGenerationQueue.Enqueue(generator);
                    tiles.Add(new Int2(x, z), tile);
                }
            }
        }

        //确保直接与当前的一个相邻的地图块应该总是完全计算，
        for (int x = p.x - disableAsyncLoadWithinRange; x <= p.x + disableAsyncLoadWithinRange; x++)
        {
            for (int z = p.y - disableAsyncLoadWithinRange; z <= p.y + disableAsyncLoadWithinRange; z++)
            {
                tiles[new Int2(x, z)].ForceFinish();
            }
        }
    }
    /// <summary>
    /// 生成地图块
    /// </summary>
    IEnumerator GenerateTiles()
    {
        while (true)
        {
            if (tileGenerationQueue.Count > 0)
            {
                //出列
                IEnumerator generator = tileGenerationQueue.Dequeue();
                yield return StartCoroutine(generator);
            }
            yield return null;
        }
    }
    #endregion

    #region 方法
    #endregion
}
/// <summary>
/// 无尽地图块
/// </summary>
class EndlessTile
{
    //管理类(无尽世界)
    EndlessWord world;
    //坐标
    int x, z;
    //随机函数(根据x,z固定随机种子)
    System.Random random;
    /// <summary>
    /// 是否已经删除
    /// </summary>
    public bool destroyed { get; private set; }
    public EndlessTile(EndlessWord world, int x, int z)
    {
        this.x = x;
        this.z = z;
        this.world = world;
        random = new System.Random((x * 10007) ^ (z * 36007));
    }
    //地图块实体父节点
    Transform root;
    //内部物体生成携程
    IEnumerator ie;
    /// <summary>
    /// 生成地图块
    /// </summary>
    public IEnumerator Generate()
    {
        ie = InternalGenerate();
        GameObject rt = new GameObject($"Tile:({x},{z})");
        root = rt.transform;
        //手动调用生成内部物体
        while (ie != null && root != null && ie.MoveNext())
        {
            yield return ie.Current;
        }
        ie = null;
    }
    /// <summary>
    /// 强制完成内部物体生成
    /// </summary>
    public void ForceFinish()
    {
        while (ie != null && root != null && ie.MoveNext())
        {
        }
        ie = null;
    }
    //获取随机数
    float RandomNumber(int minValue, int maxValue)
    {
        return (float)random.Next(minValue, maxValue);
    }
    //内部随机
    Vector3 RandomInside(float px, float pz)
    {
        Vector3 v = new Vector3();
        v.x = (px + (float)random.NextDouble() / world.subTiles) * world.tileSize;
        v.z = (pz + (float)random.NextDouble() / world.subTiles) * world.tileSize;
        return v;
    }
    /// <summary>
    /// 随机物体旋转
    /// </summary>
    Quaternion RandomYRot(EndlessPrefab prefab)
    {
        return prefab.randomRotation == RotationRandomness.AllAxes ? Quaternion.Euler(360 * (float)random.NextDouble(), 360 * (float)random.NextDouble(), 360 * (float)random.NextDouble()) : Quaternion.Euler(0, 360 * (float)random.NextDouble(), 0);
    }
    /// <summary>
    /// 地图块内部物体生成
    /// </summary>
    IEnumerator InternalGenerate()
    {
        Debug.Log("Generating tile " + x + ", " + z);
        int counter = 0; //计数器
        //噪声图
        float[,] ditherMap = new float[world.subTiles + 2, world.subTiles + 2];

        //List<GameObject> objs = new List<GameObject>();
        for (int i = 0; i < world.prefabs.Length; i++)
        {
            EndlessPrefab prefabInfo = world.prefabs[i];

            if (prefabInfo.singleFixed)
            {
                Vector3 p = new Vector3((x + 0.5f) * world.tileSize, 0, (z + 0.5f) * world.tileSize);
                GameObject ob = GameObject.Instantiate(prefabInfo.prefab, p, Quaternion.identity) as GameObject;
                ob.transform.parent = root;
                ob.SetActive(true);
            }
            else
            {
                //小尺寸
                float subSize = world.tileSize / world.subTiles;

                for (int sx = 0; sx < world.subTiles; sx++)
                {
                    for (int sz = 0; sz < world.subTiles; sz++)
                    {
                        ditherMap[sx + 1, sz + 1] = 0;
                    }
                }

                for (int sx = 0; sx < world.subTiles; sx++)
                {
                    for (int sz = 0; sz < world.subTiles; sz++)
                    {
                        float px = x + sx / (float)world.subTiles;//sx / world.tileSize;
                        float pz = z + sz / (float)world.subTiles;//sz / world.tileSize;

                        //生成2D柏林噪声
                        float perl = Mathf.Pow(Mathf.PerlinNoise((px + prefabInfo.perlinOffset.x) * prefabInfo.perlinScale, (pz + prefabInfo.perlinOffset.y) * prefabInfo.perlinScale), prefabInfo.perlinPower);
                        //密集度
                        float density = prefabInfo.density * Mathf.Lerp(1, perl, prefabInfo.perlin) * Mathf.Lerp(1, (float)random.NextDouble(), prefabInfo.random);
                        //取字段数
                        float fcount = subSize * subSize * density + ditherMap[sx + 1, sz + 1];
                        int count = Mathf.RoundToInt(fcount);
                        // 应用噪声图--添加抖动
                        // See http://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering
                        ditherMap[sx + 1 + 1, sz + 1 + 0] += (7f / 16f) * (fcount - count);
                        ditherMap[sx + 1 - 1, sz + 1 + 1] += (3f / 16f) * (fcount - count);
                        ditherMap[sx + 1 + 0, sz + 1 + 1] += (5f / 16f) * (fcount - count);
                        ditherMap[sx + 1 + 1, sz + 1 + 1] += (1f / 16f) * (fcount - count);
                        // 创建对象
                        for (int j = 0; j < count; j++)
                        {
                            //在当前小块中找到一个随机的位置
                            Vector3 pos = RandomInside(px, pz);
                            GameObject ob = GameObject.Instantiate(prefabInfo.prefab, pos, RandomYRot(prefabInfo)) as GameObject;
                            ob.transform.parent = root;
                            ob.SetActive(true);
                            float randomNum = RandomNumber(5, 20);
                            randomNum = randomNum / 10f;//缩放在[0.5,2)之间
                            ob.transform.localScale = new Vector3(randomNum, randomNum, randomNum);
                            //objs.Add(ob);
                            counter++;
                            //每帧生成一个,防止卡顿
                            if (counter % 2 == 0)
                            {
                                yield return null;
                            }
                        }
                    }
                }
            }
        }

        ditherMap = null;
        yield return null;
        yield return null;

        //批量处理一切以提高性能--Unity是用Pro授权激活的吗?
        if (Application.HasProLicense() && world.staticBatching)
        {
            StaticBatchingUtility.Combine(root.gameObject);
        }
    }
    public void Destroy()
    {
        if (root != null)
        {
            Debug.Log("Destroying tile " + x + ", " + z);
            GameObject.Destroy(root.gameObject);
            root = null;
        }
        // 确保瓦片生成器协程被破坏
        ie = null;
    }
}
/// <summary>
/// 二维整数坐标
/// </summary>
public struct Int2 : System.IEquatable<Int2>
{
    public int x;
    public int y;

    public Int2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public long sqrMagnitudeLong
    {
        get
        {
            return (long)x * (long)x + (long)y * (long)y;
        }
    }
    public static Int2 operator +(Int2 a, Int2 b)
    {
        return new Int2(a.x + b.x, a.y + b.y);
    }

    public static Int2 operator -(Int2 a, Int2 b)
    {
        return new Int2(a.x - b.x, a.y - b.y);
    }

    public static bool operator ==(Int2 a, Int2 b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Int2 a, Int2 b)
    {
        return a.x != b.x || a.y != b.y;
    }

    /// <summary>Dot product of the two coordinates</summary>
    public static long DotLong(Int2 a, Int2 b)
    {
        return (long)a.x * (long)b.x + (long)a.y * (long)b.y;
    }

    public override bool Equals(System.Object o)
    {
        if (o == null) return false;
        var rhs = (Int2)o;

        return x == rhs.x && y == rhs.y;
    }

    #region IEquatable implementation

    public bool Equals(Int2 other)
    {
        return x == other.x && y == other.y;
    }

    #endregion

    public override int GetHashCode()
    {
        return x * 49157 + y * 98317;
    }

    public static Int2 Min(Int2 a, Int2 b)
    {
        return new Int2(System.Math.Min(a.x, b.x), System.Math.Min(a.y, b.y));
    }

    public static Int2 Max(Int2 a, Int2 b)
    {
        return new Int2(System.Math.Max(a.x, b.x), System.Math.Max(a.y, b.y));
    }

    public static Int2 FromInt3XZ(Int3 o)
    {
        return new Int2(o.x, o.z);
    }

    public static Int3 ToInt3XZ(Int2 o)
    {
        return new Int3(o.x, 0, o.y);
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}
/// <summary>
/// 三维整数坐标
/// </summary>
public struct Int3 : System.IEquatable<Int3>
{
    public int x;
    public int y;
    public int z;

    //These should be set to the same value (only PrecisionFactor should be 1 divided by Precision)

    /// <summary>
    /// Precision for the integer coordinates.
    /// One world unit is divided into [value] pieces. A value of 1000 would mean millimeter precision, a value of 1 would mean meter precision (assuming 1 world unit = 1 meter).
    /// This value affects the maximum coordinates for nodes as well as how large the cost values are for moving between two nodes.
    /// A higher value means that you also have to set all penalty values to a higher value to compensate since the normal cost of moving will be higher.
    /// </summary>
    public const int Precision = 1000;

    /// <summary><see cref="Precision"/> as a float</summary>
    public const float FloatPrecision = 1000F;

    /// <summary>1 divided by <see cref="Precision"/></summary>
    public const float PrecisionFactor = 0.001F;

    public static Int3 zero { get { return new Int3(); } }

    public Int3(Vector3 position)
    {
        x = (int)System.Math.Round(position.x * FloatPrecision);
        y = (int)System.Math.Round(position.y * FloatPrecision);
        z = (int)System.Math.Round(position.z * FloatPrecision);
    }

    public Int3(int _x, int _y, int _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public static bool operator ==(Int3 lhs, Int3 rhs)
    {
        return lhs.x == rhs.x &&
               lhs.y == rhs.y &&
               lhs.z == rhs.z;
    }

    public static bool operator !=(Int3 lhs, Int3 rhs)
    {
        return lhs.x != rhs.x ||
               lhs.y != rhs.y ||
               lhs.z != rhs.z;
    }

    public static explicit operator Int3(Vector3 ob)
    {
        return new Int3(
            (int)System.Math.Round(ob.x * FloatPrecision),
            (int)System.Math.Round(ob.y * FloatPrecision),
            (int)System.Math.Round(ob.z * FloatPrecision)
            );
    }

    public static explicit operator Vector3(Int3 ob)
    {
        return new Vector3(ob.x * PrecisionFactor, ob.y * PrecisionFactor, ob.z * PrecisionFactor);
    }

    public static Int3 operator -(Int3 lhs, Int3 rhs)
    {
        lhs.x -= rhs.x;
        lhs.y -= rhs.y;
        lhs.z -= rhs.z;
        return lhs;
    }

    public static Int3 operator -(Int3 lhs)
    {
        lhs.x = -lhs.x;
        lhs.y = -lhs.y;
        lhs.z = -lhs.z;
        return lhs;
    }

    public static Int3 operator +(Int3 lhs, Int3 rhs)
    {
        lhs.x += rhs.x;
        lhs.y += rhs.y;
        lhs.z += rhs.z;
        return lhs;
    }

    public static Int3 operator *(Int3 lhs, int rhs)
    {
        lhs.x *= rhs;
        lhs.y *= rhs;
        lhs.z *= rhs;

        return lhs;
    }

    public static Int3 operator *(Int3 lhs, float rhs)
    {
        lhs.x = (int)System.Math.Round(lhs.x * rhs);
        lhs.y = (int)System.Math.Round(lhs.y * rhs);
        lhs.z = (int)System.Math.Round(lhs.z * rhs);

        return lhs;
    }

    public static Int3 operator *(Int3 lhs, double rhs)
    {
        lhs.x = (int)System.Math.Round(lhs.x * rhs);
        lhs.y = (int)System.Math.Round(lhs.y * rhs);
        lhs.z = (int)System.Math.Round(lhs.z * rhs);

        return lhs;
    }

    public static Int3 operator /(Int3 lhs, float rhs)
    {
        lhs.x = (int)System.Math.Round(lhs.x / rhs);
        lhs.y = (int)System.Math.Round(lhs.y / rhs);
        lhs.z = (int)System.Math.Round(lhs.z / rhs);
        return lhs;
    }
    public int this[int i]
    {
        get
        {
            return i == 0 ? x : (i == 1 ? y : z);
        }
        set
        {
            if (i == 0) x = value;
            else if (i == 1) y = value;
            else z = value;
        }
    }
    /// <summary>Angle between the vectors in radians</summary>
    public static float Angle(Int3 lhs, Int3 rhs)
    {
        double cos = Dot(lhs, rhs) / ((double)lhs.magnitude * (double)rhs.magnitude);

        cos = cos < -1 ? -1 : (cos > 1 ? 1 : cos);
        return (float)System.Math.Acos(cos);
    }

    public static int Dot(Int3 lhs, Int3 rhs)
    {
        return
            lhs.x * rhs.x +
            lhs.y * rhs.y +
            lhs.z * rhs.z;
    }
    public static long DotLong(Int3 lhs, Int3 rhs)
    {
        return
            (long)lhs.x * (long)rhs.x +
            (long)lhs.y * (long)rhs.y +
            (long)lhs.z * (long)rhs.z;
    }
    /// <summary>
    /// Normal in 2D space (XZ).
    /// Equivalent to Cross(this, Int3(0,1,0) )
    /// except that the Y coordinate is left unchanged with this operation.
    /// </summary>
    public Int3 Normal2D()
    {
        return new Int3(z, y, -x);
    }
    /// <summary>
    /// Returns the magnitude of the vector. The magnitude is the 'length' of the vector from 0,0,0 to this point. Can be used for distance calculations:
    /// <code> Debug.Log ("Distance between 3,4,5 and 6,7,8 is: "+(new Int3(3,4,5) - new Int3(6,7,8)).magnitude); </code>
    /// </summary>
    public float magnitude
    {
        get
        {
            //It turns out that using doubles is just as fast as using ints with Mathf.Sqrt. And this can also handle larger numbers (possibly with small errors when using huge numbers)!

            double _x = x;
            double _y = y;
            double _z = z;

            return (float)System.Math.Sqrt(_x * _x + _y * _y + _z * _z);
        }
    }
    /// <summary>
    /// Magnitude used for the cost between two nodes. The default cost between two nodes can be calculated like this:
    /// <code> int cost = (node1.position-node2.position).costMagnitude; </code>
    ///
    /// This is simply the magnitude, rounded to the nearest integer
    /// </summary>
    public int costMagnitude
    {
        get
        {
            return (int)System.Math.Round(magnitude);
        }
    }
    /// <summary>The squared magnitude of the vector</summary>
    public float sqrMagnitude
    {
        get
        {
            double _x = x;
            double _y = y;
            double _z = z;
            return (float)(_x * _x + _y * _y + _z * _z);
        }
    }
    /// <summary>The squared magnitude of the vector</summary>
    public long sqrMagnitudeLong
    {
        get
        {
            long _x = x;
            long _y = y;
            long _z = z;
            return (_x * _x + _y * _y + _z * _z);
        }
    }
    public static implicit operator string(Int3 obj)
    {
        return obj.ToString();
    }
    /// <summary>Returns a nicely formatted string representing the vector</summary>
    public override string ToString()
    {
        return "( " + x + ", " + y + ", " + z + ")";
    }
    public override bool Equals(System.Object obj)
    {
        if (obj == null) return false;

        var rhs = (Int3)obj;

        return x == rhs.x &&
               y == rhs.y &&
               z == rhs.z;
    }

    #region IEquatable implementation

    public bool Equals(Int3 other)
    {
        return x == other.x && y == other.y && z == other.z;
    }

    #endregion

    public override int GetHashCode()
    {
        return x * 73856093 ^ y * 19349663 ^ z * 83492791;
    }
}
/// <summary>
/// 预制体信息
/// </summary>
[System.Serializable]
public class EndlessPrefab
{
    /// <summary>
    /// 预制体
    /// </summary>
    public GameObject prefab;
    /// <summary>
    /// 密集(每平方世界单位的对象数)
    /// </summary>
    public float density = 0;
    /// <summary>
    /// 乘以[柏林噪声]
    /// 0 ~ 1，表示权重
    /// </summary>
    public float perlin = 0;
    /// <summary>
    /// 柏林噪声的强度
    /// 值越高,边界越清晰
    /// </summary>
    public float perlinPower = 1;
    /// <summary>
    /// 柏林噪声偏移(避免相同的密度图)
    /// </summary>
    public Vector2 perlinOffset = Vector2.zero;
    /// <summary>
    /// 柏林噪声缩放
    /// 较高的值会将密度的最大值和最小值展开
    /// </summary>
    public float perlinScale = 1;
    /// <summary>
    /// 随机值
    /// 0到1，表示权重。
    /// </summary>
    public float random = 1;
    /// <summary>
    /// 如果选中该选项，将在每个地图块的中心创建一个单独的对象
    /// </summary>
    public bool singleFixed = false;
    /// <summary>
    /// 随机旋转
    /// </summary>
    public RotationRandomness randomRotation = RotationRandomness.AllAxes;
}
/// <summary>
/// 随机旋转
/// </summary>
public enum RotationRandomness
{
    AllAxes,
    Y
}
