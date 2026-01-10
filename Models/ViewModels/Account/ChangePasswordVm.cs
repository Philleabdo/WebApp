using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace grupp6WebApp.Models.ViewModels.Account
{
    public class ChangePasswordVm
    {
        [Required(ErrorMessage = "Nuvarande lösenord krävs")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nytt lösenord krävs")]
        [MinLength(6, ErrorMessage = "Minst 6 tecken")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bekräfta nya lösenordet")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Lösenorden matchar inte")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}