using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            this.libraryRepository = libraryRepository;
        }

        [HttpGet("")]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorFromRepository = libraryRepository.GetBooksForAuthor(authorId);
            var booksForAuthor = AutoMapper.Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepository);

            return Ok(booksForAuthor);
        }

        [HttpGet("{bookId}")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid bookId)
        {
            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookFromRepository = libraryRepository.GetBookForAuthor(authorId, bookId);
            if (bookFromRepository == null)
                return NotFound();

            var bookForAuthor = AutoMapper.Mapper.Map<BookDto>(bookFromRepository);
            return Ok(bookForAuthor);
        }
    }
}
