using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReviseApplication.Models;
using ReviseApplication.Repository;

namespace ReviseApplication.Controllers
{
    public class CategoryController : Controller
    {
        // GET: Category
        [HttpGet]
        public ActionResult CategoryMain(int? id)
        {
            List<user> memberslist = new List<user>();
            ReviseDBEntities con = new ReviseDBEntities();
            List<user> Allusers = new List<user>();
            List<category> catlist = con.categories.ToList();

            project proj = con.projects.Where(p => p.ProjId == id).First();
            string name = proj.ProjName.ToString();
            Session["projectName"] = name;
            Session["projectid"] = id;
            var prj = con.projUsers.Where(u => u.projid == id).ToList();
            var numOfMembers = con.projUsers.Where(u => u.projid == id).Count();
            
            foreach (var usr in con.users)
                Allusers.Add(usr);

            foreach (var p in prj)
                memberslist.Add(p.user);

            ViewBag.memberslist = memberslist;
            Session["MemberInProj"] = memberslist;

           foreach(var cat in catlist)
            {
                var prjcat = cat.projCats.SingleOrDefault(p => p.project.ProjId == id);
                int catid = cat.CatId;
                if (prjcat == null)
                    continue;
                    
                // the calculation of the thresholdScoreValue
                double denominator = numOfMembers * 5;
                int numerator = prjcat.score ?? 0;
                double total = Convert.ToDouble((numerator / denominator)) * 100;
                cat.projCats.SingleOrDefault(p => p.project.ProjId == id).score = Convert.ToInt32(total);
                int TotalLimit = cat.projCats.SingleOrDefault(p => p.project.ProjId == id).totalLimit ?? 0;
                if (prjcat.status == null)
                    prjcat.status = 0;
    
                if (TotalLimit >= prjcat.status)
                    prjcat.isActive = true;

                else
                    prjcat.isActive = false;

                // if the thresholdScoreValue bigger than the thresholdScore so the requirement is aprroved
                if (total >= cat.projCats.SingleOrDefault(p => p.project.ProjId == id).score)
                    prjcat.isFinish = true;

                con.projCats.Find(id, catid).isActive = prjcat.isActive;
                con.projCats.Find(id, catid).isFinish = prjcat.isFinish;
                con.SaveChanges();
            }

            var repo = new MainRepository();
            var Main = repo.CatView(Session["userid"].ToString(),id);
            return View(Main);
            //return View();
        }

        [HttpGet]
        public ActionResult CreateCategory(int? projid)
        {
            Session["Project"] = projid;
            return View();
        }

        [HttpGet]
        public ActionResult EditCategory(int? id, int projid)
        {
            var repo = new MainRepository();
            var Main = repo.CatEditView(id, projid);
            return View(Main);
        }

        [HttpGet]
        public ActionResult Requirement()
        {
            int projId = Convert.ToInt32(Session["projectid"]);
            int catId = Convert.ToInt32(Session["Catid"]);

            ReviseDBEntities con = new ReviseDBEntities();
            int req = 0;
            if (con.requirements.Where(p => p.projid == projId).SingleOrDefault(c => c.catid == catId) != null)
                req = con.requirements.Where(p => p.projid == projId).SingleOrDefault(c => c.catid == catId).reqId;
            if (req == 0)
            {
                Session["ReqExisit"] = 0;
                return View();
            }
            else
            {
                Session["ReqId"] = req;
                Session["ReqExisit"] = 1;
                var repo = new MainRepository();
                var Main = repo.Requirement(req);
                return View(Main);
            }
        }

        [HttpGet]
        public ActionResult Vote()
        {
            int projId = Convert.ToInt32(Session["projectid"]);
            int catId = Convert.ToInt32(Session["Catid"]);
            int reqId = Convert.ToInt32(Session["ReqId"]);
            int IsExist = Convert.ToInt32(Session["ReqExisit"]);
            string userID = Session["userid"].ToString();

            ReviseDBEntities con = new ReviseDBEntities();
            if (IsExist == 1)
            {
                var reqVote = con.userCatReqs.Find(reqId, userID);
                if (reqVote != null)
                {
                    var repo = new MainRepository();
                    var Main = repo.Vote(reqId, userID);
                    return View(Main);
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult Requirement(string reqname, string reqdesc)
        {
            int projId = Convert.ToInt32(Session["projectid"]);
            int catId = Convert.ToInt32(Session["Catid"]);

            try
            {
                if (string.IsNullOrEmpty(reqname) || string.IsNullOrEmpty(reqdesc))
                {
                    TempData["NoReq"] = "No requirement created";
                    return RedirectToAction("CategoryMain", "Category", new { id = projId, name = Session["projectName"].ToString() });
                }
            }
            catch
            {
                TempData["FailedReq"] = "Unknown error occurred!";
                return RedirectToAction("Requirement", "Category");  
            }

            if (ModelState.IsValid)
            {
                ReviseDBEntities con = new ReviseDBEntities();
                if (Convert.ToInt32(Session["ReqExisit"]) == 0)
                {
                    var req = new requirement()
                    {
                        reqName = reqname,
                        description = reqdesc,
                        projid = projId,
                        catid = catId
                    };

                    con.requirements.Add(req);
                }
                else
                {
                    int id = Convert.ToInt32(Session["ReqId"]);
                    con.requirements.SingleOrDefault(r => r.reqId == id).reqName = reqname;
                    con.requirements.SingleOrDefault(r => r.reqId == id).description = reqdesc;
                }
                
                con.SaveChanges();

                return RedirectToAction("Requirement", "Category");
            }

            return RedirectToAction("Requirement", "Category");
        }

        [HttpPost]
        public ActionResult Vote(int reqvote)
        {
            int projId = Convert.ToInt32(Session["projectid"]);
            int catId = Convert.ToInt32(Session["Catid"]);
            int reqId = Convert.ToInt32(Session["ReqId"]);
            int IsExist = Convert.ToInt32(Session["ReqExisit"]);
            string userID = Session["userid"].ToString();

            ReviseDBEntities con = new ReviseDBEntities();
            try
            {
                if (reqvote == 0)
                {
                    TempData["EmptyVote"] = "No vote was set";
                    return RedirectToAction("Vote", "Category");
                }
            }
            catch
            {
                TempData["FailedVote"] = "Unknown error occurred!";
                return RedirectToAction("Vote", "Category");
            }

            if(con.userCatReqs.Find(reqId, userID) != null)
                con.userCatReqs.Find(reqId, userID).rate = reqvote;
            else
            {
                userCatReq RateReq = new userCatReq()
                {
                    usrid = userID,
                    reqId = reqId,
                    rate = reqvote
                };
                con.userCatReqs.Add(RateReq);
            }
            con.SaveChanges();

            return RedirectToAction("Vote", "Category");
        }

        [HttpPost]
        public ActionResult EditCategory(int ? id, string catname, int ? totalLimit)
        {
            try
            {
                if (string.IsNullOrEmpty(catname) || totalLimit == null)
                    return Json(new { success = "Failed", error = "One or more field is empty" });
            }
            catch
            {
                return Json(new { success = false, message = "Unknown error occurred!" });
            }

            ReviseDBEntities con = new ReviseDBEntities();
            var CatName = con.categories.Find(id).CatName;
            int projid = Convert.ToInt32(Session["projectid"]);
            var CatLimit = con.projCats.Where(c => c.catId == id).SingleOrDefault(p => p.projId == projid).totalLimit ?? 0;

            try
            {
                if (CatName == catname && CatLimit == totalLimit)
                    return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString()});
            }
            catch
            {
                return Json(new { success = false, message = "Unknown error occurred!" });
            }

            if (ModelState.IsValid)
            {
                con.categories.Find(id).CatName = catname;
                con.projCats.Where(c => c.catId == id).SingleOrDefault(p => p.projId == projid).totalLimit = totalLimit;
                con.SaveChanges();
                return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
            }
            return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
        }

        [HttpPost]
        public ActionResult CreateCategory(string catname, int? totalLimit)
        {
            int projid = Convert.ToInt32(Session["projectid"]);
            try
            {
                if (string.IsNullOrEmpty(catname) || totalLimit == null)
                    return Json(new { success = "Failed", error = "One or more field is empty" });
            }
            catch
            {
                return Json(new { success = false, message = "Unknown error occurred!" });
            }
            if (ModelState.IsValid)
            {
                int total = 0;
                ReviseDBEntities con = new ReviseDBEntities();
                var catTemp = con.categories.SingleOrDefault(c => c.CatName == catname);
                int catid = 0;
                if (catTemp != null)
                {
                    catid = catTemp.CatId;
                    var prjcat = con.projCats.Where(c => c.catId == catid).SingleOrDefault(p => p.projId == projid);
                    if(prjcat != null)
                    {
                        TempData["FailedCat"] = "Category with this name already exist";
                        return RedirectToAction("CreateCategory", "Category", new { id = projid });
                    }
                    else
                    {
                        total = totalLimit ?? 0;
                        var projcat = new projCat
                        {
                            catId = catid,
                            projId = projid,
                            totalLimit = total
                        };
                        con.projCats.Add(projcat);
                    }
                }
                else
                {
                    var cat = new category
                    {
                        CatName = catname
                    };

                    foreach (var prj in con.projects)
                    {
                        if (prj.ProjId != projid)
                            total = 0;
                        else
                            total = totalLimit ?? 0;

                        var prjcat = new projCat
                        {
                            catId = cat.CatId,
                            projId = prj.ProjId,
                            totalLimit = total
                        };
                        con.projCats.Add(prjcat);
                    }

                    con.categories.Add(cat);
                }
                try
                {
                    
                    con.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    TempData["FailedCat"] = "Category with this name already exist";
                    return RedirectToAction("CreateCategory", "Category", new { id = projid });
                }

                return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
            }
            return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["emptyIDcat"] = "An error occurred while trying to delete";
                return RedirectToAction("CategoryMain", "Category", new { id = Convert.ToInt32(Session["projectid"]), name = Session["projectName"].ToString() });
            }
            var repo = new MainRepository();
            var Main = repo.CatEditView(id, Convert.ToInt32(Session["projectid"]));
            return View(Main);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            int projid = Convert.ToInt32(Session["projectid"]);
            ReviseDBEntities con = new ReviseDBEntities();

            category cat = con.categories.Find(id);
            var catprj = con.projCats.Where(c => c.catId == id).SingleOrDefault(p => p.projId == projid);
            con.projCats.Remove(catprj);

            var catreq = con.requirements.Where(c => c.catid == id).SingleOrDefault(p => p.projid == projid);
            if(catreq !=null)
                con.requirements.Remove(catreq);

            var message = con.messages.Where(c => c.CatId == id).Where(p => p.projId == projid).ToList();
            if (message != null)
            {
                foreach (var msg in message)
                    con.messages.Remove(msg);
            }

            con.SaveChanges();
            return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
        }
    }
}