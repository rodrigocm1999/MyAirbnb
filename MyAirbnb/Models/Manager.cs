using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class Manager
    {

        public int Id { get; set; }


        //public virtual ICollection<Employees> Employees { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}
