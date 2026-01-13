using EaziLease.Models.Entities;

namespace EaziLease.Models
{
    public class Garage: BaseEntity
    {
        public string Name {get; set;} = string.Empty;
        public string Address {get; set;} = string.Empty;
        public string City {get; set;} = string.Empty;
        public string ContactPerson {get; set;} = string.Empty;
        public string Phone {get; set;} = string.Empty;
        public string Email {get; set;} = string.Empty;
        public bool IsPreferred {get; set;} = false;
        public string Notes {get; set;} = string.Empty;

    }
}