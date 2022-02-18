using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;
using UnityEngine.Events;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Text;
using YangTools;

public static partial class Extend
{

    #region 动画相关
    /// <summary>
    /// 获取状态机中动画长度
    /// </summary>
    /// <param name="clipName">动画片段的名称</param>
    /// <returns>可空类型-没找到返回null</returns>
    public static float? GetAnimLength(this Animator anim, string clipName)
    {
        if (anim == null || string.IsNullOrEmpty(clipName) || anim.runtimeAnimatorController == null)
        {
            return null;
        }

        RuntimeAnimatorController runtimeAniController = anim.runtimeAnimatorController;
        AnimationClip[] clips = runtimeAniController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name.Equals(clipName))
            {
                return clips[i].length;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取状态机中的动画Clip
    /// </summary>
    /// <param name="clipName">动画片段名</param>
    public static AnimationClip GetAnimClipByName(this Animator anim, string clipName)
    {
        if (anim == null || string.IsNullOrEmpty(clipName) || anim.runtimeAnimatorController == null)
        {
            return null;
        }
        RuntimeAnimatorController runtimeAniController = anim.runtimeAnimatorController;
        AnimationClip[] clips = runtimeAniController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name.Equals(clipName))
            {
                return clips[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 动态插入事件
    /// </summary>
    /// <param name="clipName">动画片段名</param>
    /// <param name="funName">事件函数名</param>
    /// <param name="time">插入的时间点</param>
    /// <returns>成功失败</returns>
    public static bool AddEventToAnimtionClip(this Animator anim, string clipName, string funName, float time)
    {
        if (anim == null || string.IsNullOrEmpty(clipName) || anim.runtimeAnimatorController == null) return false;
        float? animLength = anim.GetAnimLength(clipName);
        if (animLength == null) return false;

        AnimationEvent newEvent = new AnimationEvent();
        newEvent.functionName = funName;

        if (time > animLength) return false;

        newEvent.time = time;
        anim.GetAnimClipByName(clipName).AddEvent(newEvent);
        anim.Rebind();
        return true;
    }
    #endregion

    #region 类型扩展
    /// <summary>
    /// 克隆List
    /// </summary>
    /// <param name="tList"></param>
    public static List<T> GetClone<T>(this List<T> tList)
    {
        List<T> newList = new List<T>();
        for (int i = 0; i < tList.Count; i++)
        {
            newList.Add(tList[i]);
        }
        return newList;
    }

    /// <summary>
    /// 得到当前位置到目标点的夹角（0-360）
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float GetAngle(this Transform self, Transform target)
    {
        var dirA = self.position - Vector3.Project(self.position, Vector3.up);
        var dirB = target.position - Vector3.Project(target.position, Vector3.up);
        Vector3 angle = Quaternion.LookRotation(dirB - dirA).eulerAngles;
        return angle.y;
    }

    /// <summary>
    /// 判断一个UnityEvent中是否有存在的指定方法
    /// </summary>
    /// <param name="uEvent"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static MethodInfo HasMethod(this UnityEvent uEvent, string methodName)
    {
        for (int i = 0; i < uEvent.GetPersistentEventCount(); i++)
        {
            if (uEvent.GetPersistentMethodName(i).Equals(methodName.ToString()))
            {
                Type type = uEvent.GetPersistentTarget(i).GetType();
                MethodInfo methodInfo = type.GetMethod(methodName);
                return methodInfo;
            }
        }
        return null;
    }

    #endregion

    #region 射线和网格合并
    /// <summary>
    /// 根据一个碰撞体的边界返回一个碰撞信息
    /// </summary>
    /// <param name="mainCollider">碰撞体</param>
    /// <param name="origin">起点</param>
    /// <param name="dir">方向</param>
    /// <param name="dis">长度</param>
    /// <param name="layerMask">层级</param>
    /// <param name="containCollider">目标碰撞体</param>
    /// <returns></returns>
    public static RaycastHit? GetHitInfo(this Collider mainCollider, Vector3 origin, Vector3 dir, float dis, int layerMask, Collider containCollider = null)
    {
        Debug.DrawRay(origin, dir * dis, Color.red);
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dis, layerMask))
        {
            if (containCollider == null) return hit;
            if (hit.collider == containCollider)
                return hit;
        }
        Debug.DrawRay(origin + dir * mainCollider.bounds.extents.z, dir * dis, Color.red);
        if (Physics.Raycast(origin + dir * mainCollider.bounds.extents.z, dir, out hit, dis, layerMask))
        {
            if (containCollider == null) return hit;
            if (hit.collider == containCollider)
                return hit;
        }
        Debug.DrawRay(origin - dir * mainCollider.bounds.extents.z, dir * dis, Color.red);
        if (Physics.Raycast(origin - dir * mainCollider.bounds.extents.z, dir, out hit, dis, layerMask))
        {
            if (containCollider == null) return hit;
            if (hit.collider == containCollider)
                return hit;
        }
        Debug.DrawRay(origin + Quaternion.Euler(0, 90, 0) * dir * mainCollider.bounds.extents.x, dir * dis, Color.red);
        if (Physics.Raycast(origin + Quaternion.Euler(0, 90, 0) * dir * mainCollider.bounds.extents.x, dir, out hit, dis, layerMask))
        {
            if (containCollider == null) return hit;
            if (hit.collider == containCollider)
                return hit;
        }
        Debug.DrawRay(origin - Quaternion.Euler(0, 90, 0) * dir * mainCollider.bounds.extents.x, dir * dis, Color.red);
        if (Physics.Raycast(origin - Quaternion.Euler(0, 90, 0) * dir * mainCollider.bounds.extents.x, dir, out hit, dis, layerMask))
        {
            if (containCollider == null) return hit;
            if (hit.collider == containCollider)
                return hit;
        }
        Debug.DrawRay(origin + Quaternion.Euler(90, 0, 0) * dir * mainCollider.bounds.extents.y, dir * dis, Color.red);
        if (Physics.Raycast(origin + Quaternion.Euler(90, 0, 0) * dir * mainCollider.bounds.extents.y, dir, out hit, dis, layerMask))
        {
            if (containCollider == null) return hit;
            if (hit.collider == containCollider)
                return hit;
        }
        Debug.DrawRay(origin - Quaternion.Euler(90, 0, 0) * dir * mainCollider.bounds.extents.y, dir * dis, Color.red);
        if (Physics.Raycast(origin - Quaternion.Euler(90, 0, 0) * dir * mainCollider.bounds.extents.y, dir, out hit, dis, layerMask))
        {
            if (containCollider == null) return hit;
            if (hit.collider == containCollider)
                return hit;
        }
        return null;
    }

    /// <summary>
    /// 合并蒙皮网格，刷新骨骼
    /// 注意：合并后的网格会使用同一个Material
    /// </summary>
    /// <param name="root">角色根物体</param>
    public static void Combine(Transform root, Material material, string ignoreTag = null)
    {
        // 遍历所有蒙皮网格渲染器，以计算出所有需要合并的网格、UV、骨骼的信息
        var allSMR = root.GetComponentsInChildren<SkinnedMeshRenderer>();
        List<GameObject> targetParts = new List<GameObject>();
        float startTime = Time.realtimeSinceStartup;
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        List<Transform> boneList = new List<Transform>();
        Transform[] transforms = root.GetComponentsInChildren<Transform>();
        List<Texture2D> textures = new List<Texture2D>();
        int width = 0;
        int height = 0;
        int uvCount = 0;
        List<Vector2[]> uvList = new List<Vector2[]>();
        for (int i = 0; i < allSMR.Length; i++)
        {
            SkinnedMeshRenderer smr = allSMR[i];

            try
            {
                if (ignoreTag != null && smr.gameObject.CompareTag(ignoreTag)) continue;
                if (!smr.gameObject.Equals(root.gameObject)) targetParts.Add(smr.gameObject);
                for (int sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
                {
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = smr.sharedMesh;
                    ci.subMeshIndex = sub;
                    combineInstances.Add(ci);
                }
                uvList.Add(smr.sharedMesh.uv);
                uvCount += smr.sharedMesh.uv.Length;
                if (smr.material.mainTexture != null)
                {
                    textures.Add(smr.GetComponent<Renderer>().material.mainTexture as Texture2D);
                    width += smr.GetComponent<Renderer>().material.mainTexture.width;
                    height += smr.GetComponent<Renderer>().material.mainTexture.height;
                }
                for (int j = 0; j < smr.bones.Length; j++)
                {
                    Transform bone = smr.bones[j];
                    for (int k = 0; k < transforms.Length; k++)
                    {
                        Transform item = transforms[k];
                        if (item.name != bone.name) continue;
                        boneList.Add(item);
                        break;
                    }
                }
            }
            catch (Exception)
            {
                Debug.LogError("" + smr.name);
                throw;
            }

        }
        // 获取并配置角色所有的SkinnedMeshRenderer
        SkinnedMeshRenderer tempRenderer = root.gameObject.GetComponent<SkinnedMeshRenderer>();
        if (!tempRenderer)
        {
            tempRenderer = root.gameObject.AddComponent<SkinnedMeshRenderer>();
        }

        tempRenderer.sharedMesh = new Mesh();

        // 合并网格，刷新骨骼，附加材质
        tempRenderer.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, false);
        tempRenderer.bones = boneList.ToArray();
        tempRenderer.material = material;

        Texture2D skinnedMeshAtlas = new Texture2D(YangTools.Extend.YangExtend.GetThanPowerOfTwo(width), YangTools.Extend.YangExtend.GetThanPowerOfTwo(height));
        Rect[] packingResult = skinnedMeshAtlas.PackTextures(textures.ToArray(), 0);
        Vector2[] atlasUVs = new Vector2[uvCount];

        // 因为将贴图都整合到了一张图片上，所以需要重新计算UV
        int count = 0;
        for (int i = 0; i < uvList.Count; i++)
        {
            for (int j = 0; j < uvList[i].Length; j++)
            {
                Vector2 uv = uvList[i][j];
                atlasUVs[count].x = Mathf.Lerp(packingResult[i].xMin, packingResult[i].xMax, uv.x);
                atlasUVs[count].y = Mathf.Lerp(packingResult[i].yMin, packingResult[i].yMax, uv.y);
                count++;
            }
        }

        // 设置贴图和UV
        tempRenderer.material.mainTexture = skinnedMeshAtlas;
        tempRenderer.sharedMesh.uv = atlasUVs;

        //销毁部件
        for (int i = 0; i < targetParts.Count; i++)
        {
            if (targetParts[i])
            {
                GameObject.DestroyImmediate(targetParts[i]);
            }
        }
        Debug.Log("合并耗时 : " + (Time.realtimeSinceStartup - startTime) * 1000 + " ms");
    }


    private static List<Collider> colliders = new List<Collider>();
    /// <summary>
    /// 在一定宽度区间内绘制一定精度条的射线
    /// </summary>
    /// <param name="beginPot">起点</param>
    /// <param name="endPot">终点</param>
    /// <param name="width">宽度</param>
    /// <param name="precision">精度（必须是单数）</param>
    private static List<Collider> DrawRays(Vector3 beginPot, Vector3 endPot, float width, int precision, LayerMask layerMask)
    {
        var offsetDir = Quaternion.Euler(0, 90, 0) * (endPot - beginPot).normalized;
        var perWidthOffset = width / (precision - 1);
        RaycastHit hit;
        for (int i = 0; i < (precision + 1) / 2; i++)
        {
            Vector3 offset = perWidthOffset * i * offsetDir;
            Ray[] rays = new Ray[] { new Ray((beginPot + offset), endPot - beginPot), new Ray((beginPot - offset), endPot - beginPot) };
            float length = Vector3.Distance(beginPot, endPot);
            for (int j = 0; j < rays.Length; j++)
            {
                if (Physics.Raycast(rays[j], out hit, length, layerMask))
                {
                    if (!colliders.Contains(hit.collider))
                    {
                        colliders.Add(hit.collider);
                    }
                }
            }
        }
        return colliders;
    }

    /// <summary>
    /// 获取模型大概大小
    /// </summary>
    public static float GetModelWrapAxis(Transform model)
    {
        var modelSmrs = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        List<Transform> bones = new List<Transform>();
        Vector3 center = Vector3.zero;
        Vector3 maxPot = Vector3.one * -Mathf.Infinity;
        Vector3 minPot = Vector3.one * Mathf.Infinity;
        Vector3 size = Vector3.zero;
        for (int i = 0; i < modelSmrs.Length; i++)
        {
            var smrBones = modelSmrs[i].bones;
            for (int j = 0; j < smrBones.Length; j++)
            {
                if (!smrBones[j]) continue;
                bones.Add(smrBones[j]);
                center += smrBones[j].position;
                maxPot.x = Mathf.Max(smrBones[j].position.x, maxPot.x);
                maxPot.y = Mathf.Max(smrBones[j].position.y, maxPot.y);
                maxPot.z = Mathf.Max(smrBones[j].position.z, maxPot.z);
                minPot.x = Mathf.Min(smrBones[j].position.x, minPot.x);
                minPot.y = Mathf.Min(smrBones[j].position.y, minPot.y);
                minPot.z = Mathf.Min(smrBones[j].position.z, minPot.z);
            }
        }
        size = maxPot - minPot;

        return Mathf.Min(size.y, size.z);//只需要包裹核心部分
    }

    /// <summary>
    /// 合并网格
    /// </summary>
    /// <param name="self"></param>
    /// <param name="layMask"></param>
    public static void CombineMeshByLayer(this GameObject self, int layMask = ~0)
    {
        MeshFilter[] meshFilters = self.GetComponentsInChildren<MeshFilter>(true);       //获取自身和所有子物体中所有MeshFilter组件
        Dictionary<string, List<MeshFilter>> combineDic = new Dictionary<string, List<MeshFilter>>();
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (!meshFilters[i].gameObject.activeSelf || !meshFilters[i].transform.parent.gameObject.activeSelf || !meshFilters[i].GetComponent<MeshRenderer>() || !meshFilters[i].GetComponent<MeshRenderer>().enabled)//若网格被隐藏或者物体被隐藏，直接跳过
                continue;
            if ((1 << meshFilters[i].gameObject.layer & layMask) == 0)//如果该层被屏蔽则跳过
                continue;
            var MR = meshFilters[i].GetComponent<MeshRenderer>();
            for (int j = 0; j < MR.materials.Length; j++)
            {
                var matName = MR.materials[j];
                if (combineDic.ContainsKey(LayerMask.LayerToName(meshFilters[i].gameObject.layer) + "_" + matName))
                {
                    combineDic[LayerMask.LayerToName(meshFilters[i].gameObject.layer) + "_" + matName].Add(meshFilters[i]);
                }
                else
                {
                    List<MeshFilter> values = new List<MeshFilter>();
                    values.Add(meshFilters[i]);
                    combineDic.Add(LayerMask.LayerToName(meshFilters[i].gameObject.layer) + "_" + matName, values);//用名字和材质球来分组
                }
            }

        }
        foreach (var comb in combineDic.Keys)
        {
            var meshFs = combineDic[comb];
            var mat = meshFs[0].GetComponent<MeshRenderer>().material;
            int vertexCout = 0;
            int combineCount = 0;
            List<Mesh> meshList = new List<Mesh>();
            List<Matrix4x4> matrixList = new List<Matrix4x4>();
            for (int i = 0; i < meshFs.Count; i++)
            {
                vertexCout += meshFs[i].sharedMesh.vertexCount;

                if (vertexCout >= UInt16.MaxValue || i == (meshFs.Count - 1))
                {
                    CombineInstance[] combine = new CombineInstance[combineCount];    //新建CombineInstance数组

                    for (int j = 0; j < meshList.Count; j++)
                    {
                        combine[j].mesh = meshList[j];
                        combine[j].transform = matrixList[j];
                    }
                    GameObject go = new GameObject(comb);
                    go.layer = LayerMask.NameToLayer(go.name.Split('_')[0]);
                    if (self.transform.Find(comb.Split('_')[0]))
                    {
                        go.transform.SetParent(self.transform.Find(comb.Split('_')[0]));
                        go.transform.position = Vector3.zero;
                    }
                    else
                    {
                        GameObject layerParent = new GameObject(comb.Split('_')[0]);
                        layerParent.transform.SetParent(self.gameObject.transform);
                        layerParent.transform.position = Vector3.zero;
                        layerParent.layer = LayerMask.NameToLayer(go.name.Split('_')[0]);
                        go.transform.SetParent(layerParent.transform);
                        go.transform.position = Vector3.zero;
                    }
                    var goMesh = go.AddComponent<MeshFilter>();
                    var goMeshRenderer = go.AddComponent<MeshRenderer>();
                    goMesh.mesh = new Mesh();
                    goMesh.sharedMesh.CombineMeshes(combine);//合并
                    goMeshRenderer.material = mat;

                    meshList.Clear();
                    matrixList.Clear();
                    vertexCout = 0;
                    combineCount = 0;
                    if (i != (meshFs.Count - 1))
                    {
                        i--;
                    }
                }
                else
                {
                    combineCount++;
                    meshList.Add(meshFs[i].sharedMesh);
                    matrixList.Add(meshFs[i].transform.localToWorldMatrix);
                    if (meshFs[i].GetComponent<MeshRenderer>())
                        GameObject.Destroy(meshFs[i].GetComponent<MeshRenderer>());
                    GameObject.Destroy(meshFs[i]);
                }
            }
        }
        Debug.Log("合并了" + meshFilters.Length + "个网格");
    }

    /// <summary>
    /// 进行一次扇形射线检测
    /// </summary>
    /// /// <param name="rotate">旋转方向</param>
    /// <param name="castRange">检测范围，只需要输入一半的范围，如输入45则从-45—45度检测</param>
    /// <param name="precision">检测精度，决定射线的数量，推荐与范围值的比在4:3左右</param>
    /// <param name="length">射线长度</param>
    /// <param name="mask">mask层</param>
    /// <param name="flipRayDir">是否翻转射线方向,如果是,则射线由终点向起点发射</param>
    /// <returns>符合的碰撞信息</returns>
    public static List<Collider> SectorRayCast(Vector3 pot, Quaternion rotate, float castRange, int precision, float length, LayerMask mask, bool flipRayDir = false)
    {
        float spacing = castRange * 2 / precision;//每条线之间的间隔
        List<Collider> colliders = new List<Collider>();
        for (int i = 0; i < precision + 1; i++)//中间有一条对称轴，所以条数为精度+1
        {
            Debug.DrawRay(pot, (Quaternion.Euler(0, -castRange + (i * spacing), 0) * rotate) * Vector3.forward * length, Color.red, 5f);
            if (flipRayDir)
            {
                RaycastHit[] hits = Physics.RaycastAll(pot + (Quaternion.Euler(0, -castRange + (i * spacing), 0) * rotate) * Vector3.forward * length, (Quaternion.Euler(0, -castRange + (i * spacing), 0) * rotate) * Vector3.forward * -1f, length, mask);
                for (int j = 0; j < hits.Length; j++)
                {
                    if (!colliders.Contains(hits[j].collider))
                        colliders.Add(hits[j].collider);
                }
            }
            else
            {
                RaycastHit[] hits = Physics.RaycastAll(pot, (Quaternion.Euler(0, -castRange + (i * spacing), 0) * rotate) * Vector3.forward, length, mask);
                for (int j = 0; j < hits.Length; j++)
                {
                    if (!colliders.Contains(hits[j].collider))
                        colliders.Add(hits[j].collider);
                }
            }
        }
        return colliders;
    }

    /// <summary>
    /// 进行一次扇形射线检测
    /// </summary>
    /// /// <param name="pot">起点</param>
    /// /// <param name="rotate">旋转方向</param>
    /// <param name="castRange">检测范围，只需要输入一半的范围，如输入45则从-45—45度检测</param>
    /// <param name="precision">检测精度，决定射线的数量，推荐与范围值的比在4:3左右</param>
    /// <param name="length">射线长度</param>
    /// <param name="mask">mask层</param>
    /// <param name="flipRayDir">是否翻转射线方向,如果是,则射线由终点向起点发射</param>
    /// <returns>符合的碰撞信息</returns>
    public static List<RaycastHit> GetResultsBySectorRayCast(Vector3 pot, Quaternion rotate, float castRange, int precision, float length, LayerMask mask, bool flipRayDir = false)
    {
        float spacing = castRange * 2 / precision;//每条线之间的间隔
        List<Collider> colliders = new List<Collider>();
        List<RaycastHit> results = new List<RaycastHit>();
        for (int i = 0; i < precision + 1; i++)//中间有一条对称轴，所以条数为精度+1
        {
            Quaternion rotateQ = Quaternion.Euler(-castRange + (i * spacing), 0, 0) * rotate;
            Debug.DrawRay(pot, rotateQ * Vector3.forward * length, Color.red, 5f);
            if (flipRayDir)
            {
                RaycastHit[] hits = Physics.RaycastAll(pot + rotateQ * Vector3.forward * length, rotateQ * Vector3.forward * -1f, length, mask);
                for (int j = 0; j < hits.Length; j++)
                {
                    if (!colliders.Contains(hits[j].collider))
                    {
                        results.Add(hits[j]);
                        colliders.Add(hits[j].collider);
                    }
                }
            }
            else
            {
                RaycastHit[] hits = Physics.RaycastAll(pot, rotateQ * Vector3.forward, length, mask);
                for (int j = 0; j < hits.Length; j++)
                {
                    if (!colliders.Contains(hits[j].collider))
                    {
                        results.Add(hits[j]);
                        colliders.Add(hits[j].collider);
                    }
                }
            }
        }
        return results;
    }

    #endregion

    #region 计算角度和克隆
    /// <summary>
    /// 计算两个向量的夹角
    /// </summary>
    /// <returns></returns>
    public static float GetAngle(Vector3 formDir, Vector3 toDir, Vector3 normal)
    {
        float angle = Vector3.Angle(formDir, toDir); //求出两向量之间的夹角 
        Vector3 dirsNormal = Vector3.Cross(formDir, toDir);//叉乘求出法线向量 
        angle *= Mathf.Sign(Vector3.Dot(dirsNormal, normal));  //求法线向量与物体上方向向量点乘，结果为1或-1，修正旋转方向 
                                                               //int rotateSymbol = angle >= 0 ? 1 : -1;
        return angle;
    }

    /// <summary>
    /// 克隆类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="RealObject"></param>
    /// <returns></returns>
    public static T Clone<T>(T RealObject)
    {
        using (Stream objStream = new MemoryStream())
        {
            //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(objStream, RealObject);
            objStream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(objStream);
        }
    }

    #endregion
}
