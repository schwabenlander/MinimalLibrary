using MinimalLibrary.Api.Data;
using MinimalLibrary.Api.Models;
using MinimalLibrary.Api.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.Json;
using MinimalLibrary.Api.Auth;
using MinimalLibrary.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.IncludeFields = true;
});

builder.Configuration.AddJsonFile("appsettings.Local.json", true, true);

// Add services to the container.
builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>(_ => 
    new SqliteConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<IBookService, BookService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Add a book
app.MapPost("books", 
    //[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)] 
    async (Book book, IBookService bookService, IValidator<Book> validator) => 
{
    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var created = await bookService.CreateAsync(book);

    if (!created)
        return Results.BadRequest(
            new List<ValidationFailure>
            { 
                new("Isbn", "A book with this ISBN already exists.") 
            });

    return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
})
.WithName("CreateBook")
.Accepts<Book>("application/json")
.Produces<Book>(201)
.Produces<IEnumerable<ValidationFailure>>(400)
.WithTags("Books");

// Get books
app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
{
    IEnumerable<Book> books = Enumerable.Empty<Book>();

    if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        books = await bookService.SearchByTitleAsync(searchTerm);
    else
        books = await bookService.GetAllAsync();

    return Results.Ok(books);
})
.WithName("GetBooks")
.Produces<IEnumerable<Book>>()
.WithTags("Books");

// Get a book by ISBN
app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) => 
{
    var book = await bookService.GetByIsbnAsync(isbn);

    return book is not null ? Results.Ok(book) : Results.NotFound();
})
.WithName("GetBook")
.Produces<Book>()
.Produces(404)
.WithTags("Books");

// Update a book
app.MapPut("books/{isbn}", async (string isbn, Book book, IBookService bookService, IValidator<Book> validator) =>
{
    book.Isbn = isbn;

    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var updated = await bookService.UpdateAsync(book);

    return updated ? Results.Ok(book) : Results.NotFound();
})
.WithName("UpdateBook")
.Accepts<Book>("application/json")
.Produces<Book>()
.Produces<IEnumerable<ValidationFailure>>(400)
.WithTags("Books");

// Delete a book
app.MapDelete("books/{isbn}", async (string isbn, IBookService bookService) => 
{
    var deleted = await bookService.DeleteAsync(isbn);

    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteBook")
.Produces(204)
.Produces(404)
.WithTags("Books");

app.MapGet("status", () => Results.Extensions.Html(@"
            <html>
                <head>
                    <title>Status Page</title>
                </head>
                <body>
                    <h1>Status</h1>
                    <p>The server is configured correctly and functional.</p>
                </body>
            </html>
        "))
    .ExcludeFromDescription();

// Initialize database
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
