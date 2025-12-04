using EaziLease.Models.Entities;

namespace EaziLease.Models
{
    public class Supplier : BaseEntity
    {
        public string Name {get; set;} = string.Empty;
        public string? ContactPerson {get; set;}
        public string? ContactEmail {get; set;}
        public string? ContactPhone {get; set;}
        public string? Address {get; set;}
        public string? City {get; set;}
        public string? Country {get; set;}
        public decimal? AverageRating {get; set;}

        public virtual ICollection<Vehicle> Vehicles {get; set;} = null!;
    }
}