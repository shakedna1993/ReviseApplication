using System;
using System.Collections.Generic;
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
            List<category> catlist = new List<category>();

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

                // the calculation of the thresholdScoreValue
                double denominator = numOfMembers * 5;
                int numerator = prjcat.score ?? 0;
                double total = Convert.ToDouble((numerator / denominator)) * 100;
                con.projCats.SingleOrDefault(p => p.project.ProjId == id).score = Convert.ToInt32(total);
                
                if (cat.totalLimit <= prjcat.status)
                    con.projCats.SingleOrDefault(p => p.project.ProjId == id).isActive = true;
                else
                    con.projCats.SingleOrDefault(p => p.project.ProjId == id).isActive = false;

                // if the thresholdScoreValue bigger than the thresholdScore so the requirement is aprroved
                if (total >= con.projCats.SingleOrDefault(p => p.project.ProjId == id).score)
                {
                    con.projCats.SingleOrDefault(p => p.project.ProjId == id).isFinish = true;
                }

                con.SaveChanges();
            }

            var repo = new MainRepository();
            var Main = repo.CatView();
            return View(Main);
            //return View();
        }


        [HttpGet]
        public ActionResult Requirements(int? catid, int? projid)
        {
            var repo = new MainRepository();
            var Main = repo.ReqView(catid);
            return View(Main);
        }
    }
}