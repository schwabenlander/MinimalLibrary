using FluentValidation;
using FluentValidation.Results;
using MinimalLibrary.Api.Extensions;
using MinimalLibrary.Api.Models;
using MinimalLibrary.Api.Services;

namespace MinimalLibrary.Api.Endpoints
{
    public static partial class LibraryEndpoints
    {
        public static void AddLibraryEndpoints(this IServiceCollection services)
        {
            services.AddSingleton<IBookService, BookService>();
        }

        public static void UseLibraryEndpoints(this IEndpointRouteBuilder app)
        {
            // Add a book
            app.MapPost("books", AddBookAsync)
                .WithName("CreateBook")
                .Accepts<Book>("application/json")
                .Produces<Book>(201)
                .Produces<IEnumerable<ValidationFailure>>(400)
                .WithTags("Books");

            // Get books
            app.MapGet("books", GetBooksAsync)
                .WithName("GetBooks")
                .Produces<IEnumerable<Book>>()
                .WithTags("Books")
                .RequireCors("AnyOrigin");

            // Get a book by ISBN
            app.MapGet("books/{isbn}", GetBookByIsbnAsync)
                .WithName("GetBook")
                .Produces<Book>()
                .Produces(404)
                .WithTags("Books")
                .RequireCors("AnyOrigin");

            // Update a book
            app.MapPut("books/{isbn}",
                    UpdateBookAsync)
                .WithName("UpdateBook")
                .Accepts<Book>("application/json")
                .Produces<Book>()
                .Produces<IEnumerable<ValidationFailure>>(400)
                .WithTags("Books");

            // Delete a book
            app.MapDelete("books/{isbn}", DeleteBookAsync)
                .WithName("DeleteBook")
                .Produces(204)
                .Produces(404)
                .WithTags("Books");
        }
    }
}