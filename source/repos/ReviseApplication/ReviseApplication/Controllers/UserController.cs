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
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult welcome()
        {
            return View();
        }

        public ActionResult Chat(int? catid, int? projid)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            if (catid == null || projid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            category cat = con.categories.SingleOrDefault(c => c.CatId == catid);
            project proj = con.projects.SingleOrDefault(p => p.ProjId == projid);
            int pid = proj.ProjId;
            int cid = cat.CatId;
            var prj = con.projUsers.Where(u => u.projid == proj.ProjId).ToList();

            if (cat == null)
            {
                return HttpNotFound();
            }

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
