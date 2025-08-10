using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CareerPathPlannerAI.Models;

namespace CareerPathPlannerAI.Services;

public interface IPdfGenerationService
{
    byte[] GenerateCareerReport(CareerAnalysisResult result, UserProfile userProfile);
}

public class PdfGenerationService : IPdfGenerationService
{
    public byte[] GenerateCareerReport(CareerAnalysisResult result, UserProfile userProfile)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                
                page.Content().Element(container =>
                {
                    ComposeContent(container, result, userProfile);
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Career Path Analysis Report")
                    .FontSize(20)
                    .SemiBold();
                
                column.Item().Text($"Generated on {DateTime.Now:d MMMM yyyy}")
                    .FontSize(10);
            });
        });
    }

    private void ComposeContent(IContainer container, CareerAnalysisResult result, UserProfile userProfile)
    {
        container.Column(column =>
        {
            // User Profile Section
            column.Item().PaddingVertical(10).Column(col =>
            {
                col.Item().Text("User Profile").FontSize(16).SemiBold();
                col.Item().Text($"Name: {userProfile.Name}");
                col.Item().Text($"Skills: {string.Join(", ", userProfile.Skills)}");
                col.Item().Text($"Education: {userProfile.Education.Degree} in {userProfile.Education.Field}");
                if (userProfile.Education.Certifications.Any())
                {
                    col.Item().Text($"Certifications: {string.Join(", ", userProfile.Education.Certifications)}");
                }
            });

            // Career Paths Section
            column.Item().PaddingVertical(10).Column(col =>
            {
                col.Item().Text("Recommended Career Paths").FontSize(16).SemiBold();
                foreach (var path in result.RecommendedPaths)
                {
                    col.Item().Border(1).Padding(10).Column(pathCol =>
                    {
                        pathCol.Item().Text(path.Title).SemiBold();
                        pathCol.Item().Text($"Description: {path.Description}");
                        pathCol.Item().Text($"Required Skills: {string.Join(", ", path.RequiredSkills)}");
                        pathCol.Item().Text($"Recommended Certifications: {string.Join(", ", path.RecommendedCertifications)}");
                        pathCol.Item().Text($"Average Salary: ${path.AverageSalary:N0}");
                        pathCol.Item().Text($"Job Market Outlook: {path.JobMarketOutlook}");
                    });
                    col.Item().Height(10);
                }
            });

            // Skill Gaps Section
            column.Item().PaddingVertical(10).Column(col =>
            {
                col.Item().Text("Skill Gaps").FontSize(16).SemiBold();
                col.Item().Text(string.Join(", ", result.SkillGaps));
            });

            // Learning Roadmap Section
            column.Item().PaddingVertical(10).Column(col =>
            {
                col.Item().Text("Learning Roadmap").FontSize(16).SemiBold();
                col.Item().Text($"Estimated Duration: {result.LearningRoadmap.EstimatedTimeInMonths} months");

                foreach (var step in result.LearningRoadmap.Steps)
                {
                    col.Item().Border(1).Padding(10).Column(stepCol =>
                    {
                        stepCol.Item().Text(step.Title).SemiBold();
                        stepCol.Item().Text(step.Description);
                        stepCol.Item().Text($"Resources: {string.Join(", ", step.Resources)}");
                        stepCol.Item().Text($"Estimated Time: {step.EstimatedTimeInWeeks} weeks");
                    });
                    col.Item().Height(10);
                }
            });

            // Recommended Courses Section
            column.Item().PaddingVertical(10).Column(col =>
            {
                col.Item().Text("Recommended Courses").FontSize(16).SemiBold();
                foreach (var course in result.RecommendedCourses)
                {
                    col.Item().Text($"• {course}");
                }
            });
        });
    }
}