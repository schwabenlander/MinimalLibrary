using FluentValidation;
using FluentValidation.Results;
using MinimalLibrary.Api.Extensions;
using MinimalLibrary.Api.Models;
using MinimalLibrary.Api.Services;

namespace MinimalLibrary.Api.Endpoints
{
    public static partial class LibraryEndpoints
    {
        private const string ContentType = "application/json";
        private const string Tag = "Books";
        private const string BaseRoute = "books";

        public static void AddLibraryEndpoints(this IServiceCollection services)
        {
            services.AddSingleton<IBookService, BookService>();
        }

        public static void UseLibraryEndpoints(this WebApplication app)
        {
            // Map redirect to Swagger
            if (app.Environment.IsDevelopment())
                app.MapGet("/", () => Results.Redirect("swagger"))
                    .ExcludeFromDescription();
            
            // Add a book
            app.MapPost(BaseRoute, AddBookAsync)
                .WithName("CreateBook")
                .Accepts<Book>(ContentType)
                .Produces<Book>(201)
                .Produces<IEnumerable<ValidationFailure>>(400)
                .WithTags(Tag);

            // Get books
            app.MapGet(BaseRoute, GetBooksAsync)
                .WithName("GetBooks")
                .Produces<IEnumerable<Book>>()
                .WithTags(Tag)
                .RequireCors("AnyOrigin");

            // Get a book by ISBN
            app.MapGet($"{BaseRoute}/{{isbn}}", GetBookByIsbnAsync)
                .WithName("GetBook")
                .Produces<Book>()
                .Produces(404)
                .WithTags(Tag)
                .RequireCors("AnyOrigin");

            // Update a book
            app.MapPut($"{BaseRoute}/{{isbn}}", UpdateBookAsync)
                .WithName("UpdateBook")
                .Accepts<Book>(ContentType)
                .Produces<Book>()
                .Produces<IEnumerable<ValidationFailure>>(400)
                .WithTags(Tag);

            // Delete a book
            app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync)
                .WithName("DeleteBook")
                .Produces(204)
                .Produces(404)
                .WithTags(Tag);
        }
    }
}