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


        /// <summary>
        /// the function shows project for a specific member.
        /// </summary>
        /// <returns>list of projects the user belongs to</returns>
        // GET: projects
        [HttpGet]
        public ActionResult ProjectMain()
        {
            List<IEnumerable<projUser>> memberslist = new List<IEnumerable<projUser>>();
            List<projectModel> ProjectsList = new List<projectModel>();
            ReviseDBEntities con = new ReviseDBEntities();
            string userId = Session["userid"].ToString();
            var prj = con.projUsers.Where(u => u.userid == userId).ToList();
            List<projUser> Nodup = new List<projUser>();

            if (con.users.Find(userId).IsAdmin == true)
                prj = con.projUsers.ToList();

            foreach (var temp in con.projects)
                foreach(var tmp in prj)
                    if(tmp.projid == temp.ProjId)
                    {
                        Nodup.Add(tmp);
                        break;
                    }

            foreach(var pr in Nodup)
            {
                projectModel proj = new projectModel();
                proj.projid = pr.projid;
                proj.Date = pr.project.creation_date.ToString();
                proj.status = pr.project.status;
                proj.projname = pr.project.ProjName;
                proj.score = pr.project.totalScore ?? 0;
                proj.MemberRole = pr.role ?? 7;
                memberslist.Add(con.projUsers.Where(u => u.projid == pr.projid));
                ProjectsList.Add(proj);
            }

            Session["members"] = memberslist;
            Session["projects"] = ProjectsList.ToList();

            return View(ProjectsList.ToList());
        }

        [HttpGet]
        public ActionResult CreateProj()
        {
            var repo = new MainRepository();
            var Main = repo.ProjCreateView();
            return View(Main);
        }

        [HttpGet]
        public ActionResult ProjectDetails(int? id)
        {
            Session["projId"] = id;
            var repo = new MainRepository();
            var Main = repo.ProjView(id);
            ReviseDBEntities con = new ReviseDBEntities();
            List<projUser> proj = con.projUsers.Where(p => p.projid == id).ToList();
            ViewBag.members = proj;
            return View(Main);
        }

        [HttpGet]
        public ActionResult EditProject(int? id)
        {
            var repo = new MainRepository();
            var Main = repo.EditProjView(id);
           
            Session["projId"] = id;     

            return View(Main);
        }

        [HttpPost]
        [AllowAnonymous]
        [Authorize]
        public ActionResult ProjectMain(String txtfind)
        {
            var ProjList = Session["projects"] as List<projectModel>;
            var projects = (ProjList.Where(p => p.projname.Contains(txtfind)));
            return View(projects.ToList());
        }


        [HttpPost]
        [AllowAnonymous]
        [Authorize]
        public ActionResult EditProject(string projname, string projdesc, IEnumerable<string> SelectRemoveUser, IEnumerable<string> SelectedUser)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            int id = Convert.ToInt32(Session["projId"]);
            string Pname = con.projects.Find(id).ProjName;
            string Pdesc = con.projects.Find(id).description;
            project proj = con.projects.Find(id);
            List<projUser> tempList = new List<projUser>();
            List<user> Assigned = new List<user>();
            List<user> Remove = new List<user>();

            try
            {
                if (projname == Pname && projdesc == Pdesc && SelectedUser == null && SelectRemoveUser == null)
                    return RedirectToAction("ProjectMain", "Project");
            }
            catch
            {
                return Json(new { success = false, message = "Unknown error occurred!" });
            }

            if (ModelState.IsValid)
            {
                con.projects.Find(id).ProjName = projname;
                con.projects.Find(id).description = projdesc;

                if(SelectRemoveUser != null)
                {
                    foreach (var usr in SelectRemoveUser.ToList())
                    {
                        var newusr = (con.users.Where(u => u.UserName == usr));
                        Remove.Add(newusr.FirstOrDefault());
                    }

                    foreach (var usr in Remove)
                        tempList.Add(con.projUsers.Find(usr.userid, id));


                    foreach(var tmp in tempList)
                        con.projUsers.Remove(tmp);
                    con.SaveChanges();
                }
                con.SaveChanges();

                if (SelectedUser != null)
                {
                    foreach (var usr in SelectedUser.ToList())
                    {
                        var newusr = (con.users.Where(u => u.UserName == usr));
                        Assigned.Add(newusr.FirstOrDefault());
                    }

                    List<projUser> prjUser = new List<projUser>();

                    foreach (var usr in Assigned)
                    {
                        var prjusr = new projUser();
                        prjusr.project = proj;
                        prjusr.user = usr;
                        prjusr.projid = proj.ProjId;
                        prjusr.userid = usr.userid;
                        prjUser.Add(prjusr);
                    }

                    foreach (var user in prjUser)
                        con.projUsers.Add(user);
                    con.SaveChanges();

                    Session["IdProjectToAssign"] = proj.ProjId;
                    return RedirectToAction("AssignMembers", new { id = proj.ProjId });
                }

                return RedirectToAction("ProjectMain", "Project");
            }
            return Json(new { success = false, message = "Unknown error occurred!" });
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
                    description = projdesc,
                    status = "Open" 
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
                    prjusr.role = 7;
                    prjusr.dep = 5;
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
                Session["projectName"] = proj.ProjName;

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