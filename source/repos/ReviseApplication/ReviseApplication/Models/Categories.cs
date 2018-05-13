using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ReviseApplication.Models
{
    public class Categories
    {
        [Required]
        [StringLength(150)]
        public string category { get; set; }

        [Required]
        public int status { get; set; }
    }

    public class CategoryMain
    {
        [Required]
        [Display(Name = "Categories")]
        public List<category> Cat { get; set; }

        [Required]
        public int status { get; set; }

        [Display(Name = "Personal score:")]
        public int score { get; set; }

        [Display(Name = "Project score:")]
        public int rate { get; set; }

    }
}