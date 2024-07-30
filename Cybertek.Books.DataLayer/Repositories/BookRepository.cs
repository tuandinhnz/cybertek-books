using Cybertek.Books.Domains;
using Cybertek.Repository;
using Microsoft.EntityFrameworkCore;

namespace Cybertek.Books.DataLayer.Repositories;

public class BookRepository : RepositoryBase<Book>
{
    public BookRepository(DbContext dbContext) : base(dbContext)
    {
    }
}
