using System.ComponentModel.DataAnnotations;

namespace grupp6WebApp.Models.ViewModels.Account
{
    public class RegisterVm
    {
        [Required(ErrorMessage = "Förnamn är obligatoriskt")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Efternamn är obligatoriskt")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adress är obligatoriskt")]
        public string Address { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required(ErrorMessage = "E-post är obligatoriskt")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lösenord är obligatoriskt")]
        [MinLength(6, ErrorMessage = "Minst 6 tecken")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bekräfta lösenord")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Lösenorden matchar inte")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
