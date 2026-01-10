using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grupp6WebApp.Models.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace grupp6WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginVm Input { get; set; } = new();
        public void OnGet()
        {
            //Körs när sidan laddas (GET /Account/Login)
        }

        public IActionResult OnPost()
        {
            //Körs när formuläret skickas (POST)
            if (!ModelState.IsValid)
                return Page(); //stanna på samma sida och visa valideringsfel

            //DB/Identity kommer senare, nu bara navigation
            return RedirectToPage("/Index");
        }
    }
}
