using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ReviseApplication.Models
{
    public class AssignToProj
    {
        [Required]
        [Display(Name = "Department")]
        public string SelectedDepartment { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; }
    }

    public class CreateProj
    {
        public int projid { get; set; }

        [Display(Name = "Project name:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter project name")]
        public string projname { get; set; }

        [Display(Name = "Description:")]
        //        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter project description")]
        public string projdesc { get; set; }

        [Display(Name = "Select participants:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please choose participents")]
        public string SelectedUser { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
    }

    public class EditProject
    {
        [Display(Name = "Project ID:")]
        public int projid { get; set; }

        [Display(Name = "Project name:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter project name")]
        public string projname { get; set; }

        [Display(Name = "Description:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter project description")]
        public string projdesc { get; set; }

        [Display(Name = "Select participants:")]
        public string SelectedUser { get; set; }
        public IEnumerable<SelectListItem> AssignUser { get; set; }

        [Display(Name = "Remove participants:")]
        public string SelectRemoveUser { get; set; }
        public IEnumerable<SelectListItem> RemoveUser { get; set; }
    }

    public class ProjectDetails
    {
        [Display(Name = "Project ID:")]
        public int projid { get; set; }

        [Display(Name = "Project name:")]
        public string projname { get; set; }

        [Display(Name = "Description:")]
        public string projdesc { get; set; }
    }
}