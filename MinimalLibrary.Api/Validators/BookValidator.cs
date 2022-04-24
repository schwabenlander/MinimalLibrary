using FluentValidation;
using MinimalLibrary.Api.Models;

namespace MinimalLibrary.Api.Validators;

public class BookValidator : AbstractValidator<Book>
{
    public BookValidator()
    {
        // Isbn
        RuleFor(book => book.Isbn)
            .Matches(@"^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$")
            .WithMessage("Value was not a valid ISBN-13");

        // Title
        RuleFor(book => book.Title).NotEmpty().WithMessage("Title cannot be empty");

        // Author
        RuleFor(book => book.Author).NotEmpty().WithMessage("Author cannot be empty");

        // ShortDescription
        RuleFor(book => book.ShortDescription).NotEmpty().WithMessage("Short Description cannot be empty");

        // PageCount
        RuleFor(book => book.PageCount).GreaterThan(0).WithMessage("Invalid PageCount value");

        // ReleaseDate
        RuleFor(book => book.Title).NotNull().WithMessage("ReleaseDate must be specified");
    }
}
