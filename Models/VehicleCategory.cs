using EaziLease.Models.Entities;

namespace EaziLease.Models
{
    public class VehicleCategory: BaseEntity
    {
        public string CategoryName {get; set;} = string.Empty;
    }
}