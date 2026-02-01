using System.ComponentModel.DataAnnotations;

namespace EaziLease.Domain.Entities
{
    public class Supplier : BaseEntity
    {
        [Display(Name ="Supplier Name")]
        public string Name {get; set;} = string.Empty;
        [Display(Name ="Contact Person")]
        public string? ContactPerson {get; set;}
        [Display(Name="Email Address")]
        public string? ContactEmail {get; set;}
        [Display(Name="Phone Number")]
        public string? ContactPhone {get; set;}
        [Display(Name="Address")]
        public string? Address {get; set;}
        [Display(Name="City")]
        public string? City {get; set;}
        [Display(Name="Country")]
        public string? Country {get; set;}
        [Display(Name="Average Rating")]
        public decimal? AverageRating {get; set;}

        public virtual ICollection<Vehicle>? Vehicles {get; set;} = null!;
    }
}