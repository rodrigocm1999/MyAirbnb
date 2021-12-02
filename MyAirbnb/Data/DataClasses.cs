﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{

    public enum PropertyType { Home, Apartment }

    public enum AvailabilityType { Available, AlreadyRented }

    // public enum ComodityType { Wifi, Dryer, Dishwasher, WashingMachine, CoffeeMachine, TV, Fridge, AirConditioning, Bedsheets, Vaccum, Microwave, Balcony, Garage }


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
        public string Description { get; set; }
        public string Html { get; set; }
        public float Price { get; set; }
        public int NBeds { get; set; }
        public int NBedrooms { get; set; }

        public float Rating { get; set; }
        public PropertyType Type { get; set; }
        public AvailabilityType Availability { get; set; }

        public virtual ICollection<PostImage> PostImages { get; set; }
        public virtual ICollection<Comodity> Comodities { get; set; }
    }

    public class Comodity
    {
        [Key]
        public int Id { get; set; }
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
