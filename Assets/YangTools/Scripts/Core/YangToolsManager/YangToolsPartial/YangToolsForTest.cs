using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace YangTools
{
    public static partial class YangToolsManager
    {
        static int collectionCount = 0;
        static Stopwatch watch = null;
        /// <summary>
        /// 给定测试循环次数
        /// </summary>
        public static int testCount = 10000000;

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
        /// <param name="o"></param>
        /// <returns></returns>
        public static string getMemory(object o)
        {
            GCHandle h = GCHandle.Alloc(o, GCHandleType.Pinned);
            IntPtr addr = h.AddrOfPinnedObject();
            return "0x" + addr.ToString("X");
        }
    }
}
