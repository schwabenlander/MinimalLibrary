using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using MinimalLibrary.Api.Models;
using Xunit;

namespace MinimalLibrary.Api.Tests.Integration;

public class LibraryEndpointsTests 
    : IClassFixture<LibraryApiFactory>,
        IAsyncLifetime
{
    private readonly LibraryApiFactory _factory;
    private readonly List<string> _createdIsbns = new();

    public LibraryEndpointsTests(LibraryApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var createdBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location.Should().Be($"{httpClient.BaseAddress}books/{book.Isbn}");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenIsbnIsInvalid()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        book.Isbn = "INVALID_ISBN";
        
        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Value was not a valid ISBN-13");
    }
    
    [Fact]
    public async Task CreateBook_Fails_WhenBookAlreadyExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var result = await httpClient.PostAsJsonAsync("/books", book); // Duplicate
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("A book with this ISBN already exists");
    }

    [Fact]
    public async Task GetBook_ReturnsBook_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        
        // Act
        var result = await httpClient.GetAsync($"books/{book.Isbn}");
        var existingBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBook.Should().BeEquivalentTo(book);
    }
    
    [Fact]
    public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();
        
        // Act
        var result = await httpClient.GetAsync($"books/{isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBooks_ReturnsAllBooks_WhenBooksExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book1 = GenerateBook();
        var book2 = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book1);
        await httpClient.PostAsJsonAsync("/books", book2);
        _createdIsbns.Add(book1.Isbn);
        _createdIsbns.Add(book2.Isbn);
        var books = new List<Book>{ book1, book2 };
        
        // Act
        var result = await httpClient.GetAsync("/books");
        var existingBooks = await result.Content.ReadFromJsonAsync<List<Book>>();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBooks.Should().BeEquivalentTo(books);
    }
    
    [Fact]
    public async Task GetBooks_ReturnsNoBooks_WhenNoBooksExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();

        // Act
        var result = await httpClient.GetAsync("/books");
        var existingBooks = await result.Content.ReadFromJsonAsync<List<Book>>();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBooks.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchBooks_ReturnsBooks_WhenTitleMatches()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book>{ book };
        
        // Act
        var result = await httpClient.GetAsync("/books?searchTerm=dirty");
        var existingBooks = await result.Content.ReadFromJsonAsync<List<Book>>();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBooks.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task UpdateBook_UpdatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        
        // Act
        book.PageCount = 69;
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        var updatedBook = await result.Content.ReadFromJsonAsync<Book>();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedBook.Should().BeEquivalentTo(book);
    }
    
    [Fact]
    public async Task UpdateBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task UpdateBook_DoesNotUpdateBook_WhenDataIsIncorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        
        // Act
        book.Title = String.Empty;
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Title");
        error.ErrorMessage.Should().Be("Title cannot be empty");
    }
    
    [Fact]
    public async Task DeleteBook_DeletesBook_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var result = await httpClient.DeleteAsync($"/books/{book.Isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var isbn = GenerateIsbn();

        // Act
        var result = await httpClient.DeleteAsync($"/books/{isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private Book GenerateBook(string title = "The Dirty Coder")
    {
        return new Book
        {
            Isbn = GenerateIsbn(),
            Title = title,
            Author = "Nick Chapsas",
            ShortDescription = "All Nick's tricks in one book.",
            PageCount = 420,
            ReleaseDate = new DateTime(2023, 06, 09)
        };
    }

    private string GenerateIsbn()
    {
        return $"{Random.Shared.Next(100, 999)}-" +
               $"{Random.Shared.Next(1000000000, 2100999999)}";
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();
        foreach (var createdIsbn in _createdIsbns)
        {
            await httpClient.DeleteAsync($"/books/{createdIsbn}");
        }
    }
}