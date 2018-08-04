using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using ReviseApplication.Models;
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

    [HttpPost]
    [AllowAnonymous]
    public ActionResult Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    TempData["EmptyLogin"] = "One or more field is empty";
                    return RedirectToAction("Login", "Authentication");
                }
                using (var con = new ReviseDBEntities())
                {
                    var usr = username;
                    var users = con.users.Where(u => u.UserName == usr).ToList();
                    var user = users.First();
                    if (users.First().isConnected == 1)
                    {
                        TempData["UserLogin"] = "User is alredy logged in";
                        return RedirectToAction("Login", "Authentication");
                    }

                    if (users.First().password == password)
                    {
                        users.First().isConnected = 1;
                        con.SaveChanges();
                        System.Web.HttpContext.Current.Session["username"] = user.UserName;
                        Session["userid"] = user.userid;
                        Session["IsAdmin"] = user.IsAdmin;
                        if(user.score != null)
                            Session["UserScore"] = user.score;
                        else
                            Session["UserScore"] = 0;
                        System.Web.HttpContext.Current.Session.Timeout = 30;
                        return RedirectToAction("ProjectMain", "Project");
                    }
                    else
                    {
                        TempData["FailedLogin"] = "User not found or details are mismatched";
                        return RedirectToAction("Login", "Authentication");
                    }
                }
            }
            catch (FormatException)
            {
                TempData["UserName"] = "UserName is incorrect!";
                return RedirectToAction("Login", "Authentication");
            }
            catch
            {
                TempData["Unknown"] = "Unknown error occurred!";
                return RedirectToAction("Login", "Authentication");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Registration(string firstname, string lastname, string UserId, string username, string phonenum, string EmailID, string password, string pic, Nullable<System.DateTime> DateOfBirth)
        {
            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(EmailID) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(phonenum))
            {
                return Json(new { success = false, message = "One or more fields is missing, Please fill all blank fields" });
            }

            if (string.IsNullOrEmpty(pic))
                pic = "https://he.gravatar.com/userimage/138919762/622efbcbeb0e8cea9b64cf6e8bffffc0.jpg";

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
                            PhoneNum = phonenum,
                            pic = pic
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

       
        public ActionResult Logout()
        {
            try
            {
                ReviseDBEntities con = new ReviseDBEntities();
                con.users.Find(Session["userid"].ToString()).isConnected = 0;
                con.SaveChanges();
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Authentication");
            }
            catch
            {
                throw;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/Authentication/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("reviseproj@gmail.com", "Revise Administrator");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "Aa12345!";

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "Your account is successfully created!";
                body = "<br/><br/>We are excited to tell you that your Revise Application account is" +
                    " successfully created. Please click on the below link to verify your account" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password-REVISE application";
                body = "Hello,<br/><br/>We got request for reset your account password. Please click on the below link to reset your password" +
                    "<br/><br/><a href=" + link + ">Reset Password link</a>";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        public static class Crypto
        {
            public static string Hash(string value)
            {
                return Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.Create()
                    .ComputeHash(Encoding.UTF8.GetBytes(value))
                    );
            }
        }

        [HttpPost]
        public ActionResult RenewPass(string EmailID)
        {
            //Verify Email ID
            //Generate Reset password link 
            //Send Email 
            string message = "";

            using (var con = new ReviseDBEntities())
            {
                var account = con.users.Where(a => a.Email == EmailID).FirstOrDefault();
                if (account != null)
                {
                    //Send email for reset password
                    string resetCode = Guid.NewGuid().ToString();
                    SendVerificationLinkEmail(account.Email, resetCode, "ResetPassword");
                    account.ResetPasswordCode = resetCode;
                    //This line I have added here to avoid confirm password not match issue , as we had added a confirm password property 
                    //in our model class in part 1
                    con.Configuration.ValidateOnSaveEnabled = false;
                    con.SaveChanges();
                    message = "Reset password link has been sent to your email id.";
                }
                else
                {
                    message = "Account not found";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        public ActionResult ResetPassword(string id)
        {
            //Verify the reset password link
            //Find account associated with this link
            //redirect to reset password page
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            using (var con = new ReviseDBEntities())
            {
                var user = con.users.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (var con = new ReviseDBEntities())
                {
                    var user = con.users.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        // user.password = Crypto.Hash(model.NewPassword);
                        user.password = model.NewPassword;
                        user.ResetPasswordCode = "";
                        con.Configuration.ValidateOnSaveEnabled = false;
                        con.SaveChanges();
                        message = "New password updated successfully";
                    }
                }
            }
            else
            {
                message = "Something invalid";
            }
            ViewBag.Message = message;
            return View(model);
        }
    }

}
