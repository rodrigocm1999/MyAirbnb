using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class AcceptReservationWorkerInputModel
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }

        public ApplicationUser User { get; set; }
    }

    public class CheckInWorkerInputModel
    {
        public int ReservationId { get; set; }
        public List<string> CheckItems { get; set; }
        public string Notes { get; set; }

       public ApplicationUser User { get; set; }
    }

    public class CheckInWorkerOutputModel
    {
        public List<string> CheckItems { get; set; }
        public List<int> ItemsIndeces { get; set; }
        public string Notes { get; set; }
    }


    public class CheckOutWorkerInputModel
    {
        public int ReservationId { get; set; }
        public List<string> CheckItems { get; set; }

        [Range(1, 5, ErrorMessage = "Value between 1 and 5")]
        public int RatingUser { get; set; }

        public string Notes { get; set; }
        public IEnumerable<CheckOutImage> Files { get; set; }

        public ApplicationUser User { get; set; }


    }
    public class CheckOutWorkerOutputModel
    {
        public List<string> CheckItems { get; set; }
        public List<int> ItemsIndeces { get; set; }
        public int RatingUser { get; set; }
        public string Notes { get; set; }
    }

}
