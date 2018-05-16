using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReviseApplication.Models
{
    public class Requirements
    {
        [Required]
        [Display(Name = "Requirement")]
        public List<category> Req { get; set; }
    }
}