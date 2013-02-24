using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcSpelling.Models
{
    public class BiographyViewModel
    {
        [Required]
        [Display(Name = "First Name", Prompt = "Enter First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name", Prompt = "Enter Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name="e-Mail Address", Prompt="Enter Email")]
        [EmailAddress]
        public string Email {get; set; }
        [Display(Name = "Birth Date", Prompt = "Enter Birth Date")]
        public DateTime BirthDate { get; set; }
        [AllowHtml]
        [Display(Name = "Biography", Prompt = "Tell us about yourself")]
        [UIHint("RichTextEditor")]
        public string Biography { get; set; }
        [Display(Name = "Comments", Prompt = "Enter any additional comments")]
        [UIHint("Multiline")]
        public string Comment { get; set; }


    }
}