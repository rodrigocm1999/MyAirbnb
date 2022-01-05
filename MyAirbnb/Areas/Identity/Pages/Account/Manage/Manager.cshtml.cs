using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyAirbnb.Areas.Identity.Pages.Account.Manage
{
    public class ManagerModel : PageModel
    {

        public IActionResult OnGet()
        {
            return Page();
        }

    }
}
