using FluentAssertions;
using Microsoft.Data.Sqlite;
using MinimalLibrary.Api.Data;
using MinimalLibrary.Api.Models;
using MinimalLibrary.Api.Repositories;
using MinimalLibrary.Api.Services;
using NSubstitute;

namespace MinimalLibrary.Api.Tests.Unit;

public class BookServiceTests
{
    private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();

    // System Under Test
    private readonly BookService _sut;

    public BookServiceTests()
    {
        _sut = new BookService(_bookRepository);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoBooksExist()
    {
        // Arrange
        _bookRepository.GetAllAsync().Returns(Enumerable.Empty<Book>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnBooks_WhenBooksExist()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            Isbn = "9876543210123",
            PageCount = 100,
            ReleaseDate = new DateTime(2000, 1, 1),
            ShortDescription = "This is a test book"
        };

        var expectedBooks = new[] {book};

        _bookRepository.GetAllAsync().Returns(expectedBooks);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedBooks);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateBook_WhenGivenValidBook()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            Isbn = "9876543210123",
            PageCount = 100,
            ReleaseDate = new DateTime(2000, 1, 1),
            ShortDescription = "This is a test book"
        };

        _bookRepository.CreateAsync(book).Returns(true);

        // Act
        var result = await _sut.CreateAsync(book);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteABook_WhenBookExists()
    {
        // Arrange
        var isbn = "9876543210123";
        _bookRepository.DeleteAsync(isbn).Returns(true);

        // Act
        var result = await _sut.DeleteAsync(isbn);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIsbnAsync_ShouldReturnBook_WhenBookExists()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            Isbn = "9876543210123",
            PageCount = 100,
            ReleaseDate = new DateTime(2000, 1, 1),
            ShortDescription = "This is a test book"
        };

        _bookRepository.GetByIsbnAsync(book.Isbn).Returns(book);

        // Act
        var result = await _sut.GetByIsbnAsync(book.Isbn);

        // Assert
        result.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBook_WhenGivenBook()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            Isbn = "9876543210123",
            PageCount = 100,
            ReleaseDate = new DateTime(2000, 1, 1),
            ShortDescription = "This is a test book"
        };

        _bookRepository.UpdateAsync(book).Returns(true);

        // Act
        var result = await _sut.UpdateAsync(book);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SearchByTitleAsync_ShouldReturnBooks_WhenBooksContainSearchTermInTitle()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            Isbn = "9876543210123",
            PageCount = 100,
            ReleaseDate = new DateTime(2000, 1, 1),
            ShortDescription = "This is a test book"
        };

        var expectedBooks = new[] { book };
        var searchTerm = "Test";

        _bookRepository.SearchByTitleAsync(searchTerm).Returns(expectedBooks);

        // Act
        var result = await _sut.SearchByTitleAsync(searchTerm);

        // Assert
        result.Should().BeEquivalentTo(expectedBooks);
    }
}