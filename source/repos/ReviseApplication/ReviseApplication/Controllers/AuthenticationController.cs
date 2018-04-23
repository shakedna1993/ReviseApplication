using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ReviseApplication.MyUtils;

namespace ReviseApplication.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Registration()
        {
            return View();
        }
        public ActionResult RenewPass()
        {
            return View();
        }

        [HttpPost]
    [AllowAnonymous]
    public ActionResult Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return Json(new { success = "Failed", error = "One or more field is empty" });
                }
                using (var con = new ReviseDBEntities())
                {
                    var usr = username;
                    var users = con.users.Where(u => u.UserName == usr).ToList();
                    if (users.First().isConnected == 1)
                    {
                        return Json(new { success = false, message = "User is alredy logged in" });
                    }

                    if (users.First().password == password)
                    {
                        var user = users.First();
                        users.First().isConnected = 1;
                        con.SaveChanges();
                        System.Web.HttpContext.Current.Session["username"] = user.UserName;
                        System.Web.HttpContext.Current.Session.Timeout = 30;
                        return Json(new
                        {
                            success = true,
                            message = "Connected! Loading your Projects, please wait.",
                            Yourname = user.fname + " " + user.lname
                        });
                    }
                    else
                    {
                        return Json(new { success = false, message = "User not found or details are mismatched" });
                    }
                }
            }
            catch (FormatException)
            {
                return Json(new { success = false, message = "UserName is incorrect!" });
            }
            catch
            {
                return Json(new { success = false, message = "Unknown error occurred!" });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Registration(string firstname, string lastname, string UserId, string username, string phonenum, string EmailID, string password, Nullable<System.DateTime> DateOfBirth)
        {
            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(EmailID) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(phonenum))
            {
                return Json(new { success = false, message = "One or more fields is missing, Please fill all blank fields" });
            }

            #region Save to Database
            try
            {
                using (var con = new ReviseDBEntities())
                {
                    var isUserExists = con.users.Where(u => u.UserName == username || u.Email == EmailID || u.userid == UserId).Any();

                    if (!isUserExists)
                    {
                        var user = new user
                        {
                            Email = EmailID,
                            fname = firstname,
                            lname = lastname,
                            password = password,
                            userid = UserId,
                            UserName = username,
                            birthday = DateOfBirth,
                            PhoneNum = phonenum
                        };
                        con.users.Add(user);
                        con.SaveChanges();
            #endregion 
                        return RedirectToAction("Login", "Authentication");
                    }
                    return Json(new { success = false, message = "User already exists!" });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Unknown error occurred!" });
            }
        }

        [HttpGet]
        public JsonResult GetUserInfo()
        {
            var user = UserUtils.GetUser();
            if (!UserUtils.IsLoggedIn())
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                fullname = string.Format("{0} {1}", user.fname, user.lname)
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            try
            {
                HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

                System.Web.HttpContext.Current.Session.Clear();
                System.Web.HttpContext.Current.Session.RemoveAll();
                System.Web.HttpContext.Current.Session.Abandon();

                return RedirectToAction("Login", "Authentication");
            }
            catch
            {
                throw;
            }
        }
    }
}
