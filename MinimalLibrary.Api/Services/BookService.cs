using Dapper;
using MinimalLibrary.Api.Data;
using MinimalLibrary.Api.Models;

namespace MinimalLibrary.Api.Services
{
    public class BookService : IBookService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public BookService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> CreateAsync(Book book)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var result = await connection.ExecuteAsync(
                @"
                INSERT INTO Books (Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate)
                VALUES (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate);
                ", book);

            return result > 0;
        }

        public Task<bool> DeleteAsync(string isbn)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var result = await connection.QueryAsync<Book>(
                @"
                SELECT Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate
                FROM Books
                ORDER BY Title;
                ");

            return result;
        }

        public Task<Book?> GetByIsbnAsync(string isbn)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(Book book)
        {
            throw new NotImplementedException();
        }
    }
}
