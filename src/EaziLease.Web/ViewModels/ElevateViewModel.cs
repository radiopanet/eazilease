using System.ComponentModel.DataAnnotations;

namespace EaziLease.Web.ViewModels
{
    public class ElevateViewModel
    {
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}