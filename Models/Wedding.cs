using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
///////////////////////////////////////////////////////////////////////
        [Key]
        public int WeddingId { get; set; }
///////////////////////////////////////////////////////////////////////
        [Required(ErrorMessage = "Wedder one is required.")]
        [MinLength(2, ErrorMessage = "Wedder one must be more than 2 characters long.")]
        public string WedderOne { get; set; }
///////////////////////////////////////////////////////////////////////
        [Required(ErrorMessage = "Wedder two is required.")]
        [MinLength(2, ErrorMessage = "Wedder two must be more than 2 characters long.")]
        public string WedderTwo { get; set; }
///////////////////////////////////////////////////////////////////////
        [Required(ErrorMessage = "Wedding date is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "Wedding date is required.")]
        public DateTime WeddingDate { get; set; }
///////////////////////////////////////////////////////////////////////
        [Required(ErrorMessage = "An address is required.")]
        public string Address {get;set;}
///////////////////////////////////////////////////////////////////////
        public int GuestCount {get;set;}
        public User Creator {get;set;}
        public List<Associations> Guests {get;set;}
///////////////////////////////////////////////////////////////////////
    
        // We can provide some hardcoded default values like so:
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
///////////////////////////////////////////////////////////////////////
    }
} 