using System;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class BackupService
    {
        public static bool CreateBackup()
        {
            try
            {
                string backupPath = GetSetting("BackupPath");
                string dbName = "SalesManagementDB";
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFile = System.IO.Path.Combine(backupPath, $"{dbName}_{timestamp}.bak");

                string query = $"BACKUP DATABASE [{dbName}] TO DISK = '{backupFile}' WITH FORMAT";

                DatabaseHelper.ExecuteNonQuery(query);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Backup error: {ex.Message}");
                return false;
            }
        }

        public static bool RestoreBackup(string backupFilePath)
        {
            try
            {
                string dbName = "SalesManagementDB";

                // Close all connections
                string query = $@"
                    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [{dbName}] FROM DISK = '{backupFilePath}' WITH REPLACE;
                    ALTER DATABASE [{dbName}] SET MULTI_USER;";

                DatabaseHelper.ExecuteNonQuery(query);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetSetting(string key)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT SettingValue FROM SystemSettings WHERE SettingKey = @Key",
                new[] { new SqlParameter("@Key", key) });

            return result?.ToString() ?? "";
        }
    }
}
