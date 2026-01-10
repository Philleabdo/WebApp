using grupp6WebApp.Models.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grupp6WebApp.Pages.Manage
{
    public class ChangePasswordModel : PageModel
    {
        [BindProperty]
        public ChangePasswordVm Input { get; set; } = new();
        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            //Demo: ingen riktig lagring ännu
            TempData["StatusMessage"] = "Lösenord uppdaterat (demo).";

            return RedirectToPage("/Manage/Index");
        }
    }
}
