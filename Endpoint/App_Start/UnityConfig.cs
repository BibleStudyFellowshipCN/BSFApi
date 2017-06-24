namespace Church.BibleStudyFellowship.Endpoint
{
    using System.Web.Http;

    using Church.BibleStudyFellowship.Models;
    using Microsoft.Practices.Unity;
    using Unity.WebApi;
    using System.Web.Configuration;

    internal static class UnityConfig
    {
        public static void RegisterComponents(HttpConfiguration config)
        {
			var container = new UnityContainer();
            var connectionString = WebConfigurationManager.AppSettings["ConnectionString"];
            var repository = BibleStudyFellowship.Models.Storage.Repository.Create(connectionString);
            container.RegisterInstance<IRepository>(repository);
            config.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}