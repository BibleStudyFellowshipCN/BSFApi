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

    public class FeedbackController : ApiController
    {
        private readonly IRepository repository;

        public FeedbackController(IRepository repository)
        {
            this.repository = repository;
        }

        // GET api/values
        [SwaggerOperation("GetAll")]
        public IEnumerable<Feedback> Get()
        {
            return this.repository.GetFeedback();
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SelfAuthorizeAttribute]
        public void Post([FromBody]Feedback feedback)
        {
            this.repository.AddFeedbackAsync(feedback).Wait();
        }
    }
}
