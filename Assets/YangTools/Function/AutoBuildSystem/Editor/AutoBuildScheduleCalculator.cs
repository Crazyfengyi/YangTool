#if UNITY_EDITOR
using System;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 自动构建本地时间计划计算器
    /// </summary>
    public static class AutoBuildScheduleCalculator
    {
        /// <summary>
        /// 允许 Editor 更新循环的最大调度延迟，超过后视为错过计划
        /// </summary>
        public const int MaxScheduleLatenessSeconds = 5;

        /// <summary>
        /// 计算严格晚于当前时间的下一次触发时间
        /// </summary>
        public static DateTime GetNextRun(DateTime now, AutoBuildScheduleSettings schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            int hour = Math.Max(0, Math.Min(23, schedule.hour));
            int minute = Math.Max(0, Math.Min(59, schedule.minute));
            DateTime candidate = now.Date.AddHours(hour).AddMinutes(minute);

            if (schedule.mode == AutoBuildScheduleMode.Weekly)
            {
                int daysUntil = ((int)schedule.dayOfWeek - (int)now.DayOfWeek + 7) % 7;
                candidate = candidate.AddDays(daysUntil);
                if (candidate <= now)
                {
                    candidate = candidate.AddDays(7);
                }

                return candidate;
            }

            return candidate <= now ? candidate.AddDays(1) : candidate;
        }

        /// <summary>
        /// 判断定时检查是否错过了计划时刻
        /// </summary>
        public static bool IsMissed(DateTime scheduledTime, DateTime now)
        {
            return now - scheduledTime > TimeSpan.FromSeconds(MaxScheduleLatenessSeconds);
        }
    }
}
#endif
