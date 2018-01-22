namespace Website.Controllers
{
    using Church.BibleStudyFellowship.Models;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [Route("material/{culture}/[controller]")]
    public class LessonsController : Controller
    {
        private readonly IRepository repository;

        public LessonsController(IRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("")]
        public Task<IEnumerable<Lesson>> Get(string culture)
        {
            return this.repository.GetLessonsAsync(culture);
        }

        [HttpGet("{id}")]
        public Task<Lesson> GetBy(string culture, string id)
        {
            return this.repository.GetLessonAsync(culture, id);
        }
    }
}
