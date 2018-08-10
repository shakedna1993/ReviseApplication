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
using ReviseApplication.Repository;

namespace ReviseApplication.Controllers
{
    /// <summary>
    /// The Authentication Controller Contains all logic that belongs to the issues checking the user's privileges identity.
    /// </summary>
    public class AuthenticationController : Controller
    {
        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // GET: Registration
        public ActionResult Registration()
        {
            return View();
        }

        // GET: RenewPass
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


        // GET: PersonalFile
        [HttpGet]
        public ActionResult PersonalFile()
        {
            string user = Session["userid"].ToString();
            var repo = new MainRepository();
            var Main = repo.PersonalFileView(user);

            return View(Main);
        }

        /// <summary>
        /// The function logges the user in to the web and check the detials that he provided in the Log-in screen.
        /// </summary>
        /// <param name="username">contains the user name that the user provided on the screen</param>
        /// <param name="password">contains the password that the user provided on the screen</param>
        /// <returns>Redirect to the right page if the details for the web site correct, if not, return to the log-in screen again with an error.</returns>
        [HttpPost]
    [AllowAnonymous]
    public ActionResult Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) //if the user didn't fill in the details
                {
                    TempData["EmptyLogin"] = "One or more field is empty";
                    return RedirectToAction("Login", "Authentication");
                }
                using (var con = new ReviseDBEntities())
                {
                    var usr = username;
                    var users = con.users.Where(u => u.UserName == usr).ToList();
                    var user = users.First();
                    if (users.First().isConnected == 1) //checks if the user is already connected
                    {
                        TempData["UserLogin"] = "User is alredy logged in";
                        return RedirectToAction("Login", "Authentication");
                    }

                    if (users.First().password == password)  //checks the password matches the regestration password
                    {
                        users.First().isConnected = 1;      //marking the user as "connected"
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

        /// <summary>
        /// function that change the user regestration information
        /// </summary>
        /// <param name="firstname">cotains the provided first name</param>
        /// <param name="lastname">cotains the provided last name</param>
        /// <param name="UserId">cotains the user id</param>
        /// <param name="username">cotains the provided user name</param>
        /// <param name="phonenum">cotains the provided phone number</param>
        /// <param name="EmailID">cotains the provided email</param>
        /// <param name="pic">cotains the provided picture</param>
        /// <returns>redirect to the main page if everything is correct, if not redirect to the personal file page again</returns>

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PersonalFile(string firstname, string lastname, string UserId, string username, string phonenum, string EmailID, string pic)
        {
            ReviseDBEntities con = new ReviseDBEntities();   //connection to the DB
            string userInfo = Session["userid"].ToString();
            var isUserExists = false;
            if (firstname == con.users.Find(userInfo).fname && lastname == con.users.Find(userInfo).lname && username == con.users.Find(userInfo).UserName &&
                EmailID == con.users.Find(userInfo).Email && phonenum == con.users.Find(userInfo).PhoneNum && string.IsNullOrEmpty(pic))  //checks if the information equal to the one in the DB
            {
                TempData["NoInfoChanges"] = "No Changes where made";
                return RedirectToAction("PersonalFile", "Authentication");
            }

            if (string.IsNullOrEmpty(pic) && string.IsNullOrEmpty(con.users.Find(userInfo).pic))    //if no picture provided now and thers no picture in the DB, assign the default
                pic = "https://he.gravatar.com/userimage/138919762/622efbcbeb0e8cea9b64cf6e8bffffc0.jpg";

            #region Save to Database
            try
            {
                using (var conn = new ReviseDBEntities())
                {
                    foreach (var usr in conn.users)
                        if (usr.userid != userInfo)
                            if (usr.UserName == username || usr.Email == EmailID)
                                isUserExists = true;

                    if (!isUserExists)   //updating the user information
                    {
                        conn.users.Find(userInfo).Email = EmailID;
                        conn.users.Find(userInfo).fname = firstname;
                        conn.users.Find(userInfo).lname = lastname;
                        conn.users.Find(userInfo).UserName = username;
                        conn.users.Find(userInfo).PhoneNum = phonenum;
                        if (!string.IsNullOrEmpty(pic))
                            conn.users.Find(userInfo).pic = pic;
                        conn.SaveChanges();
                        #endregion
                        return RedirectToAction("ProjectMain", "Project");
                    }
                    TempData["UserExist"] = "User with this user name or email is already exists";
                    return RedirectToAction("PersonalFile", "Authentication");
                }
            }
            catch
            {
                TempData["Unknown"] = "Unknown error occurred!";
                return RedirectToAction("PersonalFile", "Authentication");
            }
        }

        /// <summary>
        /// function that register the user in order to use the application
        /// </summary>
        /// <param name="firstname">cotains the provided first name</param>
        /// <param name="lastname">cotains the provided last name</param>
        /// <param name="UserId">cotains the provided id</param>
        /// <param name="username">cotains the provided user name</param>
        /// <param name="phonenum">cotains the provided phone</param>
        /// <param name="EmailID">cotains the provided email</param>
        /// <param name="password">cotains the provided password</param>
        /// <param name="pic">cotains the provided picture</param>
        /// <param name="DateOfBirth">cotains the provided date of birth</param>
        /// <returns>if all information is correct redirect to the main screen, if not redirect to the registraion again with error</returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Registration(string firstname, string lastname, string UserId, string username, string phonenum, string EmailID, string password, string pic, Nullable<System.DateTime> DateOfBirth)
        {
            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(EmailID) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(phonenum)) //checks if one of the filldes is empty
            {
                TempData["EmptyFildes"] = "One or more fields is missing, Please fill all blank fields";
                return RedirectToAction("Registration", "Authentication");
            }

            if (string.IsNullOrEmpty(pic))    //if no picture provided, assign the default
                pic = "https://he.gravatar.com/userimage/138919762/622efbcbeb0e8cea9b64cf6e8bffffc0.jpg";

            #region Save to Database
            try
            {
                using (var con = new ReviseDBEntities())
                {
                    var isUserExists = con.users.Where(u => u.UserName == username || u.Email == EmailID || u.userid == UserId).Any();

                    if (!isUserExists)  //creat new user
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
                        con.users.Add(user);  //save the user in the DB
                        con.SaveChanges();
            #endregion 
                        return RedirectToAction("Login", "Authentication");
                    }
                    TempData["Exists"] = "User already exists!";
                    return RedirectToAction("Registration", "Authentication");
                }
            }
            catch
            {
                TempData["Unknown"] = "Unknown error occurred!!";
                return RedirectToAction("Registration", "Authentication");
            }
        }

       /// <summary>
       /// logs out the connected user
       /// </summary>
       /// <returns>redirect to the log-in screen</returns>
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


        /// <summary>
        /// this function sends verification email for resrt password
        /// </summary>
        /// <param name="emailID">the user email</param>
        /// <param name="activationCode">code for the activation</param>
        /// <param name="emailFor"> the resone the email is send</param>
        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/Authentication/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("reviseproj@gmail.com", "Revise Administrator");   //details about the sender and reciver
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "Aa12345!";

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")   //the message that will be sent 
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

        /// <summary>
        /// function that encrypt the password
        /// </summary>
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

        /// <summary>
        ///Verify Email ID, Generate Reset password link ,Send Email 
        /// </summary>
        /// <param name="EmailID">the user's email</param>
        /// <returns>showing the page</returns>
        [HttpPost]
        public ActionResult RenewPass(string EmailID)
        {

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

        /// <summary>
        /// Verify the reset password link
        /// Find account associated with this link
        /// redirect to reset password page
        /// </summary>
        /// <param name="id">the reset code</param>
        /// <returns></returns>
        public ActionResult ResetPassword(string id)
        {
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

        /// <summary>
        /// function that updating the user's password after reset
        /// </summary>
        /// <param name="model"> contains the new password</param>
        /// <returns></returns>
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
