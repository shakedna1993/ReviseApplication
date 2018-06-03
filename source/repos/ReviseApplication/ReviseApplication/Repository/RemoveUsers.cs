using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReviseApplication.Repository
{
    public class RemoveUsers
    {
        public IEnumerable<SelectListItem> GetUsers(int? id)
        {
            using (var context = new ReviseDBEntities())
            {
                List<projUser> prj = context.projUsers.Where(p => p.projid == id).ToList();
                List<user> usr = new List<user>();
                foreach (var p in prj)
                    usr.Add(p.user);

                List<SelectListItem> users = usr
                    .OrderBy(n => n.UserName)
                        .Select(n =>
                        new SelectListItem
                        {
                            Value = n.UserName.ToString(),
                            Text = n.fname + " " + n.lname
                        }).ToList();
                var userType = new SelectListItem()
                {
                    Value = null,
                    Text = "Remove users:"
                };
                users.Insert(0, userType);
                return new SelectList(users, "Value", "Text");
            }
        }
    }
}