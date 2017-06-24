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

    [SelfAuthorizeAttribute]
    public class LessonsController : ApiController
    {
        private readonly IRepository repository;

        public LessonsController(IRepository repository)
        {
            this.repository = repository;
        }

        // GET api/values
        [SwaggerOperation("GetAll")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public Lesson Get(string id, string culture = null)
        {
            return this.repository.GetLesson(culture ?? "zh-CN", id);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}
