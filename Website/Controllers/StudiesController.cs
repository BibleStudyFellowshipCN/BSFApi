namespace Website.Controllers
{
    using Church.BibleStudyFellowship.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("material/{culture}/[controller]")]
    public class StudiesController : Controller
    {
        private readonly IRepository repository;

        public StudiesController(IRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("{title}")]
        public Study By(string culture, string title)
        {
            return this.repository.GetStudy(culture, title);
        }
    }
}
