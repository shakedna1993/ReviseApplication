using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReviseApplication.Controllers
{
    /// <summary>
    /// controller for the main screen 
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// shows the main screen
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "User");
        }
    }
}