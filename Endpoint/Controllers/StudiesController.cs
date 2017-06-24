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

    public class StudiesController : ApiController
    {
        private readonly IRepository repository;

        public StudiesController(IRepository repository)
        {
            this.repository = repository;
        }

        // GET api/values
        [SwaggerOperation("GetAll")]
        public IEnumerable<Study> Get(string culture = null)
        {
            // TODO Set zh-CN as default
            return this.repository.GetStudies(culture ?? "zh-CN");
        }

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string Get(int id)
        {
            return "value";
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
