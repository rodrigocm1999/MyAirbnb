﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{

    public enum PropertyType { Home, Apartment }

    public enum AvailabilityType { Available, AlreadyRented }

    public enum ComodityType { Wifi, Dryer, Dishwasher, WashingMachine, CoffeeMachine, TV, Fridge, AirConditioning, Bedsheets, Vaccum, Microwave, Balcony, Garage }

    public class Post
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string Html { get; set; }
        public float Price { get; set; }
        public int NBeds { get; set; }
        public int NBedrooms { get; set; }
        public float Rating { get; set; }
        public PropertyType Type { get; set; }
        public AvailabilityType Availability { get; set; }

        public virtual ICollection<string> PostImages { get; set; }
        public virtual ICollection<ComodityType> Comodities { get; set; }
    }
}