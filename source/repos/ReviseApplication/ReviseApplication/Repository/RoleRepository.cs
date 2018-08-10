using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReviseApplication.Repository
{
    /// <summary>
    /// class for role list
    /// </summary>
    public class RoleRepository
    {
        /// <summary>
        /// function that gets roles from the DB and add them to a list
        /// </summary>
        /// <returns>return list of roles</returns>
        public IEnumerable<SelectListItem> GetRole()
        {
            using (var context = new ReviseDBEntities())
            {
                List<SelectListItem> roles = context.roles.AsNoTracking()
                    .OrderBy(n => n.RoleID)
                        .Select(n =>
                        new SelectListItem
                        {
                            Value = n.RoleID.ToString(),
                            Text = n.RoleName
                        }).ToList();
                var RoleType = new SelectListItem()
                {
                    Value = null,
                    Text = "--- select role ---"
                };
                roles.Insert(0, RoleType);
                return new SelectList(roles, "Value", "Text");
            }
        }
    }
}