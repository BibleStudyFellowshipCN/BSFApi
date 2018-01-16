namespace Website.Controllers
{
    using Church.BibleStudyFellowship.Models;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    [Route("material/{culture}/[controller]")]
    public class LessonsController : Controller
    {
        private readonly IRepository repository;

        public LessonsController(IRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("")]
        public IEnumerable<Lesson> By(string culture)
        {
            return this.repository.GetLessons(culture);
        }

        [HttpGet("{id}")]
        public Lesson By(string culture, string id)
        {
            return this.repository.GetLesson(culture, id);
        }
    }
}
