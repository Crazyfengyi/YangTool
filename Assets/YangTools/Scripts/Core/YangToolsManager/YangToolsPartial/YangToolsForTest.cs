using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace YangTools
{
    public static partial class YangToolsManager
    {
        static int collectionCount = 0;
        static Stopwatch watch = null;
        public static int testCount = 1000000;//测试循环次数

        /// <summary>
        /// 测试开始
        /// </summary>
        public static void TestBegin()
        {
            GC.Collect(); ////强制对所有代码进行即时垃圾回收
            GC.WaitForPendingFinalizers();////挂起线程，执行终结器队列中的终结器（即析构方法）
            GC.Collect();///再次对所有代码进行垃圾回收，主要包括从终结器队列中出来的对象
            collectionCount = GC.CollectionCount(0);///返回在0代中执行的垃圾回收次数
            watch = new Stopwatch();
            watch.Start();
        }
        /// <summary>
        /// 测试结束
        /// </summary>
        public static void TestEnd()
        {
            watch.Stop();
            Console.WriteLine("耗时：{0}", watch.ElapsedMilliseconds.ToString());
            Console.WriteLine("垃圾回收次数：{0}", GC.CollectionCount(0) - collectionCount);
        }
        /// <summary>
        /// 获取引用类型的内存地址方法
        /// </summary>
        public static string getMemory(object o)
        {
            GCHandle h = GCHandle.Alloc(o, GCHandleType.Pinned);
            IntPtr addr = h.AddrOfPinnedObject();
            return "0x" + addr.ToString("X");
        }
        /// <summary>
        /// Debug位置
        /// </summary>
        public static void DebugPonit()
        {
            string logStart = "--DEBUG START--";
            //文件名
            string FileName = "FileName:" + System.IO.Path.GetFileName(new System.Diagnostics.StackTrace(1, true).GetFrame(0).GetFileName());
            //文件行号
            string LineNumber = "Line:" + new System.Diagnostics.StackTrace(1, true).GetFrame(0).GetFileLineNumber().ToString();
            //方法名
            string callStack = "CallStack:" + new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();

            string logEnd = "--END--";

            string OutputResult = $"{logStart}\n{callStack} \n{FileName}\n{LineNumber}\n{logEnd}";
            UnityEngine.Debug.Log(OutputResult);
        }
    }
}
