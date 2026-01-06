using System.ComponentModel.DataAnnotations;
namespace EaziLease.Models.ViewModels
{
    public class RateOverrideRequestViewModel
    {
        public string VehicleId { get; set; } = string.Empty;

        [Display(Name = "Vehicle Registration")]
        public string VehicleRegistration { get; set; } = string.Empty; // For display only

        [Display(Name = "Current Daily Rate")]
        [DataType(DataType.Currency)]
        public decimal CurrentDailyRate { get; set; } // Display only - from Vehicle

        [Required(ErrorMessage = "New daily rate is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be greater than 0")]
        [Display(Name = "Requested New Daily Rate")]
        [DataType(DataType.Currency)]
        public decimal RequestedDailyRate { get; set; }

        [Required(ErrorMessage = "Reason for override is required")]
        [StringLength(500, MinimumLength = 10, 
            ErrorMessage = "Reason must be between 10 and 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Make this override permanent")]
        public bool IsPermanent { get; set; }

        [Display(Name = "Effective From")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveFrom { get; set; }

        [Display(Name = "Effective To")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        // Client-side validation helper properties
        public DateTime MinimumEffectiveDate => DateTime.UtcNow.Date;

        // Optional: validation attribute on model level (can be added later)
    }
}