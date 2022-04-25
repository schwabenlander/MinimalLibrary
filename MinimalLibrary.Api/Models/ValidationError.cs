namespace MinimalLibrary.Api.Models;

public class ValidationError
{
    public string PropertyName { get; set; } = default!;

    public string ErrorMessage { get; set; } = default!;
}
