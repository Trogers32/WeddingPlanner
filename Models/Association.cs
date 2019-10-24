using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WeddingPlanner.Models
{
    public class Associations
    {
///////////////////////////////////////////////////////////////////////
        [Key]
        public int AssociationId { get; set; }
///////////////////////////////////////////////////////////////////////
        public int UserId { get; set; }
///////////////////////////////////////////////////////////////////////
        public int WeddingId { get; set; }
///////////////////////////////////////////////////////////////////////
        public User User {get;set;}
        public Wedding Wedding {get;set;}
///////////////////////////////////////////////////////////////////////
        // We can provide some hardcoded default values like so:
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }
}
