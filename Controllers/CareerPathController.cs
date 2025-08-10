using CareerPathPlannerAI.Models;
using CareerPathPlannerAI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CareerPathPlannerAI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Pdf)]
public class CareerPathController : ControllerBase
{
    private readonly ICareerAnalysisService _careerAnalysisService;
    private readonly IPdfGenerationService _pdfGenerationService;
    private readonly ILogger<CareerPathController> _logger;

    public CareerPathController(
        ICareerAnalysisService careerAnalysisService,
        IPdfGenerationService pdfGenerationService,
        ILogger<CareerPathController> logger)
    {
        _careerAnalysisService = careerAnalysisService;
        _pdfGenerationService = pdfGenerationService;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes career path and returns either JSON or PDF based on Accept header
    /// </summary>
    /// <param name="userProfile">User profile information</param>
    /// <returns>Career analysis result in JSON or PDF format</returns>
    /// <response code="200">Returns the career analysis result</response>
    /// <response code="400">If the userProfile is invalid</response>
    /// <response code="500">If there was an internal error</response>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(CareerAnalysisResult), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, MediaTypeNames.Application.Pdf)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeCareerPath([FromBody] UserProfile userProfile)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _careerAnalysisService.AnalyzeCareerPath(userProfile);

            // Check Accept header for PDF request
            if (Request.Headers.Accept.Any(x => x.Contains(MediaTypeNames.Application.Pdf)))
            {
                var pdfBytes = _pdfGenerationService.GenerateCareerReport(result, userProfile);
                return File(
                    pdfBytes,
                    MediaTypeNames.Application.Pdf,
                    $"career-analysis-{DateTime.Now:yyyy-MM-dd}.pdf"
                );
            }

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                "Unable to process request due to an error with the AI service");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing career path");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while analyzing career path");
        }
    }
}