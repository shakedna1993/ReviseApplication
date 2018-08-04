using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReviseApplication.Models
{
    public class projectModel
    {
        [Required]
        [Display(Name = "Project Name")]
        public string projname { get; set; }

        public int projid { get; set; }

        [Required]
        [Display(Name = "Creation date")]
        public string Date { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string status { get; set; }

        [Required]
        [Display(Name = "Score")]
        public int score { get; set; }

        [Required]
        [Display(Name = "Participants")]
        public List<user> members { get; set; }

        public int MemberRole { get; set; }
    }

    public class Gamfication
    {
        [Required]
        [Display(Name = "Project Name")]
        public string projname { get; set; }

        [Required (ErrorMessage = "Please select gamigication method")]
        [Display(Name = "Gamification")]
        public string SelectedGame { get; set; }
        public IEnumerable<SelectListItem> Gamification { get; set; }

    }

}