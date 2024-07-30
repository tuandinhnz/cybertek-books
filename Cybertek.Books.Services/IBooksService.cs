using Cybertek.Books.Domains;
using Cybertek.Paging;

namespace Cybertek.Books.Services
{
    public interface IBooksService
    {
        Task<IList<Book>> GetMany(IPaginator paginator,
            CancellationToken cancellationToken = default);

        Task<Book?> GetById(Guid id,
            CancellationToken cancellationToken = default);

        Task<Guid?> Post(Book book,
            CancellationToken cancellationToken = default);
    }
}
