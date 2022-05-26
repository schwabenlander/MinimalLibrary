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
}