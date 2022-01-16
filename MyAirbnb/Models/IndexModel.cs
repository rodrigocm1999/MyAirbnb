using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class IndexModel
    {

        public IEnumerable<Post> Posts { get; set; }

        public SelectList SpaceCategories { get; set; }

        public int CurrentPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public IndexSearch Search { get; set; }
    }

    public class IndexSearch
    {
        public string City { get; set; }
        public int? Nbeds { get; set; }
        public int? NRooms { get; set; }
        public SpaceCategory? SpaceCategory { get; set; }

    }
}
