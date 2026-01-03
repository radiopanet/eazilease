namespace EaziLease.Models.ViewModels;

public class UserVeiwModel
{
    public string Id {get; set;} = string.Empty;
    public string? Email {get; set;}
    public string? FullName {get; set;}
    public IList<string> Roles {get; set;} = new List<string>();
    public bool CanElevate {get; set;}
}