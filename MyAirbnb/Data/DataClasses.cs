using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static MyAirbnb.Data.DataClassesHelper;

namespace MyAirbnb.Models
{

    public class PropertyType
    {
        public static ValueName Home = new(0, "Home");
        public static ValueName Apartment = new(1, "Apartment");
        public static List<ValueName> Types = new() { Home, Apartment };
    }

    public class AvailabilityType
    {
        public static ValueName Available = new(0, "Available");
        public static ValueName AlreadyRented = new(1, "Already Rented");
        public static List<ValueName> Types = new() { Available, AlreadyRented };
    }

    public class Post
    {
        [Key]
        public int Id { get; set; }
        public int ManagerId { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Title is too short (min 10)")]
        [MaxLength(100, ErrorMessage = "Title is too long (max 100)")]
        public string Title { get; set; }
        [Required]
        public string Address { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Price per Night")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Range(1, float.MaxValue,ErrorMessage = "Invalid Price, cannot be 0")]
        public decimal Price { get; set; }

        [Display(Name = "Number of Beds")]
        [Range(1, 50, ErrorMessage = "Invalid number of Bedrooms (1 - 50)")]
        public int NBeds { get; set; }

        [Display(Name = "Number of Bedrooms")]
        [Range(1, 50, ErrorMessage = "Invalid number of Bedrooms (1 - 50)")]
        public int NBedrooms { get; set; }

        public float Rating { get; set; }

        public int PropertyType { get; set; }
        public int AvailabilityType { get; set; }

        public virtual IList<PostImage> PostImages { get; set; }
        public virtual IList<Comodity> Comodities { get; set; }
    }

    public class Comodity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
    }


    public class PostImage
    {
        [Key]
        public int Id { get; set; }
        public int PostId { get; set; }
        public string FilePath { get; set; }

    }
    public class Manager
    {

        [Key]
        public int Id { get; set; }


        //public virtual ICollection<Employees> Employees { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}
