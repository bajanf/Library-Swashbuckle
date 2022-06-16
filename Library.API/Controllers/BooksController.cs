using AutoMapper;
using Library.API.Attributes;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
   
    [Produces("application/json","application/xml")]
    [Route("api/authors/{authorId}/books")]
    [ApiController]
    // set up response type at controller level in order to not mention them for each action,
    // !!!a global one can be set in Startup.cs -> ConfigureServices > .AddMvc
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class BooksController : ControllerBase
    { 
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public BooksController(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _mapper = mapper;
        }
       
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(
        Guid authorId )
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var booksFromRepo = await _bookRepository.GetBooksAsync(authorId); 
            return Ok(_mapper.Map<IEnumerable<Book>>(booksFromRepo));
        }
        /// <summary>
        /// Gets a book by id for a specific author
        /// </summary>
        /// <param name="authorId">The id of the book author</param>
        /// <param name="bookId">The id of the book</param>
        /// <returns>An action Result of type Book</returns>
        /// <response code = "200">Returns the requested book</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/vnd.marvin.book+json")]
        [RequestHeaderMatchesMediaTypeAttribute(HeaderNames.Accept,
            "application/json", 
            "application/vnd.marvin.book+json")]
        [HttpGet("{bookId}")]
        public async Task<ActionResult<Book>> GetBook(
            Guid authorId,
            Guid bookId)
        {
            if (! await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<Book>(bookFromRepo));
        }

        /// <summary>
        /// Gets a book by id for a specific author
        /// </summary>
        /// <param name="authorId">The id of the book author</param>
        /// <param name="bookId">The id of the book</param>
        /// <returns>An action Result of type BookWithConcatenatedAuthorName</returns>
        /// <response code = "200">Returns the requested BookWithConcatenatedAuthorName</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/vnd.marvin.bookWithConcatenatedAuthorName+json")]
        [RequestHeaderMatchesMediaTypeAttribute(HeaderNames.Accept,
            "application/json",
            "application/vnd.marvin.bookWithConcatenatedAuthorName+json")]
        [ApiExplorerSettings(IgnoreApi = true)] 
        [HttpGet("{bookId}")]
        public async Task<ActionResult<BookWithConcatenatedAuthorName>> GetBookWithConcatenatedAuthorName(
            Guid authorId,
            Guid bookId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<BookWithConcatenatedAuthorName>(bookFromRepo));
        }

        /// <summary>
        /// Create a book for a specific author
        /// </summary>
        /// <param name="authorId">The id of the book author</param>
        /// <param name="bookForCreation">The book to create</param>
        /// <returns>An AcctionResult of type Book</returns>
        /// <response code="422">Validation error</response>
        [HttpPost()]
        [Consumes("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status422UnprocessableEntity,
        //    Type = typeof(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary))]
        public async Task<ActionResult<Book>> CreateBook(
            Guid authorId,
            [FromBody] BookForCreation bookForCreation)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookToAdd = _mapper.Map<Entities.Book>(bookForCreation);
            _bookRepository.AddBook(bookToAdd);
            await _bookRepository.SaveChangesAsync();

            return CreatedAtRoute(
                "GetBook",
                new { authorId, bookId = bookToAdd.Id },
                _mapper.Map<Book>(bookToAdd));
        }
    }
}
