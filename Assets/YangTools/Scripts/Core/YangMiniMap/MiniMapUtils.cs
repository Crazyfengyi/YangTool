using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YangTools.MiniMap
{
    public static class MiniMapUtils
    {
        /// <summary>
        /// 计算小地图位置
        /// </summary>
        /// <param name="viewPoint"></param>
        /// <param name="maxAnchor"></param>
        public static Vector3 CalculateMiniMapPosition(Vector3 viewPoint, RectTransform maxAnchor)
        {
            viewPoint = new Vector2((viewPoint.x * maxAnchor.sizeDelta.x) - (maxAnchor.sizeDelta.x * 0.5f),
                (viewPoint.y * maxAnchor.sizeDelta.y) - (maxAnchor.sizeDelta.y * 0.5f));

            return viewPoint;
        }

        public static MiniMapManager GetMiniMap(int id = 0)
        {
            MiniMapManager[] allmm = Object.FindObjectsOfType<MiniMapManager>();
            return allmm[id];
        }

        /// <summary>
        /// 输出照片的宽度(以像素为单位)(1 - 4096 px)
        /// </summary>
        public static readonly int resWidth = 2048;

        /// <summary>
        /// 输出照片的高度(以像素为单位)(1 - 4096 px)
        /// </summary>
        public static readonly int resHeight = 2048;

        /// <summary>
        /// 应用的MSAA，可能值为1、2、4和8
        /// </summary>
        public static int msaa = 1;

        private const string _folderPath = "/UGUIMiniMap/Content/Art/SnapShots/";
        public static string FolderPath => _folderPath;

        /// <summary>
        /// 照片名字
        /// </summary>
        private static string SnapshotName(int width, int height)
        {
            string levelName = SceneManager.GetActiveScene().name;
            //如果在编辑器中，我们必须通过编辑器获取名称
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                string[] path = SceneManager.GetActiveScene().name.Split(char.Parse("/"));
                string[] fileName = path[path.Length - 1].Split(char.Parse("."));
                levelName = fileName[0];
            }
#endif
            return string.Format(GetFullFolderPath() + "MapSnapshot_{0}_{1}x{2}_{3}.png", levelName, width, height, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        }

        /// <summary>
        /// 获取完整文件夹路径
        /// </summary>
        private static string GetFullFolderPath()
        {
            return Application.dataPath + _folderPath;
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        public static void TakeSnapshot(Camera camera)
        {
            //TODO fix
#if UNITY_EDITOR && !UNITY_WEBPLAYER
            //设置render texture
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            rt.antiAliasing = msaa;
            rt.filterMode = FilterMode.Trilinear;
            camera.targetTexture = rt;
            //渲染图片
            Texture2D snapshot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            snapshot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null;
            snapshot.alphaIsTransparency = true;
            byte[] bytes = snapshot.EncodeToPNG();
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(snapshot);
            //保存到文件
            if (!System.IO.Directory.Exists(GetFullFolderPath()))
            {
                Debug.LogError("File path: " + GetFullFolderPath() + " doesn't exist! Create it.");
                return;
            }
            string fileName = SnapshotName(resWidth, resHeight);
            System.IO.File.WriteAllBytes(fileName, bytes);
            Debug.Log($"Saved snapshot to: {fileName}");
            fileName = "";
            AssetDatabase.Refresh();
#endif
        }
    }
}