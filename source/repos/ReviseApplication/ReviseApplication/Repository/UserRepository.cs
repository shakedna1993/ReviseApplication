using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ReviseApplication.Models;


namespace ReviseApplication.Repository
{
    /// <summary>
    /// class to get users
    /// </summary>
    public class UserRepository
    {
        /// <summary>
        /// function that gets all users and adds them to a list
        /// </summary>
        /// <returns>returns list of users</returns>
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
                    Text = ""
                };
                users.Insert(0, userType);
                return new SelectList(users, "Value", "Text");
            }
        }
    }
}
