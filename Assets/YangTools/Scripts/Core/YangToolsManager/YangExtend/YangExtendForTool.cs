using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using YangTools.Log;

namespace YangTools.Extend
{
    public partial class YangExtend
    {
        /// <summary>
        /// 当天结束时间(24点)
        /// </summary>
        public static DateTime GetDayLastTime()
        {
            return Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy-MM-dd 23:59:59"));
        }

        /// <summary>
        /// 获得给定时间在所在年里是第几周
        /// </summary>
        public static int GetWeekOfYear(DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }

        //测试值占内存大小
        /* * 1字节(byte)=8位(bit) * 1024字节=1K * 1024k=1M */ //如果为空也要占用1字节,不为空则为实际数据大小
        public struct TestStruct
        {
            //public Int32 a;//4字节
            //public Int64 a;//8字节
            //public long a;//8字节
            //public Person a;//24字节 //里面有3个string
            //public string FirstName;//8字节  1汉字=2字节 但这里是算的引用地址的内存大小
            //public TestStruct2 a;//1字节 //里面为空
            //public TestStruct2 a;//4字节 //里面有int32
            public void Test()
            {
                TestStruct testStruct = new TestStruct();
                Debug.LogError($"测试大小:{Marshal.SizeOf(testStruct)}");
            }
        }

        /*
         *权重取值
            var max = list.Sum(y => y.Weight);
            var rand = rnd.Next(max);
            var res = list.FirstOrDefault(x => rand >= (max -= x.Weight));
         *
         */
    }
}