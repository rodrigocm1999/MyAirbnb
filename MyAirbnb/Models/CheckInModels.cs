using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{

    public class SimpleUserModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public float? Rating { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

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
    //TODO fazer upload de imagens no checkout de eventuais danos
    public class CheckOutWorkerOutputModel
    {
        public List<string> CheckItems { get; set; }
        public List<int> ItemsIndeces { get; set; }
        public int RatingUser { get; set; }
        public string Notes { get; set; }
    }

    public class CheckedText
    {
        public string Text { get; set; }
        public bool Checked { get; set; }
    }

    public class ChecklistsHelper
    {
        public static List<string> SplitItems(string str)
        {
            if (str == null) return new List<string>();
            return str.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
        }

        public static List<string> Trim(List<string> items)
        {
            var list = new List<string>();
            foreach (var item in items)
            {
                var trimmed = item.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    list.Add(trimmed);
            }
            return list;
        }

        public static string JoinForDatabase(List<string> items, List<int> checkedItemsIndeces)
        {
            if (checkedItemsIndeces == null) checkedItemsIndeces = new List<int>(0);

            var checkInItems = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (checkedItemsIndeces.Contains(i))
                    checkInItems.Add($"*{item}");
                else checkInItems.Add(item);
            }

            return string.Join("\n", checkInItems);
        }
        public static string JoinToStr(List<string> items)
        {
            if (items == null) return null;
            return string.Join("\n", items);
        }

        public static List<CheckedText> SplitIntoCheckedList(string str)
        {
            var checkList = new List<CheckedText>();
            if (str == null) return checkList;

            var list = str.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (var l in list)
                checkList.Add(new CheckedText { Checked = true, Text = l.StartsWith("*") ? l[1..] : l });
            return checkList;
        }



    }
}
