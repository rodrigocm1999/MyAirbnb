﻿using Microsoft.EntityFrameworkCore;
using MyAirbnb.Models;
using MyAirbnb.Other;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Data
{
    public class TestData
    {

        public static async Task FillTestDataAsync(ApplicationDbContext context)
        {
            var spaceCategories = new List<SpaceCategory>(){
                new SpaceCategory{Name = "Apartment" },
                new SpaceCategory{Name = "Home" },
                new SpaceCategory{Name = "Garage" },
                new SpaceCategory{Name = "Shared House" },
            };

            if (context.SpaceCategories.FirstOrDefault() == null)
            {
                context.SpaceCategories.AddRange(spaceCategories);
                await context.SaveChangesAsync();
            }
            else
            {
                spaceCategories = context.SpaceCategories.ToList();
            }

            var apartementId = spaceCategories[0].Id;
            var homeId = spaceCategories[1].Id;
            var garageId = spaceCategories[2].Id;
            var sharedHouseId = spaceCategories[3].Id;



            var manager = context.Managers.Include(e => e.CheckLists).FirstOrDefault();
            if (manager.CheckLists.Count == 0)
            {
                manager.CheckLists.Add(new CheckList { SpaceCategoryId = apartementId, CheckInItems = "Clean\nGive keys to the home", CheckOutItems = "Check rooms state\nCall neighbor to verify existence of trouble\nGet the keys back" });
                manager.CheckLists.Add(new CheckList { SpaceCategoryId = homeId, CheckInItems = "Clean\nGive keys to the apartment", CheckOutItems = "Check bathroom state\nCall neighbor to verify existence of trouble\nGet the keys back" });
                await context.SaveChangesAsync();
            }

            var comodities = new List<Comodity>(){
                new Comodity{Name = "Wifi" },
                new Comodity{Name = "Dryer" },
                new Comodity{Name = "Dishwasher" },
                new Comodity{Name = "Washing Machine" },
                new Comodity{Name = "Coffee Machine" },
                new Comodity{Name = "TV" },
                new Comodity{Name = "Fridge" },
                new Comodity{Name = "Air Conditioning" },
                new Comodity{Name = "Bedsheets" },
                new Comodity{Name = "Vaccum" },
                new Comodity{Name = "Microwave" },
                new Comodity{Name = "Balcony" },
                new Comodity{Name = "Garage" },
                new Comodity{Name = "Pool" },
                new Comodity{Name = "Jacuzzi" },
            };

            if (context.Posts.FirstOrDefault() == null)
            {
                context.Comodities.AddRange(comodities);
            }

            var worker = context.Workers.FirstOrDefault();
            var workerId = worker.Id;

            var posts = new List<Post>
            {
                new Post{Address="Rua nº3",Description="Este apartamente é indicado para pessoas sem filhos",Title="Casa do precipício",
                    NBedrooms=1,NBeds=3,Price=400,SpaceCategoryId = apartementId,WorkerId = workerId,City ="Lisboa"},
                new Post{Address="Rua da esquina de baixo",Description="Este apartamento não vale nada\nE chega bem",Title="Casa a cair aos bocados",
                    NBedrooms=2,NBeds=1,Price=20,SpaceCategoryId=apartementId,WorkerId = workerId,City ="Coimbra"},
                new Post{Address="Rua da esquina da esquerda",Description="Nem sei o que dizer, é uma casa razoável\n\tE chega bem",Title="Título representador da casa",
                    NBedrooms=5,NBeds=10,Price=300,SpaceCategoryId=homeId,WorkerId = workerId,City ="Lisboa"},
                new Post{Address="Rua da estátua caida",Description="Esta casa",Title="Casa para 3.5",
                    NBedrooms=10,NBeds=3,Price=400,SpaceCategoryId = homeId,WorkerId = workerId,City ="Coimbra"},
                new Post{Address="Rua do sinal de STOP",Description="Este apartamente tem aço",Title="Somewenfsvdomo",
                    NBedrooms=1,NBeds=1,Price=299,SpaceCategoryId = apartementId, WorkerId = workerId,City ="Porto"},
                 new Post{Address="Local da merenda",Description="Paredes de esferotive!",Title="Casa apresentável para 2 pessoas",
                    NBedrooms=1,NBeds=2,Price=299,SpaceCategoryId = apartementId, WorkerId = workerId,City ="Coimbra"},
                 new Post{Address="Rua das Garagens",Description="cabem 12 pessoas deitadas encostadas, mas bem compactadas leva 18\n e em pé até dá para 30",Title="Garagem para dormir no chão, cabem 12 pessoas",
                    NBedrooms=1,NBeds=12,Price=299,SpaceCategoryId = garageId, WorkerId = workerId,City ="Cantanhede"},
                 new Post{Address="BigHouse Street",Description="Este apartamente tem aço",Title="Casa apresentável para 2 pessoas",
                    NBedrooms=2,NBeds=2,Price=299,SpaceCategoryId = sharedHouseId, WorkerId = workerId,City ="Cantanhede"},
            };

            var rand = new Random(4321);

            var d = new DirectoryInfo(Path.Combine("wwwroot", App.PostImagesFolderName));
            var files = d.GetFiles();

            var presetImages = new List<string>();
            for (int i = 0; i < files.Length; i++)
                presetImages.Add("/" + App.PostImagesFolderName + "/" + files[i].Name);

            foreach (var p in posts)
            {
                p.PostImages = new List<PostImage>();
                var amount = rand.Next(2, 6);
                for (int i = 0; i < amount; i++)
                {
                    var filePath = presetImages[rand.Next(presetImages.Count)];
                    p.PostImages.Add(new PostImage() { FilePath = filePath });
                }
            }

            if (context.Posts.FirstOrDefault() == null)
            {
                foreach (var p in posts)
                {
                    p.Comodities = new List<Comodity>();
                    var amount = rand.Next(5, 10);
                    for (int i = 0; i < amount; i++)
                    {
                        var randIndex = rand.Next(comodities.Count);
                        p.Comodities.Add(comodities[randIndex]);
                    }
                }

                context.Posts.AddRange(posts);
                await context.SaveChangesAsync();
            }
        }
    }
}
