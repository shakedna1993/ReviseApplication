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
            var ShowView = new ProjectDetails()
            {
                projid = id ?? default(int),
                projname = con.projects.Find(id).ProjName,
                projdesc = con.projects.Find(id).description
            };
            return (ShowView);
        }

        public CategoryMain CatView(string uid, int? pid)
        {
            List<category> CatList = new List<category>();
            ReviseDBEntities con = new ReviseDBEntities();

            foreach (var cat in con.categories)
                CatList.Add(cat);

            int roleNum = con.projUsers.Where(u => u.userid == uid).SingleOrDefault(p => p.projid == pid).role ?? 7;

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

    }
}