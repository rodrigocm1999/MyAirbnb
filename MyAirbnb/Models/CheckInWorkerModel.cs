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
        public List<string> CheckInItems { get; set; }
    }

    public class CheckInWorkerOutputModel
    {

        public List<string> CheckInItems { get; set; }

        public List<int> ItemsIndeces { get; set; }
    }


    public class CheckOutWorkerInputModel
    {
        public SelectList CheckOutItems { get; set; }
        public int RatingUser { get; set; }
    }

    public class CheckListsHelper
    {
        public static List<string> SplitFromDatabase(string str)
        {
            if (str == null) return new List<string>();
            return str.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
        }
        public static string JoinForDatabase(List<string> items, List<int> itemsIndeces)
        {
            var checkInItems = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                bool contains = itemsIndeces.Contains(i);

                if (contains) checkInItems.Add($"*{item}");
                else checkInItems.Add(item);
            }

            return string.Join("\n", checkInItems);
        }


    }
}
