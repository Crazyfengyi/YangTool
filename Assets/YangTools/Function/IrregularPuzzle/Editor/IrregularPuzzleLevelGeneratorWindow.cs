#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YangTools.EditorStudio.ImageFragmentTool;

namespace YangTools.Function.IrregularPuzzle.Editor
{
    /// <summary>
    /// 从既有不规则图片切割结果创建 UI 拼图关卡预制体的编辑器窗口。
    /// </summary>
    public sealed class IrregularPuzzleLevelGeneratorWindow : EditorWindow
    {
        private const string DefaultOutputFolder = "Assets/YangTools/Function/IrregularPuzzle/Generated";

        [SerializeField] private DefaultAsset fragmentFolder;
        [SerializeField] private DefaultAsset outputFolder;
        [SerializeField] private string prefabName = "IrregularPuzzleLevel";

        /// <summary>
        /// 打开关卡预制体生成窗口。
        /// </summary>
        [MenuItem("YangTools/辅助工具/不规则拼图/生成拼图关卡")]
        private static void OpenWindow()
        {
            GetWindow<IrregularPuzzleLevelGeneratorWindow>("生成拼图关卡");
        }

        /// <summary>
        /// 绘制生成参数。
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.LabelField("不规则拼图关卡生成", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("选择“不规则图片切割”工具生成的单个碎片目录。生成的 Prefab 需放在已有 Canvas 和 EventSystem 下使用。",
                MessageType.Info);
            fragmentFolder = (DefaultAsset)EditorGUILayout.ObjectField("碎片目录", fragmentFolder, typeof(DefaultAsset), false);
            outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("输出目录", outputFolder, typeof(DefaultAsset), false);
            prefabName = EditorGUILayout.TextField("Prefab 名称", prefabName);
            GUILayout.Space(8f);
            using (new EditorGUI.DisabledScope(!CanGenerate()))
            {
                if (GUILayout.Button("生成关卡 Prefab", GUILayout.Height(30f)))
                {
                    GeneratePrefab();
                }
            }
        }

        /// <summary>
        /// 判断当前资源选择能否开始生成。
        /// </summary>
        private bool CanGenerate()
        {
            return fragmentFolder != null && !string.IsNullOrWhiteSpace(prefabName);
        }

        /// <summary>
        /// 执行完整的清单校验、连接重建和预制体创建流程。
        /// </summary>
        private void GeneratePrefab()
        {
            try
            {
                string fragmentFolderPath = AssetDatabase.GetAssetPath(fragmentFolder);
                ImageFragmentManifest manifest = LoadManifest(fragmentFolderPath, out _);
                string targetFolderPath = GetOrCreateOutputFolder();
                string targetPath = targetFolderPath + "/" + prefabName + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) != null && !EditorUtility.DisplayDialog("覆盖关卡 Prefab",
                        $"将覆盖已有资源：{targetPath}\n是否继续？", "覆盖", "取消"))
                {
                    return;
                }

                CreatePrefab(fragmentFolderPath, manifest, targetPath);
                EditorUtility.DisplayDialog("生成拼图关卡", $"已生成：{targetPath}", "确定");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("生成拼图关卡失败", exception.Message, "确定");
            }
        }

        /// <summary>
        /// 从碎片目录中加载唯一的清单文件。
        /// </summary>
        private static ImageFragmentManifest LoadManifest(string folderPath, out string manifestPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                throw new InvalidOperationException("请选择 Assets 目录下有效的碎片输出文件夹。");
            }

            string[] manifestGuids = AssetDatabase.FindAssets("t:TextAsset", new[] { folderPath });
            List<string> manifestPaths = new List<string>();
            for (int i = 0; i < manifestGuids.Length; i++)
            {
                string candidate = AssetDatabase.GUIDToAssetPath(manifestGuids[i]);
                if (candidate.EndsWith(".fragments.json", StringComparison.OrdinalIgnoreCase))
                {
                    manifestPaths.Add(candidate);
                }
            }

            if (manifestPaths.Count != 1)
            {
                throw new InvalidOperationException("碎片目录中必须且只能包含一个 .fragments.json 清单文件。");
            }

            manifestPath = manifestPaths[0];
            string json = File.ReadAllText(ToAbsolutePath(manifestPath), Encoding.UTF8);
            ImageFragmentManifest manifest = JsonUtility.FromJson<ImageFragmentManifest>(json);
            if (manifest == null || manifest.fragments == null || manifest.fragments.Length == 0)
            {
                throw new InvalidOperationException("碎片清单为空或格式无效。");
            }

            return manifest;
        }

        /// <summary>
        /// 创建并保存包含碎片及连接数据的预制体。
        /// </summary>
        private static void CreatePrefab(string fragmentFolderPath, ImageFragmentManifest manifest, string targetPath)
        {
            Color32[] sourcePixels = GetSourcePixels(manifest.sourceAssetPath);
            ImageFragmentAlgorithmSettings settings = CloneSettingsWithoutGap(manifest.algorithmSettings, manifest);
            VoronoiFragmentResult result = ImageFragmentAlgorithmGenerator.Generate(manifest.sourceWidth, manifest.sourceHeight,
                sourcePixels, settings);
            if (result.fragmentRects.Length != manifest.fragments.Length)
            {
                throw new InvalidOperationException("重建后的碎片数量与清单不一致，无法安全生成连接信息。请重新使用当前版本的图片切割工具生成碎片。");
            }

            IrregularPuzzleConnection[] connections = IrregularPuzzleConnectionBuilder.Build(manifest.sourceWidth,
                manifest.sourceHeight, result.owners);
            if (connections.Length == 0 && manifest.fragments.Length > 1)
            {
                throw new InvalidOperationException("没有检测到碎片之间的共享边界，无法生成拼图关卡。");
            }

            GameObject rootObject = new GameObject(Path.GetFileNameWithoutExtension(targetPath), typeof(RectTransform));
            try
            {
                RectTransform rootTransform = rootObject.GetComponent<RectTransform>();
                rootTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rootTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rootTransform.pivot = new Vector2(0.5f, 0.5f);
                rootTransform.sizeDelta = new Vector2(manifest.sourceWidth, manifest.sourceHeight);
                IrregularPuzzleLevel level = rootObject.AddComponent<IrregularPuzzleLevel>();
                RectTransform piecesRoot = CreatePiecesRoot(rootTransform);
                IrregularPuzzlePiece[] pieces = CreatePieces(fragmentFolderPath, manifest, level, piecesRoot);
                level.Configure(pieces, connections, new Vector2(manifest.sourceWidth, manifest.sourceHeight));
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, targetPath);
                Selection.activeObject = prefab;
                AssetDatabase.SaveAssets();
            }
            finally
            {
                DestroyImmediate(rootObject);
            }
        }

        /// <summary>
        /// 创建用于组织初始碎片节点的容器。
        /// </summary>
        private static RectTransform CreatePiecesRoot(RectTransform parent)
        {
            GameObject piecesObject = new GameObject("Pieces", typeof(RectTransform));
            RectTransform piecesTransform = piecesObject.GetComponent<RectTransform>();
            piecesTransform.SetParent(parent, false);
            piecesTransform.anchorMin = new Vector2(0.5f, 0.5f);
            piecesTransform.anchorMax = new Vector2(0.5f, 0.5f);
            piecesTransform.pivot = new Vector2(0.5f, 0.5f);
            piecesTransform.sizeDelta = Vector2.zero;
            return piecesTransform;
        }

        /// <summary>
        /// 根据清单中的 PNG 和复原坐标创建所有 UI 碎片。
        /// </summary>
        private static IrregularPuzzlePiece[] CreatePieces(string fragmentFolderPath, ImageFragmentManifest manifest,
            IrregularPuzzleLevel level, RectTransform parent)
        {
            IrregularPuzzlePiece[] pieces = new IrregularPuzzlePiece[manifest.fragments.Length];
            for (int i = 0; i < manifest.fragments.Length; i++)
            {
                ImageFragmentEntry entry = manifest.fragments[i];
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fragmentFolderPath + "/" + entry.fileName);
                if (sprite == null)
                {
                    throw new InvalidOperationException($"缺少碎片 Sprite：{entry.fileName}");
                }

                GameObject pieceObject = new GameObject($"Piece_{entry.index:D3}", typeof(RectTransform), typeof(Image),
                    typeof(IrregularPuzzlePiece));
                RectTransform pieceTransform = pieceObject.GetComponent<RectTransform>();
                pieceTransform.SetParent(parent, false);
                pieceTransform.anchorMin = new Vector2(0.5f, 0.5f);
                pieceTransform.anchorMax = new Vector2(0.5f, 0.5f);
                pieceTransform.pivot = new Vector2(0.5f, 0.5f);
                pieceTransform.sizeDelta = entry.sourceRect.size;
                pieceTransform.anchoredPosition = entry.uiLocalCenter;
                Image image = pieceObject.GetComponent<Image>();
                image.sprite = sprite;
                image.raycastTarget = true;
                IrregularPuzzlePiece piece = pieceObject.GetComponent<IrregularPuzzlePiece>();
                piece.Configure(level, entry.index, entry.uiLocalCenter);
                pieces[i] = piece;
            }

            return pieces;
        }

        /// <summary>
        /// 克隆切割参数并固定为零间隙，使内部边界可以用于连接分析。
        /// </summary>
        private static ImageFragmentAlgorithmSettings CloneSettingsWithoutGap(ImageFragmentAlgorithmSettings source,
            ImageFragmentManifest manifest)
        {
            ImageFragmentAlgorithmSettings settings = source ?? new ImageFragmentAlgorithmSettings();
            return new ImageFragmentAlgorithmSettings
            {
                algorithm = manifest.algorithm,
                maxFragmentCount = settings.maxFragmentCount,
                seed = manifest.seed,
                gapPixels = 0,
                gridJitter = settings.gridJitter,
                noiseScale = settings.noiseScale,
                noiseStrength = settings.noiseStrength,
                alphaThreshold = settings.alphaThreshold,
            };
        }

        /// <summary>
        /// 读取原图像素；不可读贴图会通过临时渲染纹理转换。
        /// </summary>
        private static Color32[] GetSourcePixels(string sourcePath)
        {
            Texture2D sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (sourceTexture == null)
            {
                throw new InvalidOperationException($"无法加载清单原图：{sourcePath}");
            }

            if (sourceTexture.isReadable)
            {
                return sourceTexture.GetPixels32();
            }

            RenderTexture renderTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture previous = RenderTexture.active;
            Texture2D readableTexture = null;
            try
            {
                Graphics.Blit(sourceTexture, renderTexture);
                RenderTexture.active = renderTexture;
                readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
                readableTexture.ReadPixels(new Rect(0f, 0f, sourceTexture.width, sourceTexture.height), 0, 0);
                readableTexture.Apply(false, false);
                return readableTexture.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                if (readableTexture != null)
                {
                    DestroyImmediate(readableTexture);
                }

                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        /// <summary>
        /// 获取输出目录，不存在时创建默认目录。
        /// </summary>
        private string GetOrCreateOutputFolder()
        {
            string selectedPath = outputFolder == null ? DefaultOutputFolder : AssetDatabase.GetAssetPath(outputFolder);
            if (AssetDatabase.IsValidFolder(selectedPath))
            {
                return selectedPath;
            }

            if (selectedPath != DefaultOutputFolder)
            {
                throw new InvalidOperationException("输出目录必须是 Assets 下已有文件夹。\n可留空以使用默认目录。");
            }

            CreateAssetFolders(DefaultOutputFolder);
            return DefaultOutputFolder;
        }

        /// <summary>
        /// 递归创建 Assets 下缺失的目录。
        /// </summary>
        private static void CreateAssetFolders(string assetPath)
        {
            string[] segments = assetPath.Split('/');
            string current = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }
        }

        /// <summary>
        /// 将项目内 Asset 路径转换为绝对路径。
        /// </summary>
        private static string ToAbsolutePath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }
    }
}
#endif
