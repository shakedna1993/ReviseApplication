using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ReviseApplication.Models;

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

        public CategoryMain CatView()
        {
            List<category> CatList = new List<category>();
            ReviseDBEntities con = new ReviseDBEntities();

            foreach (var cat in con.categories)
                CatList.Add(cat);
            var ShowView = new CategoryMain()
            {
                Cat = CatList
            };
            return ShowView;
        }

    }
}