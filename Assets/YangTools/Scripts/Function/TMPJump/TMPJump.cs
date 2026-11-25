using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

/// <summary>
/// 使用 DOTween 驱动 TMP 字符顶点位移与颜色淡入淡出
/// </summary>
public class TMPJump : MonoBehaviour
{
    // 需要驱动字符网格动画的 TMP 文本
    private TMP_Text textTMP;
    // 当前文本的字符与网格信息缓存
    private TMP_TextInfo textInfo;

    // 是否循环
    [LabelText("是否循环")]
    public bool isLoop;
    // 是否改变颜色
    [LabelText("是否改变颜色")]
    public bool changeColor;
    // 偏移位置
    [LabelText("偏移位置")]
    public Vector3 endPos;
    // 字符的错位时间
    [LabelText("字符的错位时间")]
    public float waitTime = 0.2f;
    // 字符的动画时间
    [LabelText("字符的动画时间")]
    public float duration = 0.5f;
    // 字符的渐入时间
    [LabelText("字符的渐入时间")]
    public float fadeInTime = 0.5f;
    // 字符的淡出时间
    [LabelText("字符的淡出时间")]
    public float fadeOutTime = 1f;
    // 字符的最终颜色
    [LabelText("字符的最终颜色")]
    public Color endColor = Color.white;
    // 是否手动设置曲线
    [LabelText("是否手动设置曲线")]
    public bool animationCurveEase;
    // 手动的跳跃曲线
    [LabelText("手动的跳跃曲线")]
    public AnimationCurve easeCurve;
    // 手动的淡入淡出曲线
    [LabelText("手动的淡入淡出曲线")]
    public AnimationCurve fadeCurve;
    // 跳跃曲线
    [LabelText("跳跃曲线")]
    public Ease ease = Ease.OutElastic;
    // 淡入淡出曲线
    [LabelText("淡入淡出曲线")]
    public Ease fadeEase = Ease.Linear;

    /// <summary>
    /// 为可见字符创建依次错开的位移与淡出动画并按配置循环
    /// </summary>
    [Button]
    [ContextMenu("文字跳动")]
    public void DoJump()
    {
        if (textTMP == null)
        {
            TryGetComponent(out textTMP);
        }

        textInfo = textTMP.textInfo;
        var count = Mathf.Min(textInfo.characterCount, textInfo.characterInfo.Length);

        for (int i = 0; i < count; i++)
        {
            var characterInfo = textInfo.characterInfo[i];
            if (!characterInfo.isVisible)
            {
                continue;
            }
            var pos = Vector3.zero;

            textTMP.ForceMeshUpdate();
            var materialIndex = characterInfo.materialReferenceIndex;
            var meshInfo = textInfo.meshInfo[materialIndex];
            var vertexIndex = characterInfo.vertexIndex;

            var color = meshInfo.colors32[vertexIndex];
            color.a = 0;

            var oriPos = GetOriPos(meshInfo, vertexIndex);

            var characterSequence = DOTween.Sequence();

            if (animationCurveEase)
            {
                characterSequence.Insert(i * waitTime,
                    DOTween.To(() => pos, x => pos = x, endPos, duration).SetEase(easeCurve));
                characterSequence.Insert(i * waitTime,
                    DOTween.To(() => color, x => color = x, endColor, fadeInTime).SetEase(fadeCurve));
                characterSequence.Insert(count * waitTime + duration,
                    DOTween.ToAlpha(() => color, x => color = x, 0, fadeOutTime).SetEase(fadeCurve));
            }
            else
            {
                characterSequence.Insert(i * waitTime,
                    DOTween.To(() => pos, x => pos = x, endPos, duration).SetEase(ease));
                characterSequence.Insert(i * waitTime,
                    DOTween.To(() => color, x => color = x, endColor, fadeInTime).SetEase(fadeEase));
                characterSequence.Insert(count * waitTime + duration,
                    DOTween.ToAlpha(() => color, x => color = x, 0, fadeOutTime).SetEase(fadeEase));
            }

            characterSequence
                .OnUpdate(() => { SetVertexPosition(meshInfo, vertexIndex, pos, oriPos, color); });

        }

        var sequence = DOTween.Sequence();
        sequence.AppendInterval(count * waitTime + duration + fadeOutTime + 1);
        sequence.OnComplete(() =>
        {
            if (isLoop)
            {
                DoJump();
            }
        });
    }

    /// <summary>
    /// 基于原始顶点写入字符位移与颜色并上传 TMP 网格
    /// </summary>
    /// <param name="meshInfo">字符所在材质的网格信息</param>
    /// <param name="vertexIndex">字符四边形的首个顶点索引</param>
    /// <param name="pos">相对原始字符位置的偏移</param>
    /// <param name="oriPos">字符的四个原始顶点位置</param>
    /// <param name="color">本次绘制或动画使用的颜色</param>
    private void SetVertexPosition(TMP_MeshInfo meshInfo, int vertexIndex, Vector3 pos, IReadOnlyList<Vector3> oriPos,
        Color color)
    {
        for (int j = 0; j < 4; j++)
        {
            meshInfo.vertices[vertexIndex + j] = oriPos[j] + pos;
            if (changeColor)
            {
                meshInfo.colors32[vertexIndex + j] = color;
            }
            else
            {
                meshInfo.colors32[vertexIndex + j].a = (byte)(color.a * 255);
            }
        }

        textTMP.UpdateVertexData();
    }

    /// <summary>
    /// 复制指定字符的四个原始顶点位置
    /// </summary>
    /// <param name="meshInfo">字符所在材质的网格信息</param>
    /// <param name="vertexIndex">字符四边形的首个顶点索引</param>
    private Vector3[] GetOriPos(TMP_MeshInfo meshInfo, int vertexIndex)
    {
        var pos = new Vector3[4];
        for (int j = 0; j < 4; j++)
        {
            pos[j] = meshInfo.vertices[vertexIndex + j];
        }

        return pos;
    }
}