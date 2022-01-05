using MyAirbnb.Models;
using System;
using System.Collections.Generic;
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
                new SpaceCategory{Name = "Home" }
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
            };

            if (context.Posts.FirstOrDefault() == null)
            {
                context.Comodities.AddRange(comodities);
            }

            var worker = context.Workers.FirstOrDefault();
            var workerId = worker.Id;

            var posts = new List<Post>
            {
                new Post{Address="Rua do aço",Description="Este apartamente tem aço",Title="Casa para 3 nabos",
                    NBedrooms=1,NBeds=3,Price=400,Rating=0,SpaceCategoryId = apartementId,WorkerId = workerId},
                new Post{Address="Rua ao lado da rua do aço",Description="Esta casa não vale nada",Title="Casa a cair aos bocados",
                    NBedrooms=2,NBeds=1,Price=20,Rating=3,SpaceCategoryId=apartementId,WorkerId = workerId},
                new Post{Address="Teste 25 do dia 32",Description="Nem sei o que dizer",Title="Titulo não",
                    NBedrooms=5,NBeds=10,Price=300,Rating=0,SpaceCategoryId=homeId,WorkerId = workerId},
                new Post{Address="Rua do aço",Description="Este apartamente tem aço",Title="Casa para 3 nabos",
                    NBedrooms=10,NBeds=3,Price=400,Rating=0,SpaceCategoryId = homeId,WorkerId = workerId},
                new Post{Address="Rua do aço",Description="Este apartamente tem aço",Title="Casa para 3 nabos",
                    NBedrooms=1,NBeds=1,Price=299,Rating=0,SpaceCategoryId = apartementId, WorkerId = workerId},
            };

            Random rand = new Random(4321);

            var presetImages = new[] {
               "/postimages/357279596.webp",
               "/postimages/357279612.webp",
               "/postimages/357279628.webp",
               "/postimages/357279690.webp",
               "/postimages/357279736.webp",
               "/postimages/300000548.webp",
               "/postimages/32379616.webp"};

            foreach (var p in posts)
            {
                p.PostImages = new List<PostImage>();
                var amount = rand.Next(2, 6);
                for (int i = 0; i < amount; i++)
                {
                    var filePath = presetImages[rand.Next(presetImages.Length)];
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
