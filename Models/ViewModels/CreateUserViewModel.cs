using System.ComponentModel.DataAnnotations;
namespace EaziLease.Models.ViewModels;

public class CreateUserViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;
}