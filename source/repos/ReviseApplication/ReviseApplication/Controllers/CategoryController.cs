using System;
using System.Collections.Generic;
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
                if (CatName == catname || CatLimit == totalLimit)
                    return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString()});
            }
            catch
            {
                return Json(new { success = false, message = "Unknown error occurred!" });
            }

            if (ModelState.IsValid)
            {
                con.categories.Find(id).CatName = CatName;
                con.projCats.Where(c => c.catId == id).SingleOrDefault(p => p.projId == projid).totalLimit = CatLimit;
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
                con.SaveChanges();
                return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
            }
            return RedirectToAction("CategoryMain", "Category", new { id = projid, name = Session["projectName"].ToString() });
        }
    }
}