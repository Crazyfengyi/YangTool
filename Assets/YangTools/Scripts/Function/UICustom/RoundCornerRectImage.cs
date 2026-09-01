using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace GameMain
{
    /// <summary>
    /// 绘制带圆角的 UI 图片
    /// </summary>
    public class RoundCornerRectImage : Image
    {
        // 圆角半径
        public float cornerRadius = 10f;

        // 每个圆角的边数
        public int cornerSegments = 20;

        // 是否绘制边框
        public bool outline;

        // 边框颜色
        public Color outlineColor;

        /// <summary>
        /// 生成圆角图片网格
        /// </summary>
        /// <param name="toFill">待填充的顶点辅助器</param>
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();

            Rect rect = GetPixelAdjustedRect();
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float radius = Mathf.Clamp(cornerRadius, 0f, Mathf.Min(rect.width, rect.height) * 0.5f);
            if (radius <= 0f || cornerSegments <= 0)
            {
                base.OnPopulateMesh(toFill);
                return;
            }

            int segments = Mathf.Clamp(cornerSegments, 16, 32);

            Color imageColor = color;
            if (!outline && imageColor.a <= 0.001f)
            {
                return;
            }

            Vector4 uvRect = GetUVRect();
            AddRoundedRect(toFill, rect, radius, outline ? outlineColor : imageColor, uvRect, rect, segments);

            if (!outline || outlineColor.a <= 0.001f)
            {
                return;
            }

            Rect innerRect = new Rect(
                rect.x + radius,
                rect.y + radius,
                rect.width - radius * 2f,
                rect.height - radius * 2f);
            if (innerRect.width <= 0f || innerRect.height <= 0f || imageColor.a <= 0.001f)
            {
                return;
            }

            AddRoundedRect(toFill, innerRect, 0f, imageColor, uvRect, rect, segments);
        }

        /// <summary>
        /// 添加一个圆角矩形的三角形扇面
        /// </summary>
        /// <param name="toFill">待填充的顶点辅助器</param>
        /// <param name="rect">圆角矩形范围</param>
        /// <param name="radius">圆角半径</param>
        /// <param name="vertexColor">顶点颜色</param>
        /// <param name="uvRect">图片 UV 范围</param>
        /// <param name="uvSourceRect">UV 对应的原始矩形范围</param>
        /// <param name="segments">每个圆角的边数</param>
        private static void AddRoundedRect(
            VertexHelper toFill,
            Rect rect,
            float radius,
            Color vertexColor,
            Vector4 uvRect,
            Rect uvSourceRect,
            int segments)
        {
            int perimeterCount = segments * 4;
            int centerIndex = toFill.currentVertCount;
            toFill.AddVert(GetPosition(rect.center), vertexColor, GetUV(rect.center, uvSourceRect, uvRect));

            for (int i = 0; i < 4; i++)
            {
                float startAngle = 180f + i * 90f;
                Vector2 cornerCenter = GetCornerCenter(rect, radius, i);

                for (int j = 0; j < segments; j++)
                {
                    float angle = (startAngle + j * 90f / segments) * Mathf.Deg2Rad;
                    Vector2 position = cornerCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    toFill.AddVert(GetPosition(position), vertexColor, GetUV(position, uvSourceRect, uvRect));
                }
            }

            for (int i = 0; i < perimeterCount; i++)
            {
                int nextIndex = i + 1 < perimeterCount ? i + 1 : 0;
                toFill.AddTriangle(centerIndex, centerIndex + i + 1, centerIndex + nextIndex + 1);
            }
        }

        /// <summary>
        /// 获取圆角中心点
        /// </summary>
        /// <param name="rect">圆角矩形范围</param>
        /// <param name="radius">圆角半径</param>
        /// <param name="cornerIndex">圆角索引</param>
        /// <returns>圆角中心点</returns>
        private static Vector2 GetCornerCenter(Rect rect, float radius, int cornerIndex)
        {
            switch (cornerIndex)
            {
                case 0:
                    return new Vector2(rect.xMin + radius, rect.yMin + radius);
                case 1:
                    return new Vector2(rect.xMax - radius, rect.yMin + radius);
                case 2:
                    return new Vector2(rect.xMax - radius, rect.yMax - radius);
                default:
                    return new Vector2(rect.xMin + radius, rect.yMax - radius);
            }
        }

        /// <summary>
        /// 获取图片 UV 范围
        /// </summary>
        /// <returns>图片 UV 范围</returns>
        private Vector4 GetUVRect()
        {
            Sprite activeSprite = overrideSprite != null ? overrideSprite : sprite;
            return activeSprite == null ? new Vector4(0f, 0f, 1f, 1f) : DataUtility.GetOuterUV(activeSprite);
        }

        /// <summary>
        /// 根据顶点位置计算图片 UV
        /// </summary>
        /// <param name="position">顶点位置</param>
        /// <param name="sourceRect">UV 对应的原始矩形范围</param>
        /// <param name="uvRect">图片 UV 范围</param>
        /// <returns>顶点 UV</returns>
        private static Vector2 GetUV(Vector2 position, Rect sourceRect, Vector4 uvRect)
        {
            float x = sourceRect.width <= 0f ? 0f : Mathf.InverseLerp(sourceRect.xMin, sourceRect.xMax, position.x);
            float y = sourceRect.height <= 0f ? 0f : Mathf.InverseLerp(sourceRect.yMin, sourceRect.yMax, position.y);
            return new Vector2(Mathf.Lerp(uvRect.x, uvRect.z, x), Mathf.Lerp(uvRect.y, uvRect.w, y));
        }

        /// <summary>
        /// 将二维坐标转换为三维顶点坐标
        /// </summary>
        /// <param name="position">二维坐标</param>
        /// <returns>三维顶点坐标</returns>
        private static Vector3 GetPosition(Vector2 position)
        {
            return new Vector3(position.x, position.y, 0f);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 在编辑器中刷新网格
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif
    }
}
