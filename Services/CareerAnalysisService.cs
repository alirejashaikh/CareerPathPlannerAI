using CareerPathPlannerAI.Models;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CareerPathPlannerAI.Services;

public interface ICareerAnalysisService
{
    Task<CareerAnalysisResult> AnalyzeCareerPath(UserProfile userProfile);
}

public class CareerAnalysisService : ICareerAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _geminiApiUrl;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public CareerAnalysisService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _apiKey = configuration["Gemini:ApiKey"] ?? 
            throw new ArgumentNullException("Gemini:ApiKey", "Gemini API key not found in configuration");
        _geminiApiUrl = configuration["Gemini:ApiUrl"] ??
            throw new ArgumentNullException("Gemini:ApiUrl", "Gemini API URL not found in configuration");
        _httpClient = httpClientFactory.CreateClient("Gemini");
    }

    public async Task<CareerAnalysisResult> AnalyzeCareerPath(UserProfile userProfile)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = CreatePrompt(userProfile) }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.7,
                topK = 40,
                topP = 0.95,
                maxOutputTokens = 2048
            }
        };

        var url = $"{_geminiApiUrl}?key={_apiKey}";
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        
        using var response = await _httpClient.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                GetErrorMessage(responseString),
                null,
                response.StatusCode
            );
        }

        return ParseGeminiResponse(responseString);
    }

    private static string GetErrorMessage(string responseString)
    {
        try
        {
            using var document = JsonDocument.Parse(responseString);
            return document.RootElement
                .GetProperty("error")
                .GetProperty("message")
                .GetString() ?? "Unknown error";
        }
        catch
        {
            return responseString;
        }
    }

    private static string CreatePrompt(UserProfile userProfile) => 
$@"As an AI career advisor, analyze the following user profile and provide career recommendations in JSON format.

User Profile:
{JsonSerializer.Serialize(userProfile, _jsonOptions)}

Return ONLY a JSON object with these properties:
{{
    ""RecommendedPaths"": [
        {{ 
            ""Title"": ""string"",
            ""RequiredSkills"": [""string""],
            ""RecommendedCertifications"": [""string""],
            ""Description"": ""string"",
            ""AverageSalary"": 0,
            ""JobMarketOutlook"": ""string""
        }}
    ],
    ""SkillGaps"": [""string""],
    ""RecommendedCourses"": [""string""],
    ""LearningRoadmap"": {{
        ""Steps"": [
            {{
                ""Title"": ""string"",
                ""Description"": ""string"",
                ""Resources"": [""string""],
                ""EstimatedTimeInWeeks"": 0
            }}
        ],
        ""EstimatedTimeInMonths"": 0
    }}
}}";

    private static CareerAnalysisResult ParseGeminiResponse(string responseString)
    {
        try
        {
            using var document = JsonDocument.Parse(responseString);
            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Empty response from Gemini");

            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
                throw new InvalidOperationException("No valid JSON object found in response");

            var json = text.Substring(jsonStart, jsonEnd - jsonStart + 1);
            var analysisResult = JsonSerializer.Deserialize<CareerAnalysisResult>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                }
            );

            return analysisResult ?? throw new InvalidOperationException("Failed to parse Gemini response");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error parsing Gemini response: {ex.Message}", ex);
        }
    }
}