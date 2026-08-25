#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YangTools.Scripts.Core;

namespace YangTools.EditorStudio.ImageFragmentTool
{
    /// <summary>
    /// 批量生成不规则图片碎片的编辑器窗口。
    /// </summary>
    public sealed class ImageFragmentToolWindow : EditorWindow
    {
        private const string DefaultFolderPath = "Assets";
        private static readonly ImageFragmentAlgorithm[] AlgorithmOptions =
        {
            ImageFragmentAlgorithm.Voronoi,
            ImageFragmentAlgorithm.JitteredGrid,
            ImageFragmentAlgorithm.NoiseBoundary,
            ImageFragmentAlgorithm.AlphaContour,
            ImageFragmentAlgorithm.DelaunayVoronoi,
        };

        private static readonly string[] AlgorithmDisplayNames =
        {
            "Voronoi算法",
            "抖动网格",
            "噪声边界",
            "透明轮廓分割",
            "Delaunay+Voronoi算法",
        };

        [SerializeField] private string inputFolderPath = DefaultFolderPath;
        [SerializeField] private ImageFragmentAlgorithmSettings algorithmSettings = new ImageFragmentAlgorithmSettings();

        /// <summary>
        /// 确保旧窗口序列化数据升级后仍有可用的算法配置。
        /// </summary>
        private void OnEnable()
        {
            if (algorithmSettings == null)
            {
                algorithmSettings = new ImageFragmentAlgorithmSettings();
            }

            if (!Enum.IsDefined(typeof(ImageFragmentAlgorithm), algorithmSettings.algorithm))
            {
                algorithmSettings.algorithm = ImageFragmentAlgorithm.Voronoi;
            }
        }

        /// <summary>
        /// 打开不规则图片切割窗口。
        /// </summary>
        [MenuItem(SettingInfo.YongToolsFunctionPath + "不规则图片切割")]
        private static void OpenWindow()
        {
            GetWindow<ImageFragmentToolWindow>("不规则图片切割");
        }

        /// <summary>
        /// 绘制工具配置界面。
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.LabelField("多算法不规则图片切割", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("递归处理文件夹内图片，为每张原图输出透明 PNG 碎片和 UI 复原 JSON。"
                                    + "同一配置与种子可得到稳定结果。", MessageType.Info);

            DrawFolderField();
            DrawAlgorithmField();
            algorithmSettings.maxFragmentCount = EditorGUILayout.IntSlider("最大碎片数", algorithmSettings.maxFragmentCount, 2, 512);
            algorithmSettings.seed = EditorGUILayout.IntField("随机种子", algorithmSettings.seed);
            algorithmSettings.gapPixels = EditorGUILayout.IntSlider("每片边缘收缩（像素）", algorithmSettings.gapPixels, 0, 32);
            DrawAlgorithmSettings();

            GUILayout.Space(8f);
            if (GUILayout.Button("开始生成", GUILayout.Height(30f)))
            {
                GenerateFragments();
            }
        }

        /// <summary>
        /// 绘制输入文件夹及其选择按钮。
        /// </summary>
        private void DrawFolderField()
        {
            EditorGUILayout.BeginHorizontal();
            inputFolderPath = EditorGUILayout.TextField("输入文件夹", inputFolderPath);
            if (GUILayout.Button("选择", GUILayout.Width(60f)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("选择图片文件夹", GetAbsolutePath(inputFolderPath), string.Empty);
                string assetPath = ConvertToAssetPath(selectedPath);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    inputFolderPath = assetPath;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 使用中文名称绘制切割算法选择框。
        /// </summary>
        private void DrawAlgorithmField()
        {
            int selectedIndex = Array.IndexOf(AlgorithmOptions, algorithmSettings.algorithm);
            selectedIndex = EditorGUILayout.Popup("切割算法", Mathf.Max(0, selectedIndex), AlgorithmDisplayNames);
            algorithmSettings.algorithm = AlgorithmOptions[selectedIndex];
        }

        /// <summary>
        /// 绘制当前算法需要的关键参数。
        /// </summary>
        private void DrawAlgorithmSettings()
        {
            switch (algorithmSettings.algorithm)
            {
                case ImageFragmentAlgorithm.JitteredGrid:
                    algorithmSettings.gridJitter = EditorGUILayout.Slider("网格抖动", algorithmSettings.gridJitter, 0f, 1f);
                    break;
                case ImageFragmentAlgorithm.NoiseBoundary:
                    algorithmSettings.noiseScale = EditorGUILayout.Slider("噪声尺度", algorithmSettings.noiseScale, 8f, 256f);
                    algorithmSettings.noiseStrength = EditorGUILayout.Slider("噪声扭曲", algorithmSettings.noiseStrength, 0f, 64f);
                    break;
                case ImageFragmentAlgorithm.AlphaContour:
                    algorithmSettings.alphaThreshold = (byte)EditorGUILayout.IntSlider("Alpha 阈值", algorithmSettings.alphaThreshold, 0, 255);
                    break;
            }
        }

        /// <summary>
        /// 执行批量图片碎片生成流程。
        /// </summary>
        private void GenerateFragments()
        {
            if (!AssetDatabase.IsValidFolder(inputFolderPath))
            {
                EditorUtility.DisplayDialog("不规则图片切割", "请选择当前项目 Assets 目录下的有效文件夹。", "确定");
                return;
            }

            List<string> sourcePaths = FindSourceTexturePaths(inputFolderPath);
            if (sourcePaths.Count == 0)
            {
                EditorUtility.DisplayDialog("不规则图片切割", "当前文件夹及子文件夹中没有可处理的图片。", "确定");
                return;
            }

            int existingOutputCount = CountExistingOutputFolders(sourcePaths);
            if (existingOutputCount > 0 && !EditorUtility.DisplayDialog("确认覆盖",
                    $"将覆盖 {existingOutputCount} 个已有碎片输出目录。原图不会被修改，是否继续？", "继续", "取消"))
            {
                return;
            }

            ImageFragmentProcessSummary summary = new ImageFragmentProcessSummary();
            List<string> generatedPngPaths = new List<string>();
            try
            {
                for (int i = 0; i < sourcePaths.Count; i++)
                {
                    string sourcePath = sourcePaths[i];
                    EditorUtility.DisplayProgressBar("生成不规则图片碎片", sourcePath,
                        (i + 1f) / sourcePaths.Count);
                    try
                    {
                        GenerateForTexture(sourcePath, generatedPngPaths);
                        summary.successCount++;
                    }
                    catch (Exception exception)
                    {
                        summary.failedPaths.Add(sourcePath + "：" + exception.Message);
                        Debug.LogError($"不规则图片切割失败：{sourcePath}\n{exception}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
            ConfigureFragmentImporters(generatedPngPaths, summary);
            EditorUtility.DisplayDialog("不规则图片切割", summary.GetDisplayText(), "确定");
        }

        /// <summary>
        /// 查找输入目录中尚未作为碎片输出的图片资源。
        /// </summary>
        private static List<string> FindSourceTexturePaths(string folderPath)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] {folderPath});
            List<string> texturePaths = new List<string>(guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (IsFragmentOutputPath(path) || !(AssetImporter.GetAtPath(path) is TextureImporter))
                {
                    continue;
                }

                texturePaths.Add(path);
            }

            texturePaths.Sort(StringComparer.Ordinal);
            return texturePaths;
        }

        /// <summary>
        /// 判断路径是否位于工具生成的碎片输出目录中。
        /// </summary>
        private static bool IsFragmentOutputPath(string assetPath)
        {
            string[] pathParts = assetPath.Split('/');
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                if (pathParts[i].EndsWith("_Fragments", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 统计本次将覆盖的输出目录数量。
        /// </summary>
        private static int CountExistingOutputFolders(List<string> sourcePaths)
        {
            int count = 0;
            for (int i = 0; i < sourcePaths.Count; i++)
            {
                if (Directory.Exists(GetAbsolutePath(GetOutputFolderPath(sourcePaths[i]))))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 为单张图片生成所有碎片并安全替换旧输出。
        /// </summary>
        private void GenerateForTexture(string sourcePath, List<string> generatedPngPaths)
        {
            Texture2D sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (sourceTexture == null)
            {
                throw new InvalidOperationException("无法加载 Texture2D 资源。");
            }

            Color32[] sourcePixels = GetSourcePixels(sourceTexture);
            ImageFragmentAlgorithmSettings settingsForTexture = CreateSettingsForTexture(
                GetEffectiveSeed(algorithmSettings.seed, AssetDatabase.AssetPathToGUID(sourcePath)));
            VoronoiFragmentResult result = ImageFragmentAlgorithmGenerator.Generate(sourceTexture.width, sourceTexture.height,
                sourcePixels, settingsForTexture);
            string outputFolderPath = GetOutputFolderPath(sourcePath);
            string outputAbsolutePath = GetAbsolutePath(outputFolderPath);
            string temporaryAbsolutePath = outputAbsolutePath + ".__temporary_" + Guid.NewGuid().ToString("N");
            List<string> sourceGeneratedPaths = new List<string>(result.fragmentRects.Length);

            try
            {
                Directory.CreateDirectory(temporaryAbsolutePath);
                string sourceName = Path.GetFileNameWithoutExtension(sourcePath);
                for (int fragmentIndex = 0; fragmentIndex < result.fragmentRects.Length; fragmentIndex++)
                {
                    string fileName = ImageFragmentManifestFactory.GetFragmentFileName(sourceName, fragmentIndex);
                    string absoluteFilePath = Path.Combine(temporaryAbsolutePath, fileName);
                    WriteFragmentPng(absoluteFilePath, sourceTexture.width, sourceTexture.height, sourcePixels, result,
                        fragmentIndex);
                    sourceGeneratedPaths.Add(outputFolderPath + "/" + fileName);
                }

                ImageFragmentManifest manifest = ImageFragmentManifestFactory.Create(sourcePath, sourceTexture.width,
                    sourceTexture.height, settingsForTexture, result.fragmentRects);
                string manifestPath = Path.Combine(temporaryAbsolutePath,
                    sourceName + ".fragments.json");
                File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true), new UTF8Encoding(false));

                ReplaceOutputFolder(outputAbsolutePath, temporaryAbsolutePath);
                generatedPngPaths.AddRange(sourceGeneratedPaths);
            }
            finally
            {
                if (Directory.Exists(temporaryAbsolutePath))
                {
                    Directory.Delete(temporaryAbsolutePath, true);
                }
            }
        }

        /// <summary>
        /// 复制当前窗口参数，并写入当前图片的稳定有效种子。
        /// </summary>
        private ImageFragmentAlgorithmSettings CreateSettingsForTexture(int effectiveSeed)
        {
            return new ImageFragmentAlgorithmSettings
            {
                algorithm = algorithmSettings.algorithm,
                maxFragmentCount = algorithmSettings.maxFragmentCount,
                seed = effectiveSeed,
                gapPixels = algorithmSettings.gapPixels,
                gridJitter = algorithmSettings.gridJitter,
                noiseScale = algorithmSettings.noiseScale,
                noiseStrength = algorithmSettings.noiseStrength,
                alphaThreshold = algorithmSettings.alphaThreshold,
            };
        }

        /// <summary>
        /// 获取源图片像素；对关闭 Read/Write 的图片使用临时渲染纹理读取。
        /// </summary>
        private static Color32[] GetSourcePixels(Texture2D sourceTexture)
        {
            if (sourceTexture.isReadable)
            {
                return sourceTexture.GetPixels32();
            }

            RenderTexture temporaryRenderTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture previousRenderTexture = RenderTexture.active;
            Texture2D readableTexture = null;
            try
            {
                Graphics.Blit(sourceTexture, temporaryRenderTexture);
                RenderTexture.active = temporaryRenderTexture;
                readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
                readableTexture.ReadPixels(new Rect(0f, 0f, sourceTexture.width, sourceTexture.height), 0, 0);
                readableTexture.Apply(false, false);
                return readableTexture.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previousRenderTexture;
                if (readableTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(readableTexture);
                }

                RenderTexture.ReleaseTemporary(temporaryRenderTexture);
            }
        }

        /// <summary>
        /// 将一个碎片的保留像素写入裁剪后的透明 PNG。
        /// </summary>
        private static void WriteFragmentPng(string absoluteFilePath, int sourceWidth, int sourceHeight,
            Color32[] sourcePixels, VoronoiFragmentResult result, int fragmentIndex)
        {
            RectInt rect = result.fragmentRects[fragmentIndex];
            Color32[] fragmentPixels = new Color32[rect.width * rect.height];
            for (int y = 0; y < rect.height; y++)
            {
                for (int x = 0; x < rect.width; x++)
                {
                    int sourceX = rect.x + x;
                    int sourceY = rect.y + y;
                    int sourceIndex = sourceY * sourceWidth + sourceX;
                    if (result.owners[sourceIndex] == fragmentIndex && result.retainedPixels[sourceIndex])
                    {
                        fragmentPixels[y * rect.width + x] = sourcePixels[sourceIndex];
                    }
                }
            }

            Texture2D fragmentTexture = new Texture2D(rect.width, rect.height, TextureFormat.RGBA32, false);
            try
            {
                fragmentTexture.SetPixels32(fragmentPixels);
                fragmentTexture.Apply(false, false);
                File.WriteAllBytes(absoluteFilePath, fragmentTexture.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(fragmentTexture);
            }
        }

        /// <summary>
        /// 将临时输出目录替换为最终输出目录，并在异常时恢复旧目录。
        /// </summary>
        private static void ReplaceOutputFolder(string outputAbsolutePath, string temporaryAbsolutePath)
        {
            string backupAbsolutePath = outputAbsolutePath + ".__backup_" + Guid.NewGuid().ToString("N");
            string outputMetaPath = outputAbsolutePath + ".meta";
            string backupMetaPath = backupAbsolutePath + ".meta";
            bool hasBackup = false;
            try
            {
                if (Directory.Exists(outputAbsolutePath))
                {
                    Directory.Move(outputAbsolutePath, backupAbsolutePath);
                    hasBackup = true;
                    if (File.Exists(outputMetaPath))
                    {
                        File.Move(outputMetaPath, backupMetaPath);
                    }
                }

                Directory.Move(temporaryAbsolutePath, outputAbsolutePath);
                if (hasBackup)
                {
                    Directory.Delete(backupAbsolutePath, true);
                    if (File.Exists(backupMetaPath))
                    {
                        File.Delete(backupMetaPath);
                    }
                }
            }
            catch
            {
                if (!Directory.Exists(outputAbsolutePath) && hasBackup && Directory.Exists(backupAbsolutePath))
                {
                    Directory.Move(backupAbsolutePath, outputAbsolutePath);
                    if (File.Exists(backupMetaPath))
                    {
                        File.Move(backupMetaPath, outputMetaPath);
                    }
                }

                throw;
            }
        }

        /// <summary>
        /// 将导出的 PNG 统一设置为 UI 可直接使用的单 Sprite。
        /// </summary>
        private static void ConfigureFragmentImporters(List<string> generatedPngPaths, ImageFragmentProcessSummary summary)
        {
            for (int i = 0; i < generatedPngPaths.Count; i++)
            {
                TextureImporter importer = AssetImporter.GetAtPath(generatedPngPaths[i]) as TextureImporter;
                if (importer == null)
                {
                    summary.failedPaths.Add(generatedPngPaths[i] + "：导入后未找到 TextureImporter。");
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                //importer.spriteAlignment = (int)SpriteAlignment.Center;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// 根据全局种子与资源 GUID 生成稳定的单图种子。
        /// </summary>
        private static int GetEffectiveSeed(int globalSeed, string assetGuid)
        {
            unchecked
            {
                uint hash = 2166136261;
                hash = (hash ^ (uint)globalSeed) * 16777619;
                for (int i = 0; i < assetGuid.Length; i++)
                {
                    hash = (hash ^ assetGuid[i]) * 16777619;
                }

                return (int)(hash & 0x7fffffff);
            }
        }

        /// <summary>
        /// 获取原图对应的碎片输出目录资源路径。
        /// </summary>
        private static string GetOutputFolderPath(string sourcePath)
        {
            string directoryPath = Path.GetDirectoryName(sourcePath)?.Replace('\\', '/') ?? "Assets";
            return directoryPath + "/" + Path.GetFileNameWithoutExtension(sourcePath) + "_Fragments";
        }

        /// <summary>
        /// 将 Assets 相对路径转换为绝对系统路径。
        /// </summary>
        private static string GetAbsolutePath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || assetPath == "Assets")
            {
                return Application.dataPath;
            }

            const string assetsPrefix = "Assets/";
            return assetPath.StartsWith(assetsPrefix, StringComparison.OrdinalIgnoreCase)
                ? Path.Combine(Application.dataPath, assetPath.Substring(assetsPrefix.Length))
                : Application.dataPath;
        }

        /// <summary>
        /// 将文件夹选择框返回的绝对路径转换为 Assets 相对路径。
        /// </summary>
        private static string ConvertToAssetPath(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
            {
                return string.Empty;
            }

            string normalizedAssetsPath = Application.dataPath.Replace('\\', '/').TrimEnd('/');
            string normalizedSelectedPath = absolutePath.Replace('\\', '/').TrimEnd('/');
            if (string.Equals(normalizedSelectedPath, normalizedAssetsPath, StringComparison.OrdinalIgnoreCase))
            {
                return "Assets";
            }

            string assetsPrefix = normalizedAssetsPath + "/";
            if (!normalizedSelectedPath.StartsWith(assetsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("不规则图片切割", "请选择当前 Unity 项目 Assets 目录下的文件夹。", "确定");
                return string.Empty;
            }

            return "Assets/" + normalizedSelectedPath.Substring(assetsPrefix.Length);
        }
    }

    /// <summary>
    /// 记录批处理执行结果。
    /// </summary>
    internal sealed class ImageFragmentProcessSummary
    {
        internal int successCount;
        internal readonly List<string> failedPaths = new List<string>();

        /// <summary>
        /// 获取用于完成弹窗的汇总文本。
        /// </summary>
        internal string GetDisplayText()
        {
            if (failedPaths.Count == 0)
            {
                return $"生成完成，共处理 {successCount} 张图片。";
            }

            return $"生成完成：成功 {successCount} 张，失败 {failedPaths.Count} 张。\n\n"
                   + string.Join("\n", failedPaths);
        }
    }
}
#endif
