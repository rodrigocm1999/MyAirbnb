using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class ManagerViewModel
    {
        public ApplicationUser Manager { get; set; }

        public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
    }
}
