using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReviseApplication.Repository;
using ReviseApplication.Models;
using System.Text;
using System.Collections;

namespace ReviseApplication.Controllers
{
    public class ProjectController : Controller
    {
        public ActionResult AssignMembers(int? id)
        {

            List<user> memberslist = new List<user>();
            ReviseDBEntities con = new ReviseDBEntities();
            List<user> Allusers = new List<user>();
            var prj = con.projUsers.Where(u => u.projid == id).ToList();

            foreach (var p in prj)
                memberslist.Add(con.users.SingleOrDefault(u => u.userid == p.userid));

            ViewBag.memberslist = memberslist;
            Session["AssignList"] = memberslist;

            var repo = new MainRepository();
            var Main = repo.CreateView();
            return View(Main);
        }

        [HttpGet]
        public ActionResult ProjectMain()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CreateProj()
        {
            var repo = new MainRepository();
            var Main = repo.ProjCreateView();
            return View(Main);
        }

        [HttpPost]
        [AllowAnonymous]
        [Authorize]
        public ActionResult CreateProj(string projname, string projdesc, IEnumerable<string> SelectedUser)
        {
            try
            {
                if (string.IsNullOrEmpty(projname) || string.IsNullOrEmpty(projdesc) || SelectedUser == null)
                    return Json(new { success = "Failed", error = "One or more field is empty" });
            }
            catch
            {
                return Json(new { success = false, message = "Unknown error occurred!" });
            }

            if (ModelState.IsValid)
            {
                var proj = new project
                {
                    ProjName = projname,
                    creation_date = DateTime.Today,
                    description = projdesc
                };

                var usrList= new List<user>();
                ReviseDBEntities con = new ReviseDBEntities();
                foreach (var usr in SelectedUser.ToList()) {
                    var newusr = (con.users.Where(u => u.UserName == usr));
                    usrList.Add(newusr.FirstOrDefault());
                }

                List<projUser> Allusers = new List<projUser>();

                foreach (var usr in usrList)
                {
                    var prjusr = new projUser();
                    prjusr.project = proj;
                    prjusr.user = usr;
                    prjusr.projid = proj.ProjId;
                    prjusr.userid = usr.userid;
                    Allusers.Add(prjusr);
                }
                //proj.users = usrList;
                foreach(var user in Allusers)
                    con.projUsers.Add(user);
                con.projects.Add(proj);
                con.SaveChanges();

                List<category> catlist = new List<category>();
                catlist = con.categories.ToList();
                foreach (var cat in catlist)
                {
                    projCat entity = new projCat();
                    entity.category = cat;
                    entity.catId = cat.CatId;
                    entity.project = proj;
                    entity.projId = proj.ProjId;
                    con.projCats.Add(entity);
                    con.SaveChanges();
                }

                // the new id of project that create
                Session["IdProjectToAssign"] = proj.ProjId;

                // now the user Redirect To assign role every member in project. 
                return RedirectToAction("AssignMembers", new{ id = proj.ProjId });
            }

            if (SelectedUser == null)
                return Json(new { success = "Failed", error = "No users selected" });
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("You selected- " + string.Join(",", SelectedUser));
                return Json(new
                {
                    success = true,
                    message = sb.ToString()
            });
            }

        }

        [HttpPost]
        public ActionResult AssignMembers(IEnumerable<string> SelectedDepartment, IEnumerable<string> SelectedRole)
        {
            if (SelectedDepartment == null || SelectedRole == null)
                return Json(new { success = "Failed", error = "No department or role selected" });

            var ChooseAssigndep = SelectedDepartment.ToList();
            var ChooseAssignrole = SelectedRole.ToList();
            List<user> UsersToAssign = (List<user>)Session["AssignList"];
            int proj = Convert.ToInt32(Session["IdProjectToAssign"]);
            ReviseDBEntities con = new ReviseDBEntities();


            for (int i = 0; i < UsersToAssign.Count; i++)
            {
                if ((ChooseAssigndep.ElementAt(i) == "") ||(ChooseAssignrole.ElementAt(i) == ""))
                    return Json(new { success = "Failed", error = "No department or role selected" });
                else
                {
                    int dep_id = Int32.Parse(ChooseAssigndep.ElementAt(i));
                    int role_id = Int32.Parse(ChooseAssignrole.ElementAt(i));
                    var role = con.roles.Where(r => r.RoleID == role_id);
                    var dep = con.departments.Where(d => d.depId == dep_id);
                    var myId = UsersToAssign[i].userid;
                    var uspr = con.projUsers.Where(u => u.userid == myId).ToList();
                    foreach(var usr in uspr)
                    {
                        if (usr.projid == proj)
                        {
                            usr.department = dep.First();
                            usr.role1 = role.First();
                            usr.role = role.First().RoleID;
                            usr.dep = dep.First().depId;

                        }
                            

                    }
                }
                con.SaveChanges();
            }

            return RedirectToAction("CategoryMain", "Category", new { id = proj });
            // return RedirectToAction("CategoryMain/" + Convert.ToInt32(Session["IdProjectToAssign"]) + "", "Category", new { id = proj });
        }

    }
}