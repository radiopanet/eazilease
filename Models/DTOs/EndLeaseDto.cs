using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc;

namespace EaziLease.Models.ViewModels
{
    public class EndLeaseDto
    {
        [Required]
        public DateTime ReturnDate {get; set;} = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        public decimal FinalOdometerReading {get; set;}
        public string? ReturnNotes {get; set;}
        public decimal? PenaltyFee {get; set;}
        public TerminationReason TerminationReason {get; set;}
        public string? TerminationNotes {get; set;}
    }
}