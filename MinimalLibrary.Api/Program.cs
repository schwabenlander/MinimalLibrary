using MinimalLibrary.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using MinimalLibrary.Api.Auth;
using MinimalLibrary.Api.Validators;
using System.Text.Json;
using MinimalLibrary.Api.Endpoints;
using MinimalLibrary.Api.Repositories;
using MinimalLibrary.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", true, true);

// Add and configure services.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin());
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
builder.Services.AddSingleton<IBookRepository, BookRepository>();
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

// Use Library endpoints
app.UseLibraryEndpoints();

// Initialize database
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
