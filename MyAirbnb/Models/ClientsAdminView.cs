using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class ClientsAdminView
    {
        public ApplicationUser Client { get; set; }

        [Display(Name = "Reservations")]
        public virtual IQueryable<Reservation> Reservations { get; set; }
    }
}
