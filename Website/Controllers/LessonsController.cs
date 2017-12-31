namespace Website.Controllers
{
    using Church.BibleStudyFellowship.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("material/{culture}/[controller]")]
    public class LessonsController : Controller
    {
        private readonly IRepository repository;

        public LessonsController(IRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("{id}")]
        public Lesson By(string culture, string id)
        {
            return this.repository.GetLesson(culture, id);
        }
    }
}
