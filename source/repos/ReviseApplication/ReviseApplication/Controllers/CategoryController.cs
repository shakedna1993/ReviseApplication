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

            foreach (var usr in con.users)
                Allusers.Add(usr);

            foreach (var usr2 in Allusers)
            {
                var prj = usr2.projects.Select(p => p.ProjId);
                foreach (var p in prj)
                    if (p == id)
                        memberslist.Add(usr2);
            }
            ViewBag.memberslist = memberslist;
            Session["MemberInProj"] = memberslist;

            var repo = new MainRepository();
            var Main = repo.CatView();
            return View(Main);
        }
    }
}