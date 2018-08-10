using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReviseApplication.Models;
using System.Web.Mvc;

namespace ReviseApplication.Repository
{
    /// <summary>
    /// class for creating views
    /// </summary>
    public class MainRepository
    {
        /// <summary>
        /// function that creates view for assign role to users in a project
        /// </summary>
        /// <returns>returns updated model to show in the view</returns>
        public AssignToProj CreateView()
        {
            var dRepo = new DepartmentRepository();   //gets list of departments
            var rRepo = new RoleRepository();         //gets list of rols
            var ShowView = new AssignToProj()         //updating assigntoproj model
            {
                Departments = dRepo.GetDepart(),
                Roles = rRepo.GetRole()
            };
            return ShowView;                        
        }

        /// <summary>
        /// function that creates view for create project
        /// </summary>
        /// <returns>returns updated model to show in the view</returns>
        public CreateProj ProjCreateView()
        {
            var uRepo = new UserRepository();     //gets users list
            var ShowView = new CreateProj()
            {
                Users = uRepo.GetUsers(),
            };
            return ShowView;
        }

        /// <summary>
        /// function that creates view edit project
        /// </summary>
        /// <param name="id"> project id</param>
        /// <returns>returns updated model to show in the view</returns>
        public EditProject EditProjView(int? id)
        {
            ReviseDBEntities con = new ReviseDBEntities();      //connection to the DB
            var uRepo = new AssignUserRepository();             //gets user list to assign
            var rRepo = new RemoveUsers();                      //gets user list to remove

            var ShowView = new EditProject()
            {
                AssignUser = uRepo.GetUsers(id),
                RemoveUser = rRepo.GetUsers(id),
                projid = id ?? default(int),
                projname = con.projects.Find(id).ProjName,
                projdesc = con.projects.Find(id).description,
            };
            return (ShowView);
        }

        /// <summary>
        /// function that create view for project details  
        /// </summary>
        /// <param name="id">project id</param>
        /// <returns>returns updated model to show in the view</returns>
        public ProjectDetails ProjView(int? id)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            int ? game = con.projects.Find(id).game;
            string name;
            if (game == null)
                name = "Gamfication is not set";
            else
                name = con.gamifications.Find(game).gamName;
            var ShowView = new ProjectDetails()
            {
                projid = id ?? default(int),
                projname = con.projects.Find(id).ProjName,
                projdesc = con.projects.Find(id).description,
                projgame = name
            };
            return (ShowView);
        }

        /// <summary>
        /// function that create view for category main
        /// </summary>
        /// <param name="uid">user id</param>
        /// <param name="pid">project id</param>
        /// <returns>returns updated model to show in the view</returns>
        public CategoryMain CatView(string uid, int? pid)
        {
            List<category> CatList = new List<category>();
            ReviseDBEntities con = new ReviseDBEntities();
            var prjcat = con.projCats.Where(p => p.projId == pid).ToList();

            foreach (var cat in con.categories)
                foreach (var prj in prjcat)
                    if(prj.catId == cat.CatId)
                        CatList.Add(cat);
            int roleNum = 7;

            if (con.projUsers.Where(u => u.userid == uid).SingleOrDefault(p => p.projid == pid) != null)
                roleNum = con.projUsers.Where(u => u.userid == uid).SingleOrDefault(p => p.projid == pid).role ?? 7;

            var ShowView = new CategoryMain()
            {
                Cat = CatList,
                role = roleNum
            };
            return ShowView;
        }

        /// <summary>
        /// function that create view for category main 
        /// </summary>
        /// <param name="catid">category id</param>
        /// <param name="projid">project id</param>
        /// <returns>returns updated model to show in the view</returns>
        public CreateCategory CatEditView(int? catid, int projid)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            int cat = con.categories.Find(catid).CatId;
            int limit = con.projCats.Where(c => c.catId == catid).SingleOrDefault(p => p.projId == projid).totalLimit ?? 0;
            string catName = con.categories.Find(catid).CatName;
            var ShowView = new CreateCategory()
            {
                catid = cat,
                catname = catName,
                totalLimit = limit,
                projid = projid
            };
            return ShowView;

        }

        /// <summary>
        /// function that create view for requirement page
        /// </summary>
        /// <param name="id">requirement id</param>
        /// <returns>returns updated model to show in the view</returns>
        public Categories Requirement(int id)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            string req = con.requirements.SingleOrDefault(r => r.reqId == id).reqName;
            string desc = con.requirements.SingleOrDefault(r => r.reqId == id).description;
            var ShowView = new Categories()
            {
                reqid = id,
                reqname = req,
                reqdesc = desc
            };
            return ShowView;
        }

        /// <summary>
        /// function that create view for personal file page
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>returns updated model to show in the view</returns>
        public UserRegestration PersonalFileView(string id)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            var user = con.users.Find(id);

            var ShowView = new UserRegestration()
            {
                UserId = user.userid,
                UserName = user.UserName,
                FirstName = user.fname,
                LastName = user.lname,
                Phonenum = user.PhoneNum,
                EmailID = user.Email,
                DateOfBirth = user.birthday ?? DateTime.Today.Date
            };
            return ShowView;
        }

        /// <summary>
        /// function that create view for vote page
        /// </summary>
        /// <param name="reqid">requirement id</param>
        /// <param name="usrid">user id</param>
        /// <returns>returns updated model to show in the view</returns>
        public ReqRate Vote(int reqid, string usrid)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            var reqRate = con.userCatReqs.Find(reqid, usrid).rate;
            var ShowView = new ReqRate()
            {
                reqvote = reqRate ?? 0
            };
            return ShowView;
        }


        /// <summary>
        /// function that create view for gamification choice
        /// </summary>
        /// <param name="id">game id</param>
        /// <returns>returns updated model to show in the view</returns>
        public Gamfication GameView(int ? id)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            var gRepo = new GameRepository();
            var ShowView = new Gamfication()
            {
                projname = con.projects.Find(id).ProjName,
                Gamification = gRepo.GetGame()
            };
            return ShowView;
        }

    }
}