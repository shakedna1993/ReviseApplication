using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReviseApplication.Models;


namespace ReviseApplication.Repository
{
    /// <summary>
    /// class for assign users to project
    /// </summary>
    public class AssignUserRepository
    {
        /// <summary>
        /// function that gets all users of a project and puts them in dropdown list
        /// </summary>
        /// <param name="id">the project id</param>
        /// <returns>return select list of users</returns>
        public IEnumerable<SelectListItem> GetUsers(int? id)
        {
            using (var context = new ReviseDBEntities())
            {
                List<user> Allusers = context.users.ToList();
                List<projUser> prj = context.projUsers.Where(p => p.projid == id).ToList();
                List<user> usr = new List<user>();
                foreach (var p in prj)
                    usr.Add(p.user);

                foreach (var u in usr)
                    Allusers.Remove(u);

                List<SelectListItem> users = Allusers
                    .OrderBy(n => n.UserName)
                        .Select(n =>
                        new SelectListItem
                        {
                            Value = n.UserName.ToString(),
                            Text = n.fname+" "+n.lname
                        }).ToList();
                var userType = new SelectListItem()
                {
                    Value = null,
                    Text = "Assign users:"
                };
                users.Insert(0, userType);
                return new SelectList(users, "Value", "Text");
            }
        }
    }
}
