using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReviseApplication.Repository
{
    public class DepartmentRepository
    {
        public IEnumerable<SelectListItem> GetDepart()
        {
            using (var context = new ReviseDBEntities())
            {
                List<SelectListItem> deps = context.departments.AsNoTracking()
                    .OrderBy(n => n.depId)
                        .Select(n =>
                        new SelectListItem
                        {
                            Value = n.depId.ToString(),
                            Text = n.depName
                        }).ToList();
                var DepType = new SelectListItem()
                {
                    Value = null,
                    Text = "--- select department ---"
                };
                deps.Insert(0, DepType);
                return new SelectList(deps, "Value", "Text");
            }
        }
    }
}