using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReviseApplication.Models;
using System.Web.Mvc;

namespace ReviseApplication.Repository
{
    public class MainRepository
    {
        public AssignToProj CreateView()
        {
            var dRepo = new DepartmentRepository();
            var rRepo = new RoleRepository();
            var ShowView = new AssignToProj()
            {
                Departments = dRepo.GetDepart(),
                Roles = rRepo.GetRole()
            };
            return ShowView;
        }

        public CreateProj ProjCreateView()
        {
            var uRepo = new UserRepository();
            var ShowView = new CreateProj()
            {
                Users = uRepo.GetUsers(),
            };
            return ShowView;
        }

        public EditProject EditProjView(int? id)
        {
            ReviseDBEntities con = new ReviseDBEntities();
            var uRepo = new AssignUserRepository();
            var rRepo = new RemoveUsers();

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