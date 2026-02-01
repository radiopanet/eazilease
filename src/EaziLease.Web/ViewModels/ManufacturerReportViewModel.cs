using EaziLease.Domain.Entities;

namespace EaziLease.Web.ViewModels
{
    public class ManufacturerReportViewModel
    {
        public string Manufacturer { get; set; } = string.Empty;
        public int Total { get; set; }
        public List<Breakdown> BySupplier { get; set; } = new();
        public List<Breakdown> ByBranch { get; set; } = new();
        public List<Breakdown> ByClient { get; set; } = new();
    }
}