namespace EaziLease.Domain.Enums
{
    public enum InsuranceClaimStatus
    {
        NA, //Not applicable
        Pending, //Claim submitted
        Approved, //Claim paid
        Rejected, //Claim denied
        Partial //Partial payment
    }
}