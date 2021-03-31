﻿using UnityEngine;

namespace WTB.SDK.Game
{
    /// <summary>
    /// UserPlatform helper
    /// </summary>
    internal static class UserPlatform
    {
        /// <summary>
        /// User ID cache
        /// </summary>
        private static string m_UserID = null;
        /// <summary>
        /// User name cache
        /// </summary>
        private static string m_UserName = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get User ID
        /// </summary>
        /// <returns></returns>
        internal static string GetUserID()
        {
            if (m_UserID != null)
                return m_UserID;

            FetchPlatformInfos();

            return m_UserID;
        }
        /// <summary>
        /// Get User ID
        /// </summary>
        /// <returns></returns>
        internal static string GetUserName()
        {
            if (m_UserName != null)
                return m_UserName;

            FetchPlatformInfos();

            return m_UserName;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Find platform informations
        /// </summary>
        private static void FetchPlatformInfos()
        {
            try
            {
                var l_PlatformLeaderboardsModels    = Resources.FindObjectsOfTypeAll<PlatformLeaderboardsModel>();
                var l_FieldAccessor                 = typeof(PlatformLeaderboardsModel).GetField("_platformUserModel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                foreach (var l_Current in l_PlatformLeaderboardsModels)
                {
                    var l_PlatformUserModel = l_FieldAccessor.GetValue(l_Current) as IPlatformUserModel;
                    if (l_PlatformUserModel == null)
                        continue;

                    var l_Task = l_PlatformUserModel.GetUserInfo();
                    l_Task.Wait();

                    var l_PlayerID = l_Task.Result.platformUserId;
                    if (!string.IsNullOrEmpty(l_PlayerID))
                    {
                        m_UserID    = l_PlayerID;
                        m_UserName  = l_Task.Result.userName;
                        return;
                    }
                }

                var l_BSUtilsTask = BS_Utils.Gameplay.GetUserInfo.GetUserAsync();
                l_BSUtilsTask.Wait();

                m_UserID   = l_BSUtilsTask.Result.platformUserId;
                m_UserName = l_BSUtilsTask.Result.userName;
            }
            catch (System.Exception l_Exception)
            {
                Logger.log?.Error("[SDK.Game][UserPlatform] Unable to find user platform informations");
                Logger.log?.Error(l_Exception);
            }
        }
    }
}
