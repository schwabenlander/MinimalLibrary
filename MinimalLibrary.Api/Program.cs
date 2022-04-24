using MinimalLibrary.Api.Data;
using MinimalLibrary.Api.Models;
using MinimalLibrary.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>(_ => 
    new SqliteConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<IBookService, BookService>();

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
app.MapPost("books", async (Book book, IBookService bookService) => 
{
    var created = await bookService.CreateAsync(book);

    if (!created)
    {
        return Results.BadRequest(
            new { errorMessage = "A book with this ISBN already exists." });
    }

    return Results.Created($"/books/{book.Isbn}", book);
});

// Initialize database
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
