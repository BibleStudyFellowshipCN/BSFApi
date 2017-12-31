namespace Website.Controllers
{
    using Church.BibleStudyFellowship.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("bible/{culture}/[controller]")]
    public class VersesController : Controller
    {
        private readonly IRepository repository;

        public VersesController()
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=bsfmaterial;AccountKey=mCantqlbmZavlKEEhaWVXWxXBwf3+AfLeSgEX3zVkEtTgq7B2xAs8lNmm08nSF//HfY3BijIMGWCNQUpNo2tvg==;EndpointSuffix=core.windows.net";
            this.repository = Church.BibleStudyFellowship.Models.Storage.Repository.Create(connectionString);
        }

        [HttpGet("Pattern")]
        public string GetPattern(string culture)
        {
            var verseLocator = VerseLocator.Create(repository.GetBibleBooks(culture));
            return verseLocator.GetPattern();
        }
    }
}
