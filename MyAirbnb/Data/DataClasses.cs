using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public string WorkerId { get; set; }


        [Required]
        [MinLength(10, ErrorMessage = "Title is too short (min 10)")]
        [MaxLength(100, ErrorMessage = "Title is too long (max 100)")]
        public string Title { get; set; }
        [Required]
        public string Address { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Price per Night")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Range(1, float.MaxValue, ErrorMessage = "Invalid Price, cannot be 0")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
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
        public virtual IList<Comment> Comments { get; set; }
    }

    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }

        [Required]
        public virtual IdentityUser User { get; set; }

        [Required]
        public string Text { get; set; }
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
        [Required]
        [DataType(DataType.ImageUrl)]
        public string FilePath { get; set; }

    }


    // Ainda vai ser preciso guardar as reservas, 
    // guardar a checklist, provavelmente basta meter ter strings agarradas a um manager que são cada campo da checklist
    // resultado da checklist da reserva
    // wtf is this ---- Gestão das categorias de espaços/alojamento a disponibilizar, talvez seja preciso alterar o Enum do PropertyType para uma tabela



    //This Ids are of the UserId of the logged user
    //The manager has multiple workers and is also a worker and all of the worker accounts created by it are also workers
    public class Manager
    {

        [Key]
        public string Id { get; set; }
        public virtual ICollection<Worker> Workers { get; set; }
    }

    public class Worker
    {

        [Key]
        public string Id { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}
