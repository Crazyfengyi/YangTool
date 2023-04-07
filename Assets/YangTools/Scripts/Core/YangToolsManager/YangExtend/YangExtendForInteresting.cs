#define INTERESTING
//#undef  INTERESTING  //测试时关闭

/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2022-12-16 
*/
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace YangTools.Extend
{
    /// <summary>
    /// 有趣的写法
    /// </summary>
    public partial class YangExtend
    {

#if INTERESTING
        //任何有GetAwaiter()方法的都可await,你可以这样写:await 2;
        public static TaskAwaiter GetAwaiter(this int argInt)
        {
            return Task.Delay(TimeSpan.FromSeconds(argInt)).GetAwaiter();
        }
        //任何有GetEnumerator()方法的都可foreach,你可以这样写:foreach(var item in 5){};
        public static IEnumerator GetEnumerator(this int argInt)
        {
            for (int i = 0; i < argInt; i++)
            {
                yield return i.ToString();
            }
        }
        //同上,你可以写:await foreach(var item in 5){}
        public static async IAsyncEnumerator<int> GetAsyncEnumerator(this int argInt)
        {
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(200);
                yield return i;
            }
        }
#endif

    }
}