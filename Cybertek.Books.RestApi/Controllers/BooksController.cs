using AutoMapper;
using Cybertek.Apis.Common.Responses;
using Cybertek.Books.Services;
using Cybertek.Paging;
using Microsoft.AspNetCore.Mvc;

namespace Cybertek.Books.RestApi.Controllers
{
    [ApiController]
    [Route("/api/books")]
    public class BooksController : ControllerBase
    {
        private readonly IBooksService _booksService;
        private readonly IMapper _mapper;

        public BooksController(IBooksService booksService,
            IMapper mapper,
            IPaginator paginator)
        {
            _booksService = booksService;
            _mapper = mapper;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetMany()
        {
            IList<Domains.Book> result = await _booksService.GetMany(new Paginator());
            return Ok(result);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            Domains.Book? result = await _booksService.GetById(id, cancellationToken);
            return result != null ? ResponseBuilder.Ok(result) : ResponseBuilder.NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Book request, CancellationToken cancellationToken = default)
        {
            var domainRequest = _mapper.Map<Domains.Book>(request);
            Guid? result = await _booksService.Post(domainRequest, cancellationToken);
            return result != null ? ResponseBuilder.Ok(result) : ResponseBuilder.BadRequest();
        }
    }
}
