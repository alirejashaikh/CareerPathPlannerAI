using System.ComponentModel.DataAnnotations;

namespace CareerPathPlannerAI.Models;

public class UserProfile
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public List<string> Skills { get; set; } = new();

    [Required]
    public Education Education { get; set; } = new();

    [Required]
    public List<string> CareerGoals { get; set; } = new();
}

public class Education
{
    [Required]
    public string Degree { get; set; } = string.Empty;

    [Required]
    public string Field { get; set; } = string.Empty;

    public int YearCompleted { get; set; }

    public List<string> Certifications { get; set; } = new();
}