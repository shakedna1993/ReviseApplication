using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using ReviseApplication.Models;

namespace ReviseApplication.Controllers
{
    /// <summary>
    /// controller for all user involvment
    /// </summary>
    public class UserController : Controller
    {
        /// <summary>
        /// function that shows the main screen
        /// </summary>
        /// <returns>shows the screen</returns>
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult welcome()
        {
            return View();
        }

        /// <summary>
        /// function that shows the chat screen
        /// </summary>
        /// <param name="catid">category id</param>
        /// <param name="projid">project id</param>
        /// <returns>edirect to the chat screen</returns>
        public ActionResult Chat(int? catid, int? projid)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            if (catid == null || projid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            category cat = con.categories.SingleOrDefault(c => c.CatId == catid);
            project proj = con.projects.SingleOrDefault(p => p.ProjId == projid);
            //getting the specific project and category to open the chat for them
            int pid = proj.ProjId;
            int cid = cat.CatId;
            var prj = con.projUsers.Where(u => u.projid == proj.ProjId).ToList();

            if (cat == null)
            {
                return HttpNotFound();
            }

            //getting the project members
            List<user> memberslist = new List<user>();
            foreach (var usr in prj)
                memberslist.Add(con.users.SingleOrDefault(u => u.userid == usr.userid));
           
            ViewBag.memberslist = memberslist; // the list of members that participant in this project pass via ViewBag to the view.
            ViewBag.catid = catid;
            ViewBag.projid = projid;

            Session["Catid"] = catid;
            string userId = Session["userid"].ToString();
            UserScoreCalc(userId);
            Session["UserScore"] = con.users.Find(userId).score;
            var showView = new Categories()
            {
                category = cat.CatName,
                status = con.projCats.Find(pid, cid).status ?? 0
            };
     
            return View(showView);
        }

        /// <summary>
        /// function for score calculation
        /// </summary>
        /// <param name="prjid">the project id</param>
        /// <param name="usrid">the user id</param>
        /// <returns>returns the score of the user in the project</returns>
        public void UserScoreCalc(string usrid)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            int usrScore = 0;

            List<message> message = con.messages.ToList();
            int count = 0;

            foreach (var msg in message)
                if (msg.userid == usrid)
                    count++;
            usrScore = count;
            if (usrScore == 0)
                return ;
            con.users.Find(usrid).score = usrScore;
            con.SaveChanges();
        }
    }
}
