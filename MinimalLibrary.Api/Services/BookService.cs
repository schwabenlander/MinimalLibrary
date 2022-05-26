using Dapper;
using MinimalLibrary.Api.Data;
using MinimalLibrary.Api.Models;
using MinimalLibrary.Api.Repositories;

namespace MinimalLibrary.Api.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<bool> CreateAsync(Book book)
        {
            return await _bookRepository.CreateAsync(book);
        }

        public async Task<bool> DeleteAsync(string isbn)
        {
            return await _bookRepository.DeleteAsync(isbn);
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _bookRepository.GetAllAsync();
        }

        public async Task<Book?> GetByIsbnAsync(string isbn)
        {
            return await _bookRepository.GetByIsbnAsync(isbn);
        }

        public async Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
        {
            return await _bookRepository.SearchByTitleAsync(searchTerm);
        }

        public async Task<bool> UpdateAsync(Book book)
        {
            return await _bookRepository.UpdateAsync(book);
        }
    }
}
