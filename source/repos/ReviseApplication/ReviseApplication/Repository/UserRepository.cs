using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReviseApplication.Models;


namespace ReviseApplication.Repository
{
    public class UserRepository
    {
        public IEnumerable<SelectListItem> GetUsers()
        {
            using (var context = new ReviseDBEntities())
            {
                List<SelectListItem> users = context.users.AsNoTracking()
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
                    Text = "--- select user ---"
                };
                users.Insert(0, userType);
                return new SelectList(users, "Value", "Text");
            }
        }
    }
}
