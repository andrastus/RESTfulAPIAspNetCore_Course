using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
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

        [HttpGet("{bookId}", Name = "GetBookForAuthor")]
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

        [HttpPost()]
        public IActionResult CreateBookForAuthor(Guid authorId, 
            [FromBody] BookForCreationDto book)
        {
            if (book == null)
                return BadRequest();

            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Book>(book);

            libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!libraryRepository.Save())
                throw new Exception($"Creating book for author {authorId} failed on save.");

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute("GetBookForAuthor",
                new { authorId = authorId, bookId = bookToReturn.Id },
                bookToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
                return NotFound();

            libraryRepository.DeleteBook(bookForAuthorFromRepo);
            if (!libraryRepository.Save())
                throw new Exception($"Deleting book {id} for author {authorId} failed on save.");

            return NoContent();
        }

        [HttpPut("{id}")] //Fully update a resource (all omitted field will be defaulted (null)
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
        {
            if (book == null)
                return BadRequest();

            if (!libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                //Upserting
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;

                libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!libraryRepository.Save())
                    throw new Exception($"Upserting book {id} for author {authorId} failed on save.");

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor",
                    new { authorId = authorId, id = bookToReturn.Id },
                    bookToReturn);
            }

            //map

            //apply update

            //map back to entity
            Mapper.Map(book, bookForAuthorFromRepo);

            libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!libraryRepository.Save())
                throw new Exception($"Update book {id} for author {authorId} failed on save.");

            return NoContent(); //could be Ok()
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id,
            [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            if (!libraryRepository.AuthorExists(authorId))
                return NotFound();

            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                var bookDto = new BookForUpdateDto();
                patchDoc.ApplyTo(bookDto);

                var bookToAd = Mapper.Map<Book>(bookDto);
                bookToAd.Id = id;

                libraryRepository.AddBookForAuthor(authorId, bookToAd);

                if (!libraryRepository.Save())
                    throw new Exception($"Upserting book {id} for author {authorId} failed on save.");

                var bookToReturn = Mapper.Map<BookDto>(bookToAd);

                return CreatedAtRoute("GetBookForAuthor",
                    new { authorId = authorId, bookId = bookToReturn.Id },
                    bookToReturn);
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

            patchDoc.ApplyTo(bookToPatch);

            /// TODO: add validation

            Mapper.Map(bookToPatch, bookForAuthorFromRepo);

            libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!libraryRepository.Save())
                throw new Exception($"Patching book {id} for author {authorId} failed on save.");

            return NoContent();
        }
    }
}
