using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class Wrapper
    {
        public User user;
        public List<User> users;
        public Wedding wedding;
        public List<Wedding> weddings;
    }
}