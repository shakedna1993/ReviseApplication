using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.WebPages.Html;

namespace ReviseApplication.Models
{
    public class UserDetails
    {
    }

    public class LoginDetails
    {
        [Required(ErrorMessage = "Please enter your username")]
        [Display(Name = "UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        [Display(Name ="Password")]
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        public string Password { get; set; }
    }

    public class UserRegestration
    {
        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your first name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your last name")]
        public string LastName { get; set; }

        [Display(Name = "UserName")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your UserName")]
        public string UserName { get; set; }

        [Display(Name = "User ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your ID")]
        public string UserId { get; set; }

        [Display(Name = "Email Address")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your Email Address")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }

        [Display(Name = "Phone number")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your Phone number")]
        public string Phonenum { get; set; }

        [Display(Name = "Date of birth")]
        [DataType(DataType.Date)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your Date of birth")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password and password do not match")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Profile picture")]
        public string pic { get; set; }

    }
    public class ResetPasswordModel
    {
        [Display(Name = "New Password")]
        [Required(ErrorMessage = "New password required", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and confirm password does not match")]
        public string ConfirmPassword { get; set; }


        [Required]
        public string ResetCode { get; set; }
    }


}