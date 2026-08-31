using System.Collections.Generic;

namespace SignInSystem
{
    /// <summary>
    /// 签到项数据结构
    /// </summary>
    public sealed class ItemData_SevenSign
    {
        public int DayId { get; }
        public string DayString => $"第{DayId}天";
        public SignInDayState State => SignInManager.Instance.GetState(DayId - 1);
        public List<SignInReward> AwardListList { get; }

        /// <summary>
        /// 创建签到项
        /// </summary>
        public ItemData_SevenSign(int dayId, List<SignInReward> rewards)
        {
            DayId = dayId;
            AwardListList = rewards;
        }
    }
}
