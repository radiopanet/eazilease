namespace EaziLease.Models;

public enum TerminationReason
{
    NormalEnd, //Lease completed as planned.
    Accident, //Vehicle involved in an accident.
    MechanicalIssue, //Mechanical failure during lease.
    ClientRequest, //Client Requested early termination.
    Other //Catch all.
}