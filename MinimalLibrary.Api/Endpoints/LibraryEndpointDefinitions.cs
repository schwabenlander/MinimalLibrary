using FluentValidation;
using MinimalLibrary.Api.Extensions;
using MinimalLibrary.Api.Models;
using MinimalLibrary.Api.Services;

namespace MinimalLibrary.Api.Endpoints;

public static partial class LibraryEndpoints
{
    private static async Task<IResult> AddBookAsync(Book book, IBookService bookService, IValidator<Book> validator)
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors.ToErrorList());

        var created = await bookService.CreateAsync(book);

        if (!created)
            return Results.BadRequest(new List<ValidationError>
            {
                new()
                {
                    PropertyName = "Isbn", ErrorMessage = "A book with this ISBN already exists"
                }
            });

        return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
    }

    private static async Task<IResult> UpdateBookAsync(string isbn, Book book, IBookService bookService,
        IValidator<Book> validator)
    {
        book.Isbn = isbn;

        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);

        var updated = await bookService.UpdateAsync(book);

        return updated ? Results.Ok(book) : Results.NotFound();
    }

    private static async Task<IResult> GetBookByIsbnAsync(string isbn, IBookService bookService)
    {
        var book = await bookService.GetByIsbnAsync(isbn);

        return book is not null ? Results.Ok(book) : Results.NotFound();
    }

    private static async Task<IResult> GetBooksAsync(IBookService bookService, string? searchTerm)
    {
        IEnumerable<Book> books = Enumerable.Empty<Book>();

        if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
            books = await bookService.SearchByTitleAsync(searchTerm);
        else
            books = await bookService.GetAllAsync();

        return Results.Ok(books);
    }

    private static async Task<IResult> DeleteBookAsync(string isbn, IBookService bookService)
    {
        var deleted = await bookService.DeleteAsync(isbn);

        return deleted ? Results.NoContent() : Results.NotFound();
    }
}