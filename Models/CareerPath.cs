namespace CareerPathPlannerAI.Models;

public class CareerPath
{
    public string Title { get; set; } = string.Empty;
    
    public List<string> RequiredSkills { get; set; } = new();
    
    public List<string> RecommendedCertifications { get; set; } = new();
    
    public string Description { get; set; } = string.Empty;
    
    public decimal AverageSalary { get; set; }
    
    public string JobMarketOutlook { get; set; } = string.Empty;
}

public class CareerAnalysisResult
{
    public List<CareerPath> RecommendedPaths { get; set; } = new();
    
    public List<string> SkillGaps { get; set; } = new();
    
    public List<string> RecommendedCourses { get; set; } = new();
    
    public LearningRoadmap LearningRoadmap { get; set; } = new();
}

public class LearningRoadmap
{
    public List<LearningStep> Steps { get; set; } = new();
    
    public int EstimatedTimeInMonths { get; set; }
}

public class LearningStep
{
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public List<string> Resources { get; set; } = new();
    
    public int EstimatedTimeInWeeks { get; set; }
}