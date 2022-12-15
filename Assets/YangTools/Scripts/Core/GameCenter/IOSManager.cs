using UnityEngine;
using UnityEngine.SocialPlatforms;
//========================================
/*
 * ITunes Connect 里面设置排行榜单 和 成就
 * APP的GameCenter功能需要后台开启
 * 
*/
//=========================================

namespace YangTools
{
    /// <summary>
    /// 玩家信息
    /// </summary>
    public class GameCenterUserInfo
    {

    }

    /// <summary>
    /// GameCenter管理器
    /// </summary>
    public class IOSManager : MonoBehaviour
    {
        public const string RankingListID = "xxx";//排行榜ID
        public const string AchievementID = "xxx";//成就ID

        #region 变量
        /// <summary>
        /// GameCenter初始化状态
        /// </summary>
        public bool GameCenterState;
        /// <summary>
        /// 用户信息
        /// </summary>
        public GameCenterUserInfo userInfo;
        #endregion

        /// <summary>
        /// 初始化 GameCenter 登陆
        /// </summary>
        void Start()
        {
            Social.localUser.Authenticate(HandleAuthenticated);
        }

        /// <summary>
        /// 初始化GameCenter结果回调函数
        /// </summary>
        /// <param name="success">If set to <c>true</c> success.</param>
        private void HandleAuthenticated(bool success)
        {
            GameCenterState = success;
            Debug.Log("GameCenter: Success = " + success);
            ///初始化成功
            if (success)
            {
                //userInfo = "Username: " + Social.localUser.userName +
                //"\nUser ID: " + Social.localUser.id +
                //"\nIsUnderage: " + Social.localUser.underage;
                Debug.Log(userInfo);
            }
            else
            {
                ///初始化失败
            }
        }

        void OnGUI()
        {
            GUI.TextArea(new Rect(Screen.width - 200, 0, 200, 100), "GameCenter:" + GameCenterState);
            GUI.TextArea(new Rect(Screen.width - 200, 100, 200, 100), "userInfo:" + userInfo);

            if (GUI.Button(new Rect(0, 0, 110, 75), "打开成就"))
            {

                if (Social.localUser.authenticated)
                {
                    Social.ShowAchievementsUI();
                }
            }

            if (GUI.Button(new Rect(0, 150, 110, 75), "打开排行榜"))
            {

                if (Social.localUser.authenticated)
                {
                    Social.ShowLeaderboardUI();
                }
            }

            if (GUI.Button(new Rect(0, 300, 110, 75), "排行榜设置分数"))
            {

                if (Social.localUser.authenticated)
                {
                    Social.ReportScore(1000, "XXXX", HandleScoreReported);
                }
            }

            if (GUI.Button(new Rect(0, 300, 110, 75), "设置成就"))
            {

                if (Social.localUser.authenticated)
                {
                    Social.ReportProgress("XXXX", 15, HandleProgressReported);
                }
            }

        }

        //上传排行榜分数
        public void HandleScoreReported(bool success)
        {
            Debug.Log("*** HandleScoreReported: success = " + success);
        }

        //设置 成就
        private void HandleProgressReported(bool success)
        {
            Debug.Log("*** HandleProgressReported: success = " + success);
        }

        /// <summary>
        /// 加载好友回调
        /// </summary>
        /// <param name="success">If set to <c>true</c> success.</param>
        private void HandleFriendsLoaded(bool success)
        {
            Debug.Log("*** HandleFriendsLoaded: success = " + success);
            foreach (IUserProfile friend in Social.localUser.friends)
            {
                Debug.Log("* friend = " + friend.ToString());
            }
        }

        /// <summary>
        /// 加载成就回调
        /// </summary>
        /// <param name="achievements">Achievements.</param>
        private void HandleAchievementsLoaded(IAchievement[] achievements)
        {
            Debug.Log("* HandleAchievementsLoaded");
            foreach (IAchievement achievement in achievements)
            {
                Debug.Log("* achievement = " + achievement.ToString());
            }
        }

        /// <summary>
        /// 成就回调描述
        /// </summary>
        /// <param name="achievementDescriptions">Achievement descriptions.</param>
        private void HandleAchievementDescriptionsLoaded(IAchievementDescription[] achievementDescriptions)
        {
            Debug.Log("*** HandleAchievementDescriptionsLoaded");
            foreach (IAchievementDescription achievementDescription in achievementDescriptions)
            {
                Debug.Log("* achievementDescription = " + achievementDescription.ToString());
            }
        }

    }
}

