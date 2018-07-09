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

        [Display(Name = "Requirement ID:")]
        public int reqid { get; set; }

        [Display(Name = "Requirement Name:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter requirement name")]
        public string reqname { get; set; }

        [Display(Name = "Description:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter requirement description")]
        public string reqdesc { get; set; }
    }

    public class ReqRate
    {
        [Display(Name = "Please enter a number between 0-100:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter requirement rate")]
        [Range(0, 100)]
        public int reqvote { get; set; }
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

        [Display(Name = "Satisfaction limit:")]
        public int totalLimit { get; set; }
        public int role { get; set; }
    }

    public class CreateCategory
    {
        [Display(Name = "Category Name:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter category name")]
        public string catname { get; set; }

        [Display(Name = "Satisfaction limit:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter category satasfiction limit")]
        [Range(0,100)]
        public int totalLimit { get; set; }

        [Display(Name = "Category ID:")]
        public int catid { get; set; }

        public int projid { get; set; }

    }
}