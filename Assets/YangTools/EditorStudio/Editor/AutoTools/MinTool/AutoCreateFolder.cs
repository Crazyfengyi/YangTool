
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using YangTools.Scripts.Core;

namespace YangTools
{
    public class AutoCreateFolder
    {
        public static string path = Application.dataPath + "/";//路径
        public static string resPath = path + "/YangTools/Resources/";//resources路径
        /// <summary>
        /// 自动生成工具文件夹
        /// </summary>
        [MenuItem(SettingInfo.YongToolsFunctionPath + "生成工具类文件夹")]
        public static void AutoCreate()
        {
            //对象池物体放在这里面
            if (!Directory.Exists($"{resPath}Prefabs"))
            {
                Directory.CreateDirectory($"{resPath}Prefabs");
                CreateExplainTxt($"{resPath}Prefabs", "YangTool对象池物体放在这里面");
            }
            //背景音乐物体放在这里面
            if (!Directory.Exists($"{resPath}BGMusic"))
            {
                Directory.CreateDirectory($"{resPath}BGMusic");
                CreateExplainTxt($"{resPath}BGMusic", "背景音乐物体放在这里面");
            }
            //音效物体放在这里面
            if (!Directory.Exists($"{resPath}SoundMusic"))
            {
                Directory.CreateDirectory($"{resPath}SoundMusic");
                CreateExplainTxt($"{resPath}SoundMusic", "音效物体放在这里面");
            }

            //刷新文件夹显示
            AssetDatabase.Refresh();
            Debug.Log("生成工具类文件夹完成");
        }
        /// <summary>
        /// 创建说明文本
        /// </summary>
        public static void CreateExplainTxt(string path, string explain)
        {
            //判断目标文件夹中是否含有文件
            FileInfo newFileInfo = new FileInfo($"{path}/说明.txt");
            if (!newFileInfo.Exists)
            {
                //创建文件
                newFileInfo.Create().Close();
                //创建StreamWriter 类的实例
                StreamWriter streamWriter = new StreamWriter($"{path}/说明.txt");
                //向文件中写入姓名
                streamWriter.WriteLine(explain);
                //刷新缓存
                streamWriter.Flush();
                //关闭流
                streamWriter.Close();
            }
        }
    }
}
#endif