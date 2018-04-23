using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReviseApplication.MyUtils
{
    public class UserUtils
    {

        public static string GetUserId()
        {
            try
            {
                HttpContext context = HttpContext.Current;
                var userId = (string)context.Session["user"];
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }
            }
            catch { }
            return null;
        }

        internal static bool IsLoggedIn()
        {
            string userId;
            using (var con = new ReviseDBEntities())
            {
                userId = GetUserId();
                var users = con.users.Where(u => u.userid == userId).ToList();
                if (users.First().isConnected == 1)
                    return true;
                return false;
            }
        }

        public static user GetUser()
        {
            using (var con = new ReviseDBEntities())
            {
                string userId = GetUserId();
                if(!string.IsNullOrEmpty(userId))
                    return (from x in con.users where x.userid == userId select x).ToList().FirstOrDefault();
                return new user();
            }
        }
    }
}