using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyAirbnb.Areas.Identity.Pages.Account.Manage
{
    public static class ManageNavPages
    {
        public static string Index => "Index";
        public static string ChangePassword => "ChangePassword";

        public static string Client => "Client";
        public static string Worker => "Worker";
        public static string Manager => "Manager";
        public static string Admin => "Admin";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        public static string ClientNavClass(ViewContext viewContext) => PageNavClass(viewContext, Client);
        public static string WorkerNavClass(ViewContext viewContext) => PageNavClass(viewContext, Worker);
        public static string ManagerNavClass(ViewContext viewContext) => PageNavClass(viewContext, Manager);
        public static string AdminNavClass(ViewContext viewContext) => PageNavClass(viewContext, Admin);


        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
