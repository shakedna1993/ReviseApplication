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

            var repo = new MainRepository();
            var Main = repo.CatView();
            return View(Main);
        }

        [HttpGet]
        public ActionResult Requirements()
        {
            return View();
        }
    }
}