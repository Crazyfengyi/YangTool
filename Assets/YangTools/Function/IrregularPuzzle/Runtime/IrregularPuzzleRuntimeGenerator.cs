using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YangTools.Function.IrregularPuzzle
{
    /// <summary>
    /// 运行时拼图生成进度事件
    /// </summary>
    [Serializable]
    public sealed class IrregularPuzzleProgressEvent : UnityEvent<float>
    {
    }

    /// <summary>
    /// 运行时拼图关卡生成完成事件
    /// </summary>
    [Serializable]
    public sealed class IrregularPuzzleGeneratedEvent : UnityEvent<IrregularPuzzleLevel>
    {
    }

    /// <summary>
    /// 运行时拼图生成失败事件
    /// </summary>
    [Serializable]
    public sealed class IrregularPuzzleFailedEvent : UnityEvent<string>
    {
    }

    /// <summary>
    /// 根据外部 Texture2D 动态创建不规则 UI 拼图关卡
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class IrregularPuzzleRuntimeGenerator : MonoBehaviour
    {
        [SerializeField] private RectTransform levelParent;
        [SerializeField] private RuntimePuzzleFragmentSettings fragmentSettings = new RuntimePuzzleFragmentSettings();
        [SerializeField] private IrregularPuzzleProgressEvent onProgress = new IrregularPuzzleProgressEvent();
        [SerializeField] private IrregularPuzzleGeneratedEvent onGenerated = new IrregularPuzzleGeneratedEvent();
        [SerializeField] private IrregularPuzzleFailedEvent onFailed = new IrregularPuzzleFailedEvent();

        private readonly List<UnityEngine.Object> generatedObjects = new List<UnityEngine.Object>();
        private Coroutine generationCoroutine;
        private IrregularPuzzleLevel generatedLevel;

        /// <summary>当前生成的关卡。</summary>
        public IrregularPuzzleLevel GeneratedLevel => generatedLevel;

        /// <summary>切图参数。</summary>
        public RuntimePuzzleFragmentSettings FragmentSettings => fragmentSettings;

        /// <summary>生成进度事件。</summary>
        public IrregularPuzzleProgressEvent OnProgress => onProgress;

        /// <summary>生成完成事件。</summary>
        public IrregularPuzzleGeneratedEvent OnGenerated => onGenerated;

        /// <summary>生成失败事件。</summary>
        public IrregularPuzzleFailedEvent OnFailed => onFailed;

        /// <summary>开始使用外部传入的图片生成关卡。</summary>
        public void Generate(Texture2D sourceTexture)
        {
            CancelGeneration();
            ClearGeneratedLevel();
            if (sourceTexture == null)
            {
                Fail("传入的拼图图片为空。");
                return;
            }

            generationCoroutine = StartCoroutine(GenerateRoutine(sourceTexture));
        }

        /// <summary>取消尚未完成的生成任务。</summary>
        public void CancelGeneration()
        {
            if (generationCoroutine == null) return;
            StopCoroutine(generationCoroutine);
            generationCoroutine = null;
        }

        /// <summary>销毁组件禁用时遗留的生成任务和运行时资源。</summary>
        private void OnDestroy()
        {
            CancelGeneration();
            ClearGeneratedLevel();
        }

        /// <summary>协程执行像素分析、连接生成与 UI 碎片创建。</summary>
        private IEnumerator GenerateRoutine(Texture2D sourceTexture)
        {
            Color32[] pixels;
            RuntimePuzzleFragmentResult result;
            IrregularPuzzleConnection[] connections;
            try
            {
                ReportProgress(0f);
                pixels = GetPixels(sourceTexture);
                result = RuntimePuzzleFragmentGenerator.Generate(sourceTexture.width, sourceTexture.height,
                    pixels, fragmentSettings);
                connections = RuntimePuzzleConnectionBuilder.Build(sourceTexture.width,
                    sourceTexture.height, result.connectionOwners);
            }
            catch (Exception exception)
            {
                generationCoroutine = null;
                ClearGeneratedLevel();
                Fail(exception.Message);
                Debug.LogException(exception, this);
                yield break;
            }

            ReportProgress(0.35f);
            yield return null;
            generatedLevel = CreateLevelRoot(sourceTexture.width, sourceTexture.height, out RectTransform piecesRoot);
            IrregularPuzzlePiece[] pieces = new IrregularPuzzlePiece[result.fragmentRects.Length];
            for (int i = 0; i < pieces.Length; i++)
            {
                try
                {
                    pieces[i] = CreatePiece(i, sourceTexture.width, sourceTexture.height, pixels, result,
                        generatedLevel, piecesRoot);
                }
                catch (Exception exception)
                {
                    generationCoroutine = null;
                    ClearGeneratedLevel();
                    Fail(exception.Message);
                    Debug.LogException(exception, this);
                    yield break;
                }

                ReportProgress(0.35f + 0.6f * (i + 1f) / pieces.Length);
                yield return null;
            }

            generatedLevel.Configure(pieces, connections, new Vector2(sourceTexture.width, sourceTexture.height));
            generatedLevel.gameObject.SetActive(true);
            ReportProgress(1f);
            generationCoroutine = null;
            onGenerated?.Invoke(generatedLevel);
        }

        /// <summary>创建动态关卡和初始碎片容器。</summary>
        private IrregularPuzzleLevel CreateLevelRoot(int width, int height, out RectTransform piecesRoot)
        {
            RectTransform parent = levelParent == null ? transform as RectTransform : levelParent;
            GameObject levelObject = new GameObject("RuntimeIrregularPuzzleLevel", typeof(RectTransform));
            levelObject.SetActive(false);
            levelObject.AddComponent<IrregularPuzzleLevel>();
            RectTransform levelTransform = levelObject.GetComponent<RectTransform>();
            levelTransform.SetParent(parent, false);
            levelTransform.anchorMin = new Vector2(0.5f, 0.5f);
            levelTransform.anchorMax = new Vector2(0.5f, 0.5f);
            levelTransform.pivot = new Vector2(0.5f, 0.5f);
            levelTransform.sizeDelta = new Vector2(width, height);
            GameObject piecesObject = new GameObject("Pieces", typeof(RectTransform));
            piecesRoot = piecesObject.GetComponent<RectTransform>();
            piecesRoot.SetParent(levelTransform, false);
            piecesRoot.anchorMin = new Vector2(0.5f, 0.5f);
            piecesRoot.anchorMax = new Vector2(0.5f, 0.5f);
            piecesRoot.pivot = new Vector2(0.5f, 0.5f);
            piecesRoot.sizeDelta = Vector2.zero;
            return levelObject.GetComponent<IrregularPuzzleLevel>();
        }

        /// <summary>由单块像素数据创建 UI Image 和拼图组件。</summary>
        private IrregularPuzzlePiece CreatePiece(int pieceIndex, int width, int height, Color32[] sourcePixels,
            RuntimePuzzleFragmentResult result, IrregularPuzzleLevel level, RectTransform parent)
        {
            RectInt rect = result.fragmentRects[pieceIndex];
            Texture2D texture = new Texture2D(rect.width, rect.height, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[rect.width * rect.height];
            for (int y = 0; y < rect.height; y++)
            for (int x = 0; x < rect.width; x++)
            {
                int sourceIndex = (rect.y + y) * width + rect.x + x;
                if (result.owners[sourceIndex] == pieceIndex && result.retainedPixels[sourceIndex])
                    pixels[y * rect.width + x] = sourcePixels[sourceIndex];
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, rect.width, rect.height), new Vector2(0.5f, 0.5f),
                1f);
            generatedObjects.Add(sprite);
            generatedObjects.Add(texture);
            GameObject pieceObject = new GameObject($"Piece_{pieceIndex:D3}", typeof(RectTransform), typeof(Image),
                typeof(IrregularPuzzlePiece));
            RectTransform pieceTransform = pieceObject.GetComponent<RectTransform>();
            pieceTransform.SetParent(parent, false);
            pieceTransform.anchorMin = new Vector2(0.5f, 0.5f);
            pieceTransform.anchorMax = new Vector2(0.5f, 0.5f);
            pieceTransform.pivot = new Vector2(0.5f, 0.5f);
            pieceTransform.sizeDelta = rect.size;
            Vector2 solvedPosition = rect.center - new Vector2(width * 0.5f, height * 0.5f);
            pieceTransform.anchoredPosition = solvedPosition;
            Image image = pieceObject.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = true;
            IrregularPuzzlePiece piece = pieceObject.GetComponent<IrregularPuzzlePiece>();
            piece.Configure(level, pieceIndex, solvedPosition);
            return piece;
        }

        /// <summary>读取可读或 GPU 贴图的完整像素。</summary>
        private static Color32[] GetPixels(Texture2D source)
        {
            if (source.isReadable) return source.GetPixels32();
            RenderTexture renderTexture =
                RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Texture2D readable = null;
            try
            {
                Graphics.Blit(source, renderTexture);
                RenderTexture.active = renderTexture;
                readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                readable.ReadPixels(new Rect(0f, 0f, source.width, source.height), 0, 0);
                readable.Apply(false, false);
                return readable.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                if (readable != null) Destroy(readable);
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        /// <summary>释放上一局的根节点以及所有由生成器创建的贴图资源。</summary>
        private void ClearGeneratedLevel()
        {
            if (generatedLevel != null) Destroy(generatedLevel.gameObject);
            generatedLevel = null;
            for (int i = 0; i < generatedObjects.Count; i++)
            {
                if (generatedObjects[i] != null) Destroy(generatedObjects[i]);
            }

            generatedObjects.Clear();
        }

        /// <summary>触发进度回调。</summary>
        private void ReportProgress(float progress)
        {
            onProgress?.Invoke(Mathf.Clamp01(progress));
        }

        /// <summary>触发失败回调。</summary>
        private void Fail(string message)
        {
            onFailed?.Invoke(message);
        }
    }
}