using SalesManagementSystem.DAL;
using System;

namespace SalesManagementSystem.BLL
{
    public class Session
    {
        public static User CurrentUser { get; set; }
        public static DateTime LoginTime { get; set; }
        public static bool IsSessionValid()
        {
            if (CurrentUser == null) return false;

            int timeout = 30; // minutes
            string timeoutSetting = GetSetting("SessionTimeout");
            if (!string.IsNullOrEmpty(timeoutSetting))
                int.TryParse(timeoutSetting, out timeout);

            return (DateTime.Now - LoginTime).TotalMinutes < timeout;
        }

        private static string GetSetting(string key)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT SettingValue FROM SystemSettings WHERE SettingKey = @Key",
                new[] { new System.Data.SqlClient.SqlParameter("@Key", key) });

            return result?.ToString() ?? "";
        }
    }
}
