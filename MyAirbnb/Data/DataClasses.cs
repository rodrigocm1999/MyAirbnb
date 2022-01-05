using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyAirbnb.Models
{

    /*Random thoughts--------------------------------------------------

    ------------------------------------------------------------------- */

    //Non Specific
    //TODO index- conseguir fazer pesquisa por endereço, talvez também ter filtros para escolher as camas e 
    //TODO fazer pedir as reservas- tem de verificar a existencia de outras nessas datas e também mostrar as datas disponivies


    //Client
    //TODO os clientes a terminarem a estadia, podem dar um comentário e um rating ao post

    //Manager
    //TODO manager criar workers - deixar bonito
    //TODO manager gerir as checklists - deixar bonito


    //Worker
    //TODO os worker podem ver os comentário e ratings dos posts
    //TODO os workers podem ver e dar rating aos clientes, para poder decidir se aceitam a reserva ou não
    //TODO worker "entregar o espaço"
    //TODO worker "receber o espaço"
    //TODO reservas podem ou não ser aceites, por isso é um pedido de reserva
    //TODO quando fizer a reserva os workers terão de preencher a checklist da categoria do edificio para a entrega aos clientes
    // e depois ter a cena de "terminar" a reserva em que teem de preenchar a outra checklist


    //Admin
    //TODO gerir managers e workers
    //TODO ver lista de 
    //TODO gerir clientes
    //TODO gerir SpaceCategories (provavelmente conseguir alterar o nome e adicionar novo, mas apagar so ia dar merda se já existissem posts com essa categoria e reservas também)


    [Index(nameof(WorkerId))]
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string WorkerId { get; set; }


        [Required]
        [MinLength(10, ErrorMessage = "Title is too short (min 10)")]
        [MaxLength(100, ErrorMessage = "Title is too long (max 100)")]
        public string Title { get; set; } = "";
        [Required]
        public string Address { get; set; } = "";

        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = "";

        [Display(Name = "Price per Night")]
        //[DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Range(1, float.MaxValue, ErrorMessage = "Invalid Price, cannot be 0")]
        [DataType(DataType.Currency)]
        public int Price { get; set; }

        [Display(Name = "Number of Beds")]
        [Range(1, 50, ErrorMessage = "Invalid number of Bedrooms (1 - 50)")]
        public int NBeds { get; set; }

        [Display(Name = "Number of Bedrooms")]
        [Range(1, 50, ErrorMessage = "Invalid number of Bedrooms (1 - 50)")]
        public int NBedrooms { get; set; }

        public float Rating { get; set; }


        [Display(Name = "Space Category")]
        public int SpaceCategoryId { get; set; }
        public int AvailabilityType { get; set; }

        public bool Hidden { get; set; } = false;

        public virtual IList<PostImage> PostImages { get; set; }
        public virtual IList<Comodity> Comodities { get; set; }
        public virtual IList<Comment> Comments { get; set; }
    }

    [Index(nameof(PostId))]
    [Index(nameof(UserId))]
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }

        public int ReserveId { get; set; }

        public int UserId { get; set; }

        public virtual IdentityUser User { get; set; } //TODO verificar se isto vai preencher sozinho

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

    [Index(nameof(PostId))]
    public class PostImage
    {
        [Key]
        public int Id { get; set; }
        public int PostId { get; set; }
        [Required]
        [DataType(DataType.ImageUrl)]
        public string FilePath { get; set; }
    }

    public class SpaceCategory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }

    [Index(nameof(ManagerId))]
    public class CheckList
    {
        [Key]
        public int Id { get; set; }
        public string ManagerId { get; set; }
        public int SpaceCategoryId { get; set; }

        public string CheckInItems { get; set; } = ""; // Separated by \n
        public string CheckOutItems { get; set; } = ""; // Separated by \n
    }


    [Index(nameof(PostId))]
    [Index(nameof(UserId))]
    [Index(nameof(WorkerId))]
    public class Reservation
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }

        public string WorkerId { get; set; }
        public virtual Worker Worker { get; set; }
        
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

        public int RatingUser { get; set; }
        public int RatingPost { get; set; }
        public int Price { get; set; }

        public string CheckInItems { get; set; } // Separated by \n, to indicate if was checked contains '*' at the start
        public string CheckOutItems { get; set; } // Separated by \n, to indicate if was checked contains '*' at the start


        public ReservationState State { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int TotalPrice { get; set; }
    }

    public enum ReservationState { Pending, ToCheckIn, OnGoing, ToCheckOut, Finished }

    //This Ids are of the UserId of the logged user
    //The manager has multiple workers and is also a worker and all of the worker accounts created by it are also workers
    public class Manager
    {

        [Key]
        public string Id { get; set; }

        public virtual ICollection<CheckList> CheckLists { get; set; } = new List<CheckList>();
        public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
    }

    [Index(nameof(ManagerId))]
    public class Worker
    {
        [Key]
        public string Id { get; set; }

        public string ManagerId { get; set; }
        public virtual Manager Manager { get; set; }

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }

}
