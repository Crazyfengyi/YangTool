using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
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
           return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
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