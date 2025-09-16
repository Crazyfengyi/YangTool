using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using YangTools.Log;
using Object = UnityEngine.Object;

namespace YangTools.Scripts.Core.YangExtend
{
    public static partial class YangExtend
    {
        #region GameObject扩展

        /// <summary>
        /// 删除所有子节点
        /// </summary>
        public static void DestroyAllChild(this Transform content)
        {
            foreach (Transform item in content)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
        }

        /// <summary>
        /// 在物体世界坐标播放声音
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="clipName">音效名称</param>
        public static void PlayAtPoint(this GameObject obj, AudioClip clipName)
        {
            AudioSource.PlayClipAtPoint(clipName, obj.transform.position);
        }

        /// <summary>
        /// 刷新自己和子节点的ContentSizeFitter(倒序刷新)
        /// </summary>
        /// <param name="node">目标节点</param>
        public static void RefreshAllContentSizeFitter(this GameObject node)
        {
            ContentSizeFitter[] allComponent = node.transform.GetComponentsInChildren<ContentSizeFitter>(true);

            for (int i = allComponent.Length - 1; i >= 0; i--)
            {
                RectTransform rectTrans = allComponent[i].GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTrans);
            }
        }

        /// <summary>
        /// 刷新自身和父节点ContentSizeFitter(直至根节点)
        /// </summary>
        /// <param name="node">目标节点</param>
        public static void UpdateContent(this GameObject node)
        {
            Transform content = node.transform;
            while (content != null)
            {
                if (content.TryGetComponent<ContentSizeFitter>(out ContentSizeFitter fitter))
                {
                    RectTransform rectTrans = content.GetComponent<RectTransform>();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTrans);
                }

                content = content.parent;
            }
        }

        /// <summary>
        /// 获取或增加组件。
        /// </summary>
        /// <typeparam name="T">要获取或增加的组件</typeparam>
        /// <param name="gameObject">目标对象</param>
        /// <returns>获取或增加的组件</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (!component)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        private static readonly List<Transform> CachedTransforms = new List<Transform>();

        /// <summary>
        /// 递归设置游戏对象的层次
        /// </summary>
        /// <param name="gameObject">对象</param>
        /// <param name="layer">目标层次的编号</param>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.GetComponentsInChildren(true, CachedTransforms);
            for (int i = 0; i < CachedTransforms.Count; i++)
            {
                CachedTransforms[i].gameObject.layer = layer;
            }

            CachedTransforms.Clear();
        }

        /// <summary>
        /// 获得某物体的bounds中心点
        /// </summary>
        public static Vector3 GetBoundsCenter(this GameObject target)
        {
            return GetBounds(target).center;
        }

        /// <summary>
        /// 获得某物体的bounds
        /// </summary>
        public static Bounds GetBounds(this GameObject target)
        {
            Renderer[] mrs = target.GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds();
            if (mrs.Length > 0)
            {
                bounds = mrs[0].bounds;
            }

            for (int i = 0; i < mrs.Length; i++)
            {
                bounds.Encapsulate(mrs[i].bounds);
            }

            return bounds;
        }

        /// <summary>
        /// 自动设置显隐--会先判断是否已经是目标状态
        /// </summary>
        public static void AutoSetActive(this GameObject gameObject, bool isActive, [CallerMemberName] string callName = "")
        {
            if (!gameObject)
            {
                Debuger.ToError($"尝试对null对象设置显隐:调用来源{callName}");
                return;
            }

            //与或--相同取0，不同取1
            if (isActive ^ gameObject.activeSelf)
            {
                gameObject.SetActive(isActive);
            }
        }

        #endregion

        #region 对象池扩展

        /// <summary>
        /// 对象池默认Get
        /// </summary>
        public static void DefaultGameObjectOnGet(this GameObject gameObject, Transform parent)
        {
            gameObject.SetActive(true);
            gameObject.transform.SetParent(parent);
        }

        /// <summary>
        /// 对象池默认Recycle
        /// </summary>
        public static void DefaultGameObjectRecycle(this GameObject gameObject)
        {
            gameObject.SetActive(false);
            gameObject.transform.SetParent(YangToolsManager.GamePoolObject.transform);
        }

        /// <summary>
        /// 对象池默认Destroy
        /// </summary>
        public static void DefaultGameObjectDestroy(this GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }

        #endregion

        #region 网格相关

        /// <summary>
        /// 合并蒙皮网格，刷新骨骼
        /// 注意:合并后的网格会使用同一个Material
        /// </summary>
        /// <param name="root">角色根物体</param>
        /// <param name="material"></param>
        /// <param name="ignoreTag"></param>
        public static void Combine(Transform root, Material material, string ignoreTag = null)
        {
            // 遍历所有蒙皮网格渲染器，以计算出所有需要合并的网格、UV、骨骼的信息
            var allSmr = root.GetComponentsInChildren<SkinnedMeshRenderer>();
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
            for (int i = 0; i < allSmr.Length; i++)
            {
                SkinnedMeshRenderer smr = allSmr[i];
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

            Texture2D skinnedMeshAtlas = new Texture2D(YangExtend.GetThanPowerOfTwo(width),
                YangExtend.GetThanPowerOfTwo(height));
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
                    Object.DestroyImmediate(targetParts[i]);
                }
            }

            Debug.Log("合并耗时 : " + (Time.realtimeSinceStartup - startTime) * 1000 + " ms");
        }

        /// <summary>
        /// 合并网格
        /// </summary>
        /// <param name="self"></param>
        /// <param name="layMask"></param>
        public static void CombineMeshByLayer(this GameObject self, int layMask = ~0)
        {
            MeshFilter[] meshFilters = self.GetComponentsInChildren<MeshFilter>(true); //获取自身和所有子物体中所有MeshFilter组件
            Dictionary<string, List<MeshFilter>> combineDic = new Dictionary<string, List<MeshFilter>>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (!meshFilters[i].gameObject.activeSelf || !meshFilters[i].transform.parent.gameObject.activeSelf ||
                    !meshFilters[i].GetComponent<MeshRenderer>() ||
                    !meshFilters[i].GetComponent<MeshRenderer>().enabled) //若网格被隐藏或者物体被隐藏，直接跳过
                    continue;
                if ((1 << meshFilters[i].gameObject.layer & layMask) == 0) //如果该层被屏蔽则跳过
                    continue;
                var mr = meshFilters[i].GetComponent<MeshRenderer>();
                for (int j = 0; j < mr.materials.Length; j++)
                {
                    var matName = mr.materials[j];
                    if (combineDic.ContainsKey(LayerMask.LayerToName(meshFilters[i].gameObject.layer) + "_" + matName))
                    {
                        combineDic[LayerMask.LayerToName(meshFilters[i].gameObject.layer) + "_" + matName]
                            .Add(meshFilters[i]);
                    }
                    else
                    {
                        List<MeshFilter> values = new List<MeshFilter> { meshFilters[i] };
                        combineDic.Add(LayerMask.LayerToName(meshFilters[i].gameObject.layer) + "_" + matName,
                            values); //用名字和材质球来分组
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
                        CombineInstance[] combine = new CombineInstance[combineCount]; //新建CombineInstance数组

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
                        goMesh.sharedMesh.CombineMeshes(combine); //合并
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
                            Object.Destroy(meshFs[i].GetComponent<MeshRenderer>());
                        Object.Destroy(meshFs[i]);
                    }
                }
            }

            Debug.Log("合并了" + meshFilters.Length + "个网格");
        }

        /// <summary>
        /// 获取模型大概大小
        /// </summary>
        public static float GetModelWrapAxis(Transform model)
        {
            SkinnedMeshRenderer[] modelSmr = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            //List<Transform> bones = new List<Transform>();
            Vector3 center = Vector3.zero;
            Vector3 maxPot = Vector3.one * -Mathf.Infinity;
            Vector3 minPot = Vector3.one * Mathf.Infinity;
            foreach (var skinnedMeshRender in modelSmr)
            {
                var smrBones = skinnedMeshRender.bones;
                foreach (var temp in smrBones)
                {
                    if (!temp) continue;
                    //bones.Add(temp);
                    center += temp.position;
                    maxPot.x = Mathf.Max(temp.position.x, maxPot.x);
                    maxPot.y = Mathf.Max(temp.position.y, maxPot.y);
                    maxPot.z = Mathf.Max(temp.position.z, maxPot.z);
                    minPot.x = Mathf.Min(temp.position.x, minPot.x);
                    minPot.y = Mathf.Min(temp.position.y, minPot.y);
                    minPot.z = Mathf.Min(temp.position.z, minPot.z);
                }
            }
            Vector3 size = maxPot - minPot;
            return Mathf.Min(size.y, size.z);//只需要包裹核心部分
        }

        #endregion

        #region 动画相关

        /// <summary>
        /// 获取状态机中动画长度
        /// </summary>
        /// <param name="anim"></param>
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
        /// <param name="anim"></param>
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
        /// 动态插入事件,失败则返回false
        /// </summary>
        /// <param name="anim"></param>
        /// <param name="clipName">动画片段名</param>
        /// <param name="function">事件函数名</param>
        /// <param name="time">插入的时间点</param>
        /// <returns></returns>
        public static bool AddEventToAnimationClip(this Animator anim, string clipName, string function, float time)
        {
            if (anim == null || string.IsNullOrEmpty(clipName) || anim.runtimeAnimatorController == null)
                return false;
            var animLength = anim.GetAnimLength(clipName);
            if (animLength <= 0)
                return false;
            RuntimeAnimatorController ac = anim.runtimeAnimatorController;
            AnimationEvent newEvent = new AnimationEvent();
            newEvent.functionName = function;
            if (time > animLength)
                return false;
            newEvent.time = time;
            anim.GetAnimClipByName(clipName).AddEvent(newEvent);
            anim.Rebind();
            return true;
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
            Vector3 dirsNormal = Vector3.Cross(formDir, toDir); //叉乘求出法线向量 
            //求法线向量与物体上方向向量点乘，结果为1或-1，修正旋转方向 
            angle *= Mathf.Sign(Vector3.Dot(dirsNormal, normal)); //int rotateSymbol = angle >= 0 ? 1 : -1;

            return angle;
        }

        #endregion

        #region 坐标转换
        /*
         * unity 屏幕坐标转相机坐标
         *  // 获取鼠标在屏幕上的位置
            Vector3 mousePosition = Input.mousePosition;
            // 将屏幕坐标转换为相机坐标
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            
            Ray temp = mahjongCamera.ScreenPointToRay(mousePosition);
         */
        /// <summary>
        /// 获取自身的世界坐标(父节点不能为空-通过父节点坐标系转)
        /// </summary>
        /// <param name="tran">自身</param>
        public static Vector3 LocalPosToWordPos(Transform tran)
        {
            return tran.parent.TransformPoint(tran.localPosition);
        }

        /// <summary>
        /// 世界坐标转局部坐标
        /// </summary>
        /// <param name="tran">转换的坐标系原点</param>
        /// <param name="wordSpace">世界坐标</param>
        public static Vector3 WordPosToLocalPos(Transform tran, Vector3 wordSpace)
        {
            return tran.InverseTransformPoint(wordSpace);
        }

        /// <summary>
        /// 局部坐标转局部坐标(父节点不能为空-通过父节点坐标系转)
        /// </summary>
        /// <param name="localTran">被转换的节点(它的局部坐标)</param>
        /// <param name="targetTran">目标父节点(目标局部原点)</param>
        public static Vector3 LocalPosToLocalPos(Transform localTran, Transform targetTran)
        {
            Transform parent = localTran.parent;
            if (parent != null)
            {
                Vector3 wordSpace = parent.TransformPoint(localTran.localPosition);
                return targetTran.InverseTransformPoint(wordSpace);
            }

            return default;
        }

        /// <summary>
        /// 世界坐标转UI的局部坐标
        /// </summary>
        /// <param name="worldCamara">场景相机</param>
        /// <param name="uiCamara">UI相机</param>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="targetParent">目标节点</param>
        public static Vector3 WorldPosToUILocalPos(Camera worldCamara, Camera uiCamara, Vector3 worldPos,
            Transform targetParent)
        {
            //世界坐标转屏幕坐标
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamara, worldPos);
            //屏幕坐标转局部坐标
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent.GetComponent<RectTransform>(),
                    screenPoint, uiCamara, out Vector2 localPoint))
            {
                return localPoint;
            }

            return default;
        }

        /// <summary>
        /// 屏幕坐标转成UI坐标
        /// </summary>
        /// <param name="parent">目标父节点(坐标系原点)</param>
        /// <param name="screenPos">屏幕点位置</param>
        /// <param name="uiCamera">UI相机</param>
        public static Vector3 ScreenPosToUIPos(RectTransform parent, Vector2 screenPos, Camera uiCamera)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, uiCamera,
                    out Vector2 localPoint))
            {
                return localPoint;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 屏幕点是否在矩形中
        /// </summary>
        public static bool ScreenPosIsInRect(RectTransform rect, Vector2 screenPoint, Camera cam = null)
        {
            if (cam)
            {
                return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, cam);
            }
            else
            {
                return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint);
            }
        }

        #endregion

        #region 通用UI动画

        /// <summary>
        /// UGUI窗口打开动画
        /// </summary>
        /// <param name="rectTrans">动画对象</param>
        /// <param name="callback">回调</param>
        public static void WindowOpenAni(RectTransform rectTrans, Action<RectTransform> callback = null)
        {
            DOTween.Kill(rectTrans, true);

            Vector2 oldPivot = rectTrans.pivot;
            Vector2 oldScale = rectTrans.localScale;
            rectTrans.pivot = new Vector2(0.5f, 0.5f);
            rectTrans.localScale = oldScale * 0.36f;

            rectTrans.DOScale(oldScale, 0.16f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    rectTrans.pivot = oldPivot;
                    rectTrans.localScale = oldScale;
                    callback?.Invoke(rectTrans);
                })
                .SetTarget(rectTrans);
        }

        #endregion
        
        #region 网络图片
        /// <summary>
        /// 设置网络图片
        /// </summary>
        public static IEnumerator SetHttpUrl(this Image selfImage,string imageUrl)
        {
            selfImage.sprite = null;
            if (string.IsNullOrEmpty(imageUrl))
            {
                Debug.Log("下载图片地址为null");
                //TODO:默认头像
                //ResourceMgr.Instance.SetImageSprite(selfImage, "default");
                yield break;
            }
            //Debug.Log($"开始下载图片:{imageUrl}");
            using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"下载图片失败:{webRequest.error}:{imageUrl}");
            }
            else
            {
                //获取下载的纹理
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                selfImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        #endregion
    }
}