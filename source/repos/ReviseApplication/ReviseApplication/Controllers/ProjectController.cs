using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReviseApplication.Repository;
using ReviseApplication.Models;
using System.Text;
using System.Collections;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using System.Web.UI;
using Vereyon.Web;
using System.Net;

namespace ReviseApplication.Controllers
{
    /// <summary>
    /// controller for all the things connected to projects
    /// </summary>
    public class ProjectController : Controller
    {
        /// <summary>
        /// function for assign role to uesrs in a project
        /// </summary>
        /// <param name="id">the project id</param>
        /// <returns></returns>
        public ActionResult AssignMembers(int? id)
        {

            List<user> memberslist = new List<user>();
            ReviseDBEntities con = new ReviseDBEntities();
            List<user> Allusers = new List<user>();
            var prj = con.projUsers.Where(u => u.projid == id).ToList();  //finding all the project uesrs

            foreach (var p in prj)
                memberslist.Add(con.users.SingleOrDefault(u => u.userid == p.userid));   //creating a list from those users
    
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
            int prjScore = 0;
            int usrScore = 0;
            int usrproj = 0;

            //if the connected user is admin, shows him all projects
            if (con.users.Find(userId).IsAdmin == true)
                prj = con.projUsers.ToList();

            foreach (var temp in con.projects)
                foreach(var tmp in prj)
                    if(tmp.projid == temp.ProjId)
                    {
                        Nodup.Add(tmp);
                        break;
                    }

            foreach(var pr in Nodup)   //shows only the projects the user is assign to
            {
                int active = 0;
                projectModel proj = new projectModel();
                proj.projid = pr.projid;
                proj.Date = pr.project.creation_date.ToString("dd/MM/yyyy");

                //updating the project status
                foreach (var prjc in con.projCats.Where(p => p.projId == pr.projid).ToList())
                    if (prjc.isActive == true || prjc.isActive == null)
                        active++;
                if (active == 0)
                {
                    con.projects.Find(pr.projid).status = "Close";
                    con.SaveChanges();
                    proj.status = "Close";
                }
                else
                {
                    if (con.projects.Find(pr.projid).status == "Open")
                        proj.status = pr.project.status;
                    else
                    {
                        con.projects.Find(pr.projid).status = "Open";
                        con.SaveChanges();
                        proj.status = "Open";
                    }
                }
                proj.projname = pr.project.ProjName;
                proj.MemberRole = pr.role ?? 7;
                memberslist.Add(con.projUsers.Where(u => u.projid == pr.projid));
                usrproj = UserScoreCalc(pr.projid, userId);

                //calculating the project score
                foreach (var p in con.projUsers.Where(p => p.projid == pr.projid).ToList())
                    prjScore = prjScore + p.grade ?? prjScore;
                usrScore = usrScore + usrproj;
                con.projects.SingleOrDefault(p => p.ProjId == pr.projid).totalScore = prjScore;
                con.SaveChanges();
                proj.score = prjScore;
                ProjectsList.Add(proj);
                prjScore = 0;
            }

            con.users.Find(userId).score = usrScore;
            con.SaveChanges();
            Session["UserScore"] = usrScore;

            Session["members"] = memberslist;
            Session["projects"] = ProjectsList.ToList();

            return View(ProjectsList.ToList());
        }

        /// <summary>
        /// function to show the page of create new project
        /// </summary>
        /// <returns>create project view</returns>
        [HttpGet]
        public ActionResult CreateProj()
        {
            var repo = new MainRepository();
            var Main = repo.ProjCreateView();
            return View(Main);
        }

        /// <summary>
        /// function to show the page of project details
        /// </summary>
        /// <param name="id">project id</param>
        /// <returns>shows the project details</returns>
        [HttpGet]
        public ActionResult ProjectDetails(int? id)
        {
            Session["projId"] = id;   //updating project id
            var repo = new MainRepository();
            var Main = repo.ProjView(id);
            ReviseDBEntities con = new ReviseDBEntities();
            List<projUser> proj = con.projUsers.Where(p => p.projid == id).ToList();  //get all the participents of the project
            ViewBag.members = proj;
            return View(Main);
        }

        /// <summary>
        /// function to show the page of edit project
        /// </summary>
        /// <param name="id">the project id</param>
        /// <returns>shows the edit screen</returns>
        [HttpGet]
        public ActionResult EditProject(int? id)
        {
            var repo = new MainRepository();
            var Main = repo.EditProjView(id);
           
            Session["projId"] = id;     

            return View(Main);
        }

        /// <summary>
        /// function that shows the choose gamification page
        /// </summary>
        /// <param name="id">project id</param>
        /// <returns>shows the gamification page</returns>
        [HttpGet]
        public ActionResult Gamification(int ? id)
        {
            Session["GameProjId"] = id;
            var repo = new MainRepository();
            var Main = repo.GameView(id);
            return View(Main);
        }

        /// <summary>
        /// function that set gamification method for a project
        /// </summary>
        /// <param name="SelectedGame">the name of the selected gamification method</param>
        /// <returns>redirect to project main screen</returns>
        [HttpPost]
        public ActionResult Gamification(IEnumerable<string> SelectedGame)
        {
            //if no method selected
            if (SelectedGame == null)
            {
                TempData["NoGame"] = "No gamfication method selected";
                return RedirectToAction("ProjectMain", "Project");
            }

            var ChooseAssignGame = SelectedGame.ToList();
            int proj = Convert.ToInt32(Session["GameProjId"]);  //gets project id
            ReviseDBEntities con = new ReviseDBEntities();

            if (ChooseAssignGame.SingleOrDefault() == "")
            {
                TempData["NoGame"] = "No gamfication method selected";
                return RedirectToAction("ProjectMain", "Project");
            }
            else  //update the projet gamification method
            {
                int game_id = Int32.Parse(ChooseAssignGame.SingleOrDefault());
                var game = con.gamifications.Where(g => g.gamId == game_id);
                con.projects.Find(proj).game = game_id;
                con.projects.Find(proj).gamification = con.gamifications.Find(game_id);

            }

            con.SaveChanges();
            return RedirectToAction("ProjectMain", "Project");
        }

        /// <summary>
        /// function that shows all project of the user, project main screen
        /// </summary>
        /// <param name="txtfind">search result</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Authorize]
        public ActionResult ProjectMain(String txtfind)
        {
            var ProjList = Session["projects"] as List<projectModel>;
            var projects = (ProjList.Where(p => p.projname.Contains(txtfind)));
            return View(projects.ToList());
        }

        /// <summary>
        /// function that edit the project details
        /// </summary>
        /// <param name="projname">the provided project name</param>
        /// <param name="projdesc">the provided project description</param>
        /// <param name="SelectRemoveUser">the provided users to remove from the project</param>
        /// <param name="SelectedUser">the provided users to add to the project</param>
        /// <returns>redirect to project main screen, if theres an error- redirect to the edit screen with error</returns>
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
            List<message> MessagesToDel = new List<message>();
            List<user> Remove = new List<user>();

            //if nothing change or project with this name already exist
            try
            {
                if (projname == Pname && projdesc == Pdesc && SelectedUser == null && SelectRemoveUser == null)
                {
                    TempData["NoChanges"] = "No changes made";
                    return RedirectToAction("ProjectMain", "Project");
                }
                List<project> prjlist = con.projects.ToList();
                foreach(var prj in prjlist)
                    if (prj.ProjId != id && prj.ProjName == projname)
                    {
                        TempData["ProjExist"] = "Project with this name already exist";
                        return RedirectToAction("EditProject", "Project", new { id = id });
                    }
            }
            catch
            {
                TempData["Unknown"] = "Unknown error occurred!";
                return RedirectToAction("EditProject", "Project", new { id = id });
            }

            //updating the project information
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
                    {
                        MessagesToDel = con.messages.Where(p => p.projId == tmp.projid).Where(u => u.userid == tmp.userid).ToList();
                        if (MessagesToDel != null)
                            foreach (var msg in MessagesToDel)
                                con.messages.Remove(msg);
                        con.projUsers.Remove(tmp);
                    }
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

                    foreach (var usr in Assigned)   //assign users to the project
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
            TempData["Unknown"] = "Unknown error occurred!";
            return RedirectToAction("EditProject", "Project");
        }

        /// <summary>
        /// function for project creation
        /// </summary>
        /// <param name="projname">the provided project name</param>
        /// <param name="projdesc">the provided project description</param>
        /// <param name="SelectedUser">the provided users to assign</param>
        /// <returns>redirect to project main screen, if theres errors- redirect to create screen with error</returns>
        [HttpPost]
        [AllowAnonymous]
        [Authorize]
        public ActionResult CreateProj(string projname, string projdesc, IEnumerable<string> SelectedUser)
        {
            try
            {
                if (string.IsNullOrEmpty(projname) || string.IsNullOrEmpty(projdesc) || SelectedUser == null) //if fileds are empty
                {
                    TempData["EmptyFields"] = "One or more field is empty";
                    return RedirectToAction("CreateProj", "Project");
                }

            }
            catch
            {
                TempData["Unknown"] = "Unknown error occurred!";
                return RedirectToAction("CreateProj", "Project");
            }

            //updating the DB- create new project
            if (ModelState.IsValid)
            {
                ReviseDBEntities con = new ReviseDBEntities();

                var proj = new project
                {
                    ProjName = projname,
                    creation_date = DateTime.Today,
                    description = projdesc,
                    status = "Open" 
                };
                var usrList= new List<user>();
                
                foreach (var usr in SelectedUser.ToList()) {
                    var newusr = (con.users.Where(u => u.UserName == usr));
                    usrList.Add(newusr.FirstOrDefault());
                }

                List<projUser> Allusers = new List<projUser>();

                foreach (var usr in usrList)   //users to assign to the project
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
                try
                {
                    con.projects.Add(proj);
                    con.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    TempData["FailedProj"] = "Project with this name already exist";
                    return RedirectToAction("CreateProj", "Project");
                }
                
                List<category> catlist = new List<category>();
                catlist = con.categories.ToList();

                //link categories to the project
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
            {
                TempData["NoUsers"] = "No users selected";
                return RedirectToAction("CreateProj", "Project");
            }
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

        /// <summary>
        /// function that assign roles for users in a project
        /// </summary>
        /// <param name="SelectedDepartment">the users department</param>
        /// <param name="SelectedRole">the users role</param>
        /// <returns>redirect to project main screen, if there are errors- redirect to assign members with error</returns>
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

            //checks if department and role selected, if so- updating the role of the users in the project
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

        /// <summary>
        /// function that delete specific project
        /// </summary>
        /// <param name="id">the project id for delete</param>
        /// <returns>returns to the project main</returns>
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["emptyIDprj"] = "An error occurred while trying to delete";
                return RedirectToAction("ProjectMain", "Project");
            }
            var repo = new MainRepository();
            var Main = repo.ProjView(id);
            ReviseDBEntities con = new ReviseDBEntities();
            List<projUser> proj = con.projUsers.Where(p => p.projid == id).ToList();
            ViewBag.members = proj;
            return View(Main);
        }

        /// <summary>
        /// function that delete specific project
        /// </summary>
        /// <param name="id">the category id for project</param>
        /// <returns>pop up message to verify delete</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            project proj = con.projects.Find(id);
            var prjusr = con.projUsers.Where(p => p.projid == id).ToList();
            //removes the users from the project
            if (prjusr != null)
            {
                foreach (var prj in prjusr) 
                    con.projUsers.Remove(prj);
            }
            //remove all project catefories
            var prjcat = con.projCats.Where(p => p.projId == id).ToList();
            foreach (var prj in prjcat)
                con.projCats.Remove(prj);
            //remove the project
            var Projreq = con.requirements.Where(p => p.projid == id).ToList();
            if (Projreq != null)
            {
                foreach (var req in Projreq)
                    con.requirements.Remove(req);
            }
            
            var message = con.messages.Where(p => p.projId == id).ToList();
            if(message != null)
            {
                foreach (var msg in message)
                    con.messages.Remove(msg);
            }

            con.projects.Remove(proj);
            con.SaveChanges();
            return RedirectToAction("ProjectMain", "Project");
        }

        /// <summary>
        /// function for score calculation
        /// </summary>
        /// <param name="prjid">the project id</param>
        /// <param name="usrid">the user id</param>
        /// <returns>returns the score of the user in the project</returns>
        public int UserScoreCalc(int prjid, string usrid)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            int usrPrjScore = 0;

            List<message> message = con.messages.ToList();
            int count = 0;

            foreach (var msg in message)
                if (msg.projId == prjid && msg.userid == usrid)
                    count++;
            usrPrjScore = count;
            if (usrPrjScore == 0)
                return 0;
            con.projUsers.Where(u => u.userid == usrid).SingleOrDefault(p => p.projid == prjid).grade = usrPrjScore;
            con.SaveChanges();
            return usrPrjScore;
        }

        /// <summary>
        /// function for exporting software requirement specification document
        /// </summary>
        /// <param name="id">the project id</param>
        /// <returns>shows the document</returns>
        public ActionResult ReqExport(int id)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            int ProjectId = id;
            ViewBag.MemberProjectList = con.projUsers.Where(p => p.projid == ProjectId).ToList();
            var req = con.requirements.Where(p => p.projid == ProjectId).ToList();
            Session["projectName"] = con.projects.SingleOrDefault(p => p.ProjId == id).ProjName;
            List<ReviseApplication.Models.Categories> list = new List<Categories>();
            foreach (var r in req)
            {
                ReviseApplication.Models.Categories cat = new ReviseApplication.Models.Categories();
                cat.category = r.category.CatName;
                cat.reqname = r.reqName;
                cat.reqdesc = r.description;
                list.Add(cat);
            }
            return View(list);
        }
    }
}