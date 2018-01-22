namespace Website.Controllers
{
    using Church.BibleStudyFellowship.Models;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Route("material/{culture}/[controller]")]
    public class StudiesController : Controller
    {
        private readonly IRepository repository;

        public StudiesController(IRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("")]
        public Task<IEnumerable<Study>> Get(string culture)
        {
            return this.repository.GetStudiesAsync(culture);
        }

        [HttpGet("{title}")]
        public Task<Study> GetBy(string culture, string title)
        {
            return this.repository.GetStudyAsync(culture, title);
        }
    }
}
