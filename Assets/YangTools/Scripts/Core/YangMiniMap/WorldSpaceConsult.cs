using UnityEngine;
using UnityEngine.Serialization;

namespace YangTools.Scripts.Core.YangMiniMap
{
    /// <summary>
    /// 世界空间参考
    /// </summary>
    public class WorldSpaceConsult : MonoBehaviour {

        [FormerlySerializedAs("GizmoColor")] [Header("Use UI editor Tool for scale the wordSpace")]
        public Color gizmoColor = new Color(1, 1, 1, 0.75f);

        /// <summary>
        /// Debugging world space of map
        /// </summary>
        void OnDrawGizmosSelected()
        {
            RectTransform r = this.GetComponent<RectTransform>();
            Vector3 v = r.sizeDelta;
            Vector3 pivot = r.localPosition;
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(pivot, new Vector3(v.x, 2, v.y));
            Gizmos.DrawWireCube(pivot, new Vector3(v.x, 2, v.y));
        }
    }
}