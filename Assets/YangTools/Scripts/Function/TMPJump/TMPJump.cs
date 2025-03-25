using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TMPJump : MonoBehaviour
{
    private TMP_Text textTMP;
    private TMP_TextInfo textInfo;

    [LabelText("是否循环")]
    public bool isLoop;
    [LabelText("是否改变颜色")]
    public bool changeColor;
    [LabelText("偏移位置")]
    public Vector3 endPos;
    [LabelText("字符的错位时间")]
    public float waitTime = 0.2f;
    [LabelText("字符的动画时间")]
    public float duration = 0.5f;
    [LabelText("字符的渐入时间")]
    public float fadeInTime = 0.5f;
    [LabelText("字符的淡出时间")]
    public float fadeOutTime = 1f;
    [LabelText("字符的最终颜色")]
    public Color endColor = Color.white;
    [LabelText("是否手动设置曲线")]
    public bool animationCurveEase;
    [LabelText("手动的跳跃曲线")]
    public AnimationCurve easeCurve;
    [LabelText("手动的淡入淡出曲线")]
    public AnimationCurve fadeCurve;
    [LabelText("跳跃曲线")]
    public Ease ease = Ease.OutElastic;
    [LabelText("淡入淡出曲线")]
    public Ease fadeEase = Ease.Linear;

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