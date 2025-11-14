using SalesManagementSystem.DAL;
using System;
using System.Data;

namespace SalesManagementSystem.BLL
{
    public class AuthenticationManager
    {
        public static User Login(string username, string password)
        {
            DataTable dt = UserRepository.AuthenticateUser(username, password);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                User user = new User
                {
                    UserID = Convert.ToInt32(row["UserID"]),
                    Username = row["Username"].ToString(),
                    FullName = row["FullName"].ToString(),
                    Email = row["Email"].ToString(),
                    RoleID = Convert.ToInt32(row["RoleID"]),
                    RoleName = row["RoleName"].ToString()
                };

                UserRepository.UpdateLastLogin(user.UserID);
                Session.CurrentUser = user;

                return user;
            }

            return null;
        }

        public static void Logout()
        {
            Session.CurrentUser = null;
        }

        public static bool HasPermission(string module, string action)
        {
            if (Session.CurrentUser == null) return false;
            return UserRepository.CheckPermission(Session.CurrentUser.RoleID, module, action);
        }
    }
}
