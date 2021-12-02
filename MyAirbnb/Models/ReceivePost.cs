using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class ReceivePost
    {
        public string Title { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string Html { get; set; }
        public float Price { get; set; }
        public int NBeds { get; set; }
        public int NBedrooms { get; set; }
        public PropertyType Type { get; set; }
        public AvailabilityType Availability { get; set; }

        //public virtual ICollection<PostImage> PostImages { get; set; }
        public virtual ICollection<int> Comodities { get; set; } // Esta classe existe para conseguir receber os ids das comodities
    }
}
