using FluentValidation.Results;
using MinimalLibrary.Api.Models;

namespace MinimalLibrary.Api.Extensions;

public static class ValidationExtensions
{
    public static IEnumerable<ValidationError> ToErrorList(this List<ValidationFailure> failures) => 
        failures.Select(e => new ValidationError { ErrorMessage = e.ErrorMessage, PropertyName = e.PropertyName }).ToList();
}
