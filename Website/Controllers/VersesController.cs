namespace Website.Controllers
{
    using Church.BibleStudyFellowship.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("bible/{culture}/[controller]")]
    public class VersesController : Controller
    {
        private readonly IRepository repository;

        public VersesController(IRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("Pattern")]
        public string GetPattern(string culture)
        {
            var verseLocator = VerseLocator.Create(repository.GetBibleBooks(culture));
            return verseLocator.GetPattern();
        }
    }
}
