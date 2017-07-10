namespace Church.BibleStudyFellowship.Endpoint.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Church.BibleStudyFellowship.Endpoint.Models;
    using Church.BibleStudyFellowship.Models;
    using Church.BibleStudyFellowship.Models.Storage;
    using Swashbuckle.Swagger.Annotations;

    public class LessonsController : ApiController
    {
        private readonly IRepository repository;

        public LessonsController(IRepository repository)
        {
            this.repository = repository;
        }

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public Lesson Get(string culture, string id)
        {
            return this.repository.GetLesson(culture, id);
        }
    }
}
