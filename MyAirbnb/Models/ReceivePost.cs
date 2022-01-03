using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class ReceivePost
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int NBeds { get; set; }
        public int NBedrooms { get; set; }
        public int SpaceCategoryId { get; set; }

        public virtual IList<IFormFile> PostImages { get; set; }
        
        public virtual ICollection<int> Comodities { get; set; } // Esta classe existe para conseguir receber os ids das comodities
    }
}
