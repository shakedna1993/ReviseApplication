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

        [Required(ErrorMessage = "Please enter The project name")]
        [Display(Name ="Project name")]
        public string projname { get; set; }

        [Required(ErrorMessage = "Please enter The project description")]
        [Display(Name = "Description")]
        public string projdesc { get; set; }

        [Required(ErrorMessage = "Please choose participents")]
        [Display(Name = "Users to assigin")]
        public string SelectedUser { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }


    }
}