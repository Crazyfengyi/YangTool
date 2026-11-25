using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// 网格表面随机生成
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class CreateInMeshPoint : MonoBehaviour
{
    // 在网格表面生成的对象模板
    public GameObject spawnObject;

    // 提供采样网格的组件
    MeshFilter meshFilter;
    // 缓存的网格局部顶点
    Vector3[] vertices;
    // 缓存的网格顶点法线
    Vector3[] normals;
    // 缓存的网格三角形顶点索引
    int[] triangles;
    // 按三角形面积选择落点的抽样器
    WeightedRandom weightedRandom;

    /// <summary>
    /// 缓存网格数据并按三角形面积建立加权抽样表
    /// </summary>
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null) return;

        //预先获取网格的各信息
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        normals = mesh.normals;
        int trisNum = triangles.Length / 3;

        //计算所有三角形的面积
        float[] areas = new float[trisNum];
        Vector3 p1, p2, p3;
        for (int i = 0; i < trisNum; i++)
        {
            p1 = vertices[triangles[i * 3]];
            p2 = vertices[triangles[i * 3 + 1]];
            p3 = vertices[triangles[i * 3 + 2]];
            areas[i] = Vector3.Cross(p1 - p2, p1 - p3).magnitude;
        }

        //准备随机抽签对象
        weightedRandom = new WeightedRandom(areas);
    }

    /// <summary>
    /// 按空格键时在网格表面生成一个对象
    /// </summary>
    void Update()
    {
        //按下空格键后生成对象
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn();
        }
    }

    /// <summary>
    /// 按三角形面积抽样位置并以平均顶点法线确定生成朝向
    /// </summary>
    void Spawn()
    {
        if (spawnObject == null) return;
        if (weightedRandom == null) return;

        //在考虑每个三角形面积的同时获取随机索引
        int randomIndex = weightedRandom.GetRandomIndex();

        int i1 = triangles[randomIndex * 3];
        int i2 = triangles[randomIndex * 3 + 1];
        int i3 = triangles[randomIndex * 3 + 2];

        //计算所选三角形内部的随机坐标
        Vector3 p1 = vertices[i1];
        Vector3 p2 = vertices[i2];
        Vector3 p3 = vertices[i3];
        Vector3 pos = RandomPointInsideTriangle(p1, p2, p3);
        pos = transform.TransformPoint(pos);

        //计算所选三角形的法线方向
        Vector3 n1 = normals[i1];
        Vector3 n2 = normals[i2];
        Vector3 n3 = normals[i3];
        Vector3 normal = (n1 + n2 + n3).normalized;
        normal = transform.TransformDirection(normal);

        //生成
        Instantiate(spawnObject, pos, Quaternion.LookRotation(normal), transform);
    }

    /// <summary>
    /// 使用反射后的重心权重在三角形内部均匀抽取一点
    /// </summary>
    /// <param name="p1">三角形第一个顶点</param>
    /// <param name="p2">三角形第二个顶点</param>
    /// <param name="p3">三角形第三个顶点</param>
    Vector3 RandomPointInsideTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float a = Random.value;
        float b = Random.value;
        if (a + b > 1f)
        {
            a = 1f - a;
            b = 1f - b;
        }

        float c = 1f - a - b;
        return a * p1 + b * p2 + c * p3;
    }
}

/// <summary>
/// 利用Walker’s Alias Method的加权随机抽选类
/// </summary>
public class WeightedRandom
{
    // 构建别名表使用的原始权重数组
    float[] weights;
    // 非负权重总和
    float totalWeight = 0;

    // 别名抽样各槽位的概率阈值
    float[] thresholds;
    // 各抽样槽位对应的补偿索引
    int[] aliases;

    /// <summary>
    /// 创建加权随机对象
    /// </summary>
    /// <param name="weights">各要素的权重列表</param>
    public WeightedRandom(float[] weights)
    {
        ResetWeights(weights);
    }

    /// <summary>
    /// 重新设定加权
    /// </summary>
    /// <param name="weights">各要素的权重列表</param>
    public void ResetWeights(float[] weights)
    {
        int length = (weights != null) ? weights.Length : 0;
        this.weights = new float[length];
        if (length != 0) System.Array.Copy(weights, this.weights, length);

        CreateAliasList();
    }

    /// <summary>
    /// 归一化非负权重并建立常数时间抽样使用的别名表
    /// </summary>
    void CreateAliasList()
    {
        int length = weights.Length;

        thresholds = new float[length];
        aliases = new int[length];
        totalWeight = 0;

        //计算加权的总和
        for (int i = 0; i < length; ++i)
        {
            //权重为负数时设为0
            weights[i] = Mathf.Max(weights[i], 0);
            totalWeight += weights[i];
        }

        //之后为了进行权重的归一化先计算比例
        float normalizeRatio = (totalWeight != 0) ? length / totalWeight : 0;

        Stack<int> small = new Stack<int>();
        Stack<int> large = new Stack<int>();
        for (int i = 0; i < length; i++)
        {
            //别名初始化
            aliases[i] = i;

            //用元素数正规化权重
            float weight = weights[i];
            weight *= normalizeRatio;

            thresholds[i] = weight;

            //按照能控制在块内的重量和不能控制的重量进行分配
            if (weight < 1f)
            {
                small.Push(i);
            }
            else
            {
                large.Push(i);
            }
        }

        //向权重小的要素移动权重大的要素中超出的部分，填满方框
        while (small.Count > 0 && large.Count > 0)
        {
            int s = small.Pop();
            int l = large.Peek();

            aliases[s] = l;
            thresholds[l] = thresholds[l] - (1f - thresholds[s]);

            if (thresholds[l] < 1f)
            {
                small.Push(l);
                large.Pop();
            }
        }
    }

    /// <summary>
    /// 一边考虑各要素的权重一边提取一个索引
    /// </summary>
    public int GetRandomIndex()
    {
        int length = weights.Length;

        float random = Random.value * length;
        int index = (int) random;
        if (index == length) index = length - 1; //Random.value = 1.0的情况
        float weight = random - index;

        // 随机值的小数部分(weight)是块内设定的权重的基准值(thresholds)
        // 超过了就回复别名
        if (weight < thresholds[index])
        {
            return index;
        }

        return aliases[index];
    }
}