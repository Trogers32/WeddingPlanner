using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class User
    {
///////////////////////////////////////////////////////////////////////
        [Key]
        public int UserId { get; set; }
///////////////////////////////////////////////////////////////////////
        [Required(ErrorMessage = "First name is required.")]
        [MinLength(2, ErrorMessage = "First name must be more than 2 characters long.")]
        public string FName { get; set; }
///////////////////////////////////////////////////////////////////////
        [Required(ErrorMessage = "Last name is required.")]
        [MinLength(2, ErrorMessage = "Last name must be more than 2 characters long.")]
        public string LName { get; set; }
///////////////////////////////////////////////////////////////////////
        [Required(ErrorMessage = "An email is required.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        public string Email { get; set; }
///////////////////////////////////////////////////////////////////////
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [Required(ErrorMessage = "A password is required."), DataType(DataType.Password)]
        public string Password { get; set; }
///////////////////////////////////////////////////////////////////////
    
        // We can provide some hardcoded default values like so:
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
///////////////////////////////////////////////////////////////////////
    public List<Associations> Weddings {get;set;}
///////////////////////////////////////////////////////////////////////
        // Will not be mapped to your users table!
        [NotMapped]
        [Compare("Password", ErrorMessage = "Passwords need to match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword {get;set;}


    }
} 