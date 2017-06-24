namespace Church.BibleStudyFellowship.Endpoint.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SelfAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var requestHeader = actionContext.Request.Headers;
            var authorizationHeader = requestHeader.Authorization;

            if(authorizationHeader==null)
            {
                return false;
            }

            var isAuthorized = false;
            switch(authorizationHeader.Scheme)
            {
                case "Basic":
                    var data = Convert.FromBase64String(authorizationHeader.Parameter);
                    var parts = Encoding.UTF8.GetString(data).Split(':');
                    isAuthorized = parts.Length == 2;
                    break;
                default:
                    break;
            }

            ////IEnumerable<string> values;
            ////requestHeader.TryGetValues("Church-Mobile", out values);

            return isAuthorized;
        }
    }
}
