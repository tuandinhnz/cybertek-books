using System.Linq.Expressions;
using System.Net;
using Cybertek.Books.Domains;
using Cybertek.Extensions.Exceptions;
using Cybertek.Paging;
using Cybertek.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Cybertek.Books.Services;

public class BooksService : IBooksService
{
    private readonly IRepository<Book> _booksRepository;

    public BooksService(IRepository<Book> booksRepository)
    {
        _booksRepository = booksRepository;
    }

    public async Task<IList<Book>> GetMany(IPaginator paginator, CancellationToken cancellationToken = default)
    {
        Func<IQueryable<Book>, IIncludableQueryable<Book, object>> includes
            = booksQueryable => booksQueryable
                .Include(b => b.AuthorsLink)
                .ThenInclude(al => al.Author);
        Expression<Func<Book, object>> orderBy = book => book.PublishedOn; 

        return await _booksRepository.FindMany(paginator, orderBy: orderBy, descending: true, includes: includes, cancellationToken: cancellationToken);
    }

    public async Task<Book?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _booksRepository.FindById(new object[]{ id }, cancellationToken);
    }

    public async Task<Guid?> Post(Book book,
        CancellationToken cancellationToken = default)
    {
        if (book.BookId == Guid.Empty)
        {
            book.BookId = Guid.NewGuid();
        }

        _booksRepository.Add(book);
        int createdCount = await _booksRepository.SaveChanges(cancellationToken);
        if (createdCount != 1)
        {
            throw ExceptionFactory.CreateException("Resource is not created", HttpStatusCode.InternalServerError);
        }

        return book.BookId;
    }
}
