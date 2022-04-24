namespace MinimalLibrary.Api.Models;

public class Book
{
    public string Isbn { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Author { get; set; } = default!;

    public string ShortDescription { get; set; } = default!;

    public int PageCount { get; set; }

    public DateTime ReleaseDate { get; set; }
}
