using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class CheckInWorkerInputModel
    {
        public int ReservationId { get; set; }
        public IEnumerable<string> CheckInItems { get; set; }
    }

    public class CheckInWorkerOutputModel
    {
        public IEnumerable<string> CheckInItems { get; set; }
    }


    public class CheckOutWorkerInputModel
    {
        public SelectList CheckOutItems { get; set; }
        public int RatingUser { get; set; }
    }
}
