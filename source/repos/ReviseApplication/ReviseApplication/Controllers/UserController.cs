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

            var showView = new Categories()
            {
                category = cat.CatName,
                //status = 
            };

            return View(showView);
        }

        [HttpPost]
        public ActionResult Chat(string reqname, string reqdesc, int reqvote)
        {
            int projId = Convert.ToInt32(Session["projectid"]);
            int catId = Convert.ToInt32(Session["Catid"]);

            try
            {
                if(string.IsNullOrEmpty(reqname) || string.IsNullOrEmpty(reqdesc) && reqvote == 0)
                {
                    ModelState.AddModelError("", "No changes made");
                    return RedirectToAction("CategoryMain", "Category", new { id = projId, name = Session["projectName"].ToString() });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Unknown error occurred!" });
            }

            try
            {
                if (string.IsNullOrEmpty(reqname) || string.IsNullOrEmpty(reqdesc) && reqvote != 0)
                {
                    ModelState.AddModelError("", "No requirement created");
                    return RedirectToAction("CategoryMain", "Category", new { id = projId, name = Session["projectName"].ToString() });
                }
            }
            catch
            {
                return Json(new { success = false, message = "Unknown error occurred!" });
            }

            if (ModelState.IsValid)
            {
                ReviseDBEntities con = new ReviseDBEntities();
                var req = new requirement()
                {
                    reqName = reqname,
                    description = reqdesc
                };

                var requsr = new userCatReq()
                {
                    reqId = req.reqId,
                    catId = Convert.ToInt32(Session["Catid"]),
                    usrid = Session["userid"].ToString(),
                    rate = reqvote
                };

                con.requirements.Add(req);
                con.userCatReqs.Add(requsr);
                con.SaveChanges();

                return RedirectToAction("CategoryMain", "Category", new { id = projId, name = Session["projectName"].ToString() });
            }

            return RedirectToAction("CategoryMain", "Category", new { id = projId, name = Session["projectName"].ToString() });
        }

    }
}
