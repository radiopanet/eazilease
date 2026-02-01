using System.ComponentModel.DataAnnotations;

namespace EaziLease.Web.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

    }
}