using MinimalLibrary.Api.Data;
using MinimalLibrary.Api.Models;
using MinimalLibrary.Api.Services;
using MinimalLibrary.Api.Validators;
using FluentValidation;
using FluentValidation.Results;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Map redirect to Swagger
if (app.Environment.IsDevelopment())
    app.MapGet("/", () => Results.Redirect("swagger"));

// Add a book
app.MapPost("books", async (Book book, IBookService bookService, IValidator<Book> validator) => 
{
    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var created = await bookService.CreateAsync(book);

    if (!created)
        return Results.BadRequest(
            new List<ValidationFailure>
            { 
                new ValidationFailure("Isbn", "A book with this ISBN already exists.") 
            });

    return Results.Created($"/books/{book.Isbn}", book);
});

app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
{
    IEnumerable<Book> books = Enumerable.Empty<Book>();

    if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
        books = await bookService.SearchByTitleAsync(searchTerm);
    else
        books = await bookService.GetAllAsync();

    return Results.Ok(books);
});

app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) => 
{
    var book = await bookService.GetByIsbnAsync(isbn);

    return book is not null ? Results.Ok(book) : Results.NotFound();
});

// Initialize database
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
