namespace Website.Controllers
{
    using Church.BibleStudyFellowship.Models;
    using Church.BibleStudyFellowship.Models.GetBible;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;

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

        [HttpGet("{passage}")]
        public Task<IEnumerable<BibleChapter>> ByAsync(string culture, string passage)
        {
            var bibleClient = new BibleClient();
            return bibleClient.GetVersesAsync(passage);
        }
    }
}
