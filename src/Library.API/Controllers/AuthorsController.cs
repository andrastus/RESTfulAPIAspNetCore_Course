using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    //[Route("api/[controller]")] //also works but can cause uri's to change when you change controller name from a refactor for instance
    public class AuthorsController : Controller
    {
        private ILibraryRepository libraryRepository;
        public AuthorsController(ILibraryRepository libraryRepository)
        {
            this.libraryRepository = libraryRepository;
        }

        [HttpGet(])
        public IActionResult GetAuthors()
        {
            var authorsFromRepository = libraryRepository.GetAuthors();

            var authors = new List<AuthorDto>();
            foreach (var author in authorsFromRepository)
            {
                authors.Add(new AuthorDto()
                {
                    Id = author.Id,
                    Name = $"{author.FirstName} {author.LastName}",
                    Genre = author.Genre,
                    Age = author.DateOfBirth.GetCurrentAge()
                });
            }

            return new JsonResult(authors);
        }
    }
}
