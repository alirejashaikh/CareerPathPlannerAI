using CareerPathPlannerAI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to show PDF response option
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Career Path Planner API",
        Version = "v1",
        Description = "API for career path analysis with PDF export capability"
    });

    // Configure response content types
    options.MapType<FileContentResult>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

// Add HttpClient factory for Gemini
builder.Services.AddHttpClient("Gemini", client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register services
builder.Services.AddScoped<ICareerAnalysisService, CareerAnalysisService>();
builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Career Path Planner API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
