using System.ComponentModel.DataAnnotations;

namespace MasteringEfDemo;

public enum DemoCommandOptions
{
    [Display(Name = "List Commands")]
    ListCommands = 1,
    [Display(Name = "Change Tracking")]
    ChangeTracking = 2,
    [Display(Name = "Projections")]
    Projections = 3,
    [Display(Name = "N+1 Queries")]
    NPlusQuery = 4,
    [Display(Name = "Over Inclusions")]
    OverInclusions = 5,
    [Display(Name = "Paged Results")]
    PagedResults = 6,
    [Display(Name = "Split Query Example")]
    SplitQuery = 7,
    [Display(Name = "Exit")]
    Exit = 99
}