using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class EditPost
    {
        public Post Post { get; set; }
        public IList<Comodity> Comodities { get; set; }

        public SelectList SpaceCategories { get; set; }
    }
}
