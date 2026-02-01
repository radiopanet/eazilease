using EaziLease.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EaziLease.Application.DTOs
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
        public AccidentDriverResponsibility AccidentDriverResponsibility {get; set;} = AccidentDriverResponsibility.Unknown;
    }
}