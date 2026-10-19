#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace YangTools.AutoBuildSystem.Tests
{
    /// <summary>
    /// 自动构建配置和计划逻辑编辑器测试
    /// </summary>
    public sealed class AutoBuildSystemTests
    {
        /// <summary>
        /// 每日计划在当前时刻之后严格触发
        /// </summary>
        [Test]
        public void DailyScheduleAtBoundaryMovesToNextDay()
        {
            AutoBuildScheduleSettings schedule = new AutoBuildScheduleSettings
            {
                mode = AutoBuildScheduleMode.Daily,
                hour = 2,
                minute = 30
            };
            DateTime now = new DateTime(2026, 10, 16, 2, 30, 0, DateTimeKind.Local);

            DateTime next = AutoBuildScheduleCalculator.GetNextRun(now, schedule);

            Assert.AreEqual(new DateTime(2026, 10, 17, 2, 30, 0, DateTimeKind.Local), next);
        }

        /// <summary>
        /// 每周计划在当天时间已过时滚动到下周
        /// </summary>
        [Test]
        public void WeeklyScheduleAtBoundaryMovesToNextWeek()
        {
            AutoBuildScheduleSettings schedule = new AutoBuildScheduleSettings
            {
                mode = AutoBuildScheduleMode.Weekly,
                dayOfWeek = DayOfWeek.Friday,
                hour = 9,
                minute = 0
            };
            DateTime now = new DateTime(2026, 10, 16, 9, 1, 0, DateTimeKind.Local);

            DateTime next = AutoBuildScheduleCalculator.GetNextRun(now, schedule);

            Assert.AreEqual(new DateTime(2026, 10, 23, 9, 0, 0, DateTimeKind.Local), next);
        }

        /// <summary>
        /// 过期计划会被判定为错过而不会补跑
        /// </summary>
        [Test]
        public void MissedScheduleIsNotDueForCatchUp()
        {
            DateTime scheduled = new DateTime(2026, 10, 15, 2, 0, 0, DateTimeKind.Local);
            DateTime now = new DateTime(2026, 10, 16, 3, 0, 0, DateTimeKind.Local);

            Assert.IsTrue(AutoBuildScheduleCalculator.IsMissed(scheduled, now));
        }

        /// <summary>
        /// Editor 在允许延迟范围内检查时仍可正常触发计划
        /// </summary>
        [Test]
        public void ScheduleWithinEditorTickGracePeriodIsNotMissed()
        {
            DateTime scheduled = new DateTime(2026, 10, 16, 9, 0, 0, DateTimeKind.Local);
            DateTime now = scheduled.AddSeconds(AutoBuildScheduleCalculator.MaxScheduleLatenessSeconds);

            Assert.IsFalse(AutoBuildScheduleCalculator.IsMissed(scheduled, now));
        }

        /// <summary>
        /// 超过 Editor 调度容差的计划不会在恢复后补跑
        /// </summary>
        [Test]
        public void ScheduleBeyondEditorTickGracePeriodIsMissed()
        {
            DateTime scheduled = new DateTime(2026, 10, 16, 9, 0, 0, DateTimeKind.Local);
            DateTime now = scheduled.AddSeconds(AutoBuildScheduleCalculator.MaxScheduleLatenessSeconds + 1);

            Assert.IsTrue(AutoBuildScheduleCalculator.IsMissed(scheduled, now));
        }

        /// <summary>
        /// 合法的 Android 档案可通过基础配置校验
        /// </summary>
        [Test]
        public void ValidAndroidProfileHasNoConfigurationErrors()
        {
            AutoBuildProfile profile = CreateAndroidProfile();
            try
            {
                List<string> errors = AutoBuildProfileValidator.Validate(profile);

                Assert.IsEmpty(errors);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        /// <summary>
        /// 缺失版本和包标识时校验会返回明确错误
        /// </summary>
        [Test]
        public void InvalidProfileReportsVersionAndIdentifierErrors()
        {
            AutoBuildProfile profile = CreateAndroidProfile();
            profile.version = "release";
            profile.GetPlatform(AutoBuildPlatform.Android).applicationIdentifier = "invalid";
            try
            {
                List<string> errors = AutoBuildProfileValidator.Validate(profile);

                Assert.IsTrue(errors.Any(error => error.Contains("版本号")));
                Assert.IsTrue(errors.Any(error => error.Contains("包标识")));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        /// <summary>
        /// 失败状态可以写入和重新读取构建历史
        /// </summary>
        [Test]
        public void FailedBuildRecordPersistsFailureStatus()
        {
            string path = Path.Combine(Path.GetTempPath(), "autobuild-test-" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                AutoBuildHistoryStore.AddToPath(path, new AutoBuildBuildRecord
                {
                    runId = "failed-test",
                    profileName = "test",
                    status = AutoBuildResultStatus.Failed.ToString(),
                    message = "expected failure",
                    durationSeconds = 3.5
                });

                List<AutoBuildBuildRecord> records = AutoBuildHistoryStore.LoadFromPath(path);
                Assert.AreEqual(1, records.Count);
                Assert.AreEqual(AutoBuildResultStatus.Failed.ToString(), records[0].status);
                Assert.AreEqual("expected failure", records[0].message);
                Assert.AreEqual(3.5, records[0].durationSeconds);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
                if (File.Exists(path + ".tmp")) File.Delete(path + ".tmp");
            }
        }

        /// <summary>
        /// 创建不依赖外部平台资源的 Android 测试档案
        /// </summary>
        private static AutoBuildProfile CreateAndroidProfile()
        {
            AutoBuildProfile profile = ScriptableObject.CreateInstance<AutoBuildProfile>();
            profile.profileId = "test-profile";
            profile.profileName = "Test";
            profile.productName = "TestGame";
            profile.companyName = "YangTools";
            profile.version = "1.0.0";
            profile.buildNumberCode = 1;
            profile.outputRoot = "Builds/Test";
            profile.platforms = new List<AutoBuildPlatformSettings>
            {
                new AutoBuildPlatformSettings
                {
                    platform = AutoBuildPlatform.Android,
                    enabled = true,
                    applicationIdentifier = "com.yangtools.test"
                }
            };
            profile.bundles.enabled = false;
            profile.email.enabled = false;
            return profile;
        }
    }
}
#endif
