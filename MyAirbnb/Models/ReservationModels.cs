using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class ReservationModel
    {
        public int Id { get; set; }
        public Post Post { get; set; }

        public bool CanComment { get; set; } = false;
        public Comment Comment { get; set; }

        public int? RatingUser { get; set; }
        public int? RatingPost { get; set; }
        public int TotalPrice { get; set; }


        public ReservationState State { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }


    public class ReservationCommentModel
    {
        public int Id { get; set; }
        public Post Post { get; set; }

        public int NumberOfDays { get; set; }
        public int TotalPrice { get; set; }

        [Range(1, 5, ErrorMessage = "Rating needs to be between 1 and 5")]
        public int RatingPost { get; set; }

        [MaxLength(250, ErrorMessage = "Max comment length is 250")]
        public string Comment { get; set; }
    }

    public class ReservationCreationModel : IValidatableObject
    {
        public int PostId { get; set; }

        [DataType(DataType.Currency)]
        public int Price { get; set; }

        [Display(Name = "Start of Reservation")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;
        [DataType(DataType.Date)]
        [Display(Name = "End of Reservation")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(5);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            int result = DateTime.Compare(StartDate, EndDate);
            if (result >= 0)
                yield return new ValidationResult("End date must be after start date!");
        }
    }


    public class ReservationDetails
    {
        public ApplicationUser User { get; set; }
        public Worker Worker { get; set; }
        public Post Post { get; set; }
        public Comment Comment { get; set; }

        [Display(Name = "User Rating")]
        public int? RatingUser { get; set; }
        [Display(Name = "Post Rating")]
        public int? RatingPost { get; set; }

        [Display(Name = "Price Per Night")]
        public int Price { get; set; }
        [Display(Name = "Total Price")]
        public int TotalPrice { get; set; }


        public List<CheckedText> CheckInItems { get; set; }
        public List<CheckedText> CheckOutItems { get; set; }

        public string CheckInNotes { get; set; }
        public string CheckOutNotes { get; set; }
        public virtual ICollection<CheckOutImage> CheckOutImages { get; set; }

        public ReservationState State { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

    }
}
