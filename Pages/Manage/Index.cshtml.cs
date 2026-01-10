using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace grupp6WebApp.Pages.Manage
{
    public class IndexModel : PageModel
    {
        public string? StatusMessage { get; private set; }
        public void OnGet()
        {
        }
    }
}
