using MinimalLibrary.Api.Data;
using MinimalLibrary.Api.Models;
using MinimalLibrary.Api.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.Json;
using MinimalLibrary.Api.Auth;
using MinimalLibrary.Api.Extensions;
using MinimalLibrary.Api.Validators;
using System.Text.Json;
using MinimalLibrary.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", true, true);

// Add and configure services.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AnyOrigin", policy => policy.AllowAnyOrigin());
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>(_ => 
    new SqliteConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddLibraryEndpoints();
builder.Services.AddValidatorsFromAssemblyContaining<BookValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map redirect to Swagger
if (app.Environment.IsDevelopment())
    app.MapGet("/", () => Results.Redirect("swagger"))
        .ExcludeFromDescription();

// Use Library endpoints
app.UseLibraryEndpoints();

// Initialize database
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
